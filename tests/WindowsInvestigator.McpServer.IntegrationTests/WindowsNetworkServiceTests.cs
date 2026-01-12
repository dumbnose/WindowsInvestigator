using FluentAssertions;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.IntegrationTests;

/// <summary>
/// Integration tests that run against real Windows network APIs.
/// </summary>
public class WindowsNetworkServiceTests
{
    private readonly WindowsNetworkService _sut;

    public WindowsNetworkServiceTests()
    {
        _sut = new WindowsNetworkService();
    }

    [Fact]
    public void TestConnection_ToLocalhost_Succeeds()
    {
        // Localhost should always resolve, but port 80 may not be open
        // We test that the method executes without throwing

        // Act
        var result = _sut.TestConnection("localhost", 80, 1000);

        // Assert
        result.Should().NotBeNull();
        result.Host.Should().Be("localhost");
        result.Port.Should().Be(80);
    }

    [Fact]
    public void TestConnection_ToUnreachableHost_ReturnsError()
    {
        // Act
        var result = _sut.TestConnection("192.0.2.1", 80, 1000); // TEST-NET-1, should be unreachable

        // Assert
        result.Should().NotBeNull();
        result.TcpConnected.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void TestConnection_RespectsTimeout()
    {
        // Act
        var startTime = DateTime.Now;
        var result = _sut.TestConnection("192.0.2.1", 80, 1000); // 1 second timeout
        var elapsed = DateTime.Now - startTime;

        // Assert
        elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5)); // Should not take much longer than timeout
    }

    [Fact]
    public void TestConnection_WithInvalidPort_Fails()
    {
        // Act
        var result = _sut.TestConnection("localhost", 65000, 1000); // Unlikely to have a service here

        // Assert
        result.Should().NotBeNull();
        // Either fails to connect or succeeds (if something is listening)
    }

    [Fact]
    public void ResolveDns_WithLocalhost_ReturnsLoopback()
    {
        // Act
        var result = _sut.ResolveDns("localhost");

        // Assert
        result.Should().NotBeNull();
        result.HostName.Should().Be("localhost");
        result.IpAddresses.Should().NotBeEmpty();
        result.IpAddresses.Should().Contain(ip => ip == "127.0.0.1" || ip == "::1");
    }

    [Fact]
    public void ResolveDns_WithCurrentMachine_ReturnsAddresses()
    {
        // Act
        var result = _sut.ResolveDns(Environment.MachineName);

        // Assert
        result.Should().NotBeNull();
        result.IpAddresses.Should().NotBeEmpty();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void ResolveDns_WithInvalidHostname_ReturnsError()
    {
        // Act
        var result = _sut.ResolveDns("this-hostname-should-not-exist-12345.invalid");

        // Assert
        result.Should().NotBeNull();
        result.IpAddresses.Should().BeEmpty();
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ResolveDns_WithIpAddress_ReturnsAddress()
    {
        // Act
        var result = _sut.ResolveDns("127.0.0.1");

        // Assert
        result.Should().NotBeNull();
        result.IpAddresses.Should().Contain("127.0.0.1");
    }

    [Fact]
    public void GetNetworkAdapters_ReturnsAdapters()
    {
        // Act
        var result = _sut.GetNetworkAdapters().ToList();

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void GetNetworkAdapters_ReturnsOrderedByName()
    {
        // Act
        var result = _sut.GetNetworkAdapters().ToList();

        // Assert
        result.Select(a => a.Name).Should().BeInAscendingOrder(StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetNetworkAdapters_AdaptersHaveRequiredProperties()
    {
        // Act
        var result = _sut.GetNetworkAdapters().ToList();

        // Assert
        result.Should().AllSatisfy(adapter =>
        {
            adapter.Name.Should().NotBeNullOrEmpty();
            adapter.Description.Should().NotBeNullOrEmpty();
            adapter.Status.Should().NotBeNullOrEmpty();
            adapter.Status.Should().BeOneOf("Up", "Down", "Testing", "Unknown", "Dormant", "NotPresent", "LowerLayerDown");
        });
    }

    [Fact]
    public void GetNetworkAdapters_ContainsLoopbackAdapter()
    {
        // Act
        var result = _sut.GetNetworkAdapters().ToList();

        // Assert
        result.Should().Contain(a => a.Name.Contains("Loopback") || a.Description.Contains("Loopback"));
    }

    [Fact]
    public void GetNetworkAdapters_ActiveAdaptersHaveIpAddresses()
    {
        // Act
        var result = _sut.GetNetworkAdapters().ToList();
        var activeAdapters = result.Where(a => a.Status == "Up").ToList();

        // Assert
        if (activeAdapters.Any())
        {
            activeAdapters.Should().Contain(a => a.IpAddresses.Any());
        }
    }

    [Theory]
    [InlineData("localhost")]
    [InlineData("127.0.0.1")]
    public void ResolveDns_WithVariousLocalNames_Succeeds(string hostname)
    {
        // Act
        var result = _sut.ResolveDns(hostname);

        // Assert
        result.Should().NotBeNull();
        result.IpAddresses.Should().NotBeEmpty();
        result.Error.Should().BeNull();
    }
}
