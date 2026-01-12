using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WindowsInvestigator.McpServer.Exceptions;
using WindowsInvestigator.McpServer.Services;
using WindowsInvestigator.McpServer.Tools;

namespace WindowsInvestigator.McpServer.Tests;

public class NetworkToolsTests
{
    private readonly INetworkService _networkService;
    private readonly NetworkTools _sut;

    public NetworkToolsTests()
    {
        _networkService = Substitute.For<INetworkService>();
        _sut = new NetworkTools(_networkService, NullLogger<NetworkTools>.Instance);
    }

    [Fact]
    public void TestConnection_WithHostAndPort_ReturnsConnectivityResult()
    {
        // Arrange
        var expectedResult = new ConnectivityResult
        {
            Host = "google.com",
            Port = 443,
            TcpConnected = true,
            LatencyMs = 25,
            Error = null
        };
        _networkService.TestConnection("google.com", 443, 5000).Returns(expectedResult);

        // Act
        var result = _sut.TestConnection("google.com", 443);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _networkService.Received(1).TestConnection("google.com", 443, 5000);
    }

    [Fact]
    public void TestConnection_WithCustomTimeout_PassesTimeoutToService()
    {
        // Arrange
        var expectedResult = new ConnectivityResult { Host = "test.com", Port = 80 };
        _networkService.TestConnection("test.com", 80, 10000).Returns(expectedResult);

        // Act
        var result = _sut.TestConnection("test.com", 80, 10000);

        // Assert
        _networkService.Received(1).TestConnection("test.com", 80, 10000);
    }

    [Fact]
    public void TestConnection_WhenConnectionFails_ReturnsErrorResult()
    {
        // Arrange
        var expectedResult = new ConnectivityResult
        {
            Host = "unreachable.local",
            Port = 8080,
            TcpConnected = false,
            LatencyMs = 0,
            Error = "Connection refused"
        };
        _networkService.TestConnection("unreachable.local", 8080, 5000).Returns(expectedResult);

        // Act
        var result = _sut.TestConnection("unreachable.local", 8080);

        // Assert
        result.TcpConnected.Should().BeFalse();
        result.Error.Should().Be("Connection refused");
    }

    [Fact]
    public void TestConnection_DefaultTimeoutIs5000()
    {
        // Arrange
        _networkService.TestConnection("localhost", 80, 5000).Returns(new ConnectivityResult());

        // Act
        _ = _sut.TestConnection("localhost", 80);

        // Assert
        _networkService.Received(1).TestConnection("localhost", 80, 5000);
    }

    [Fact]
    public void ResolveDns_WithHostname_ReturnsDnsResult()
    {
        // Arrange
        var expectedResult = new DnsResult
        {
            HostName = "google.com",
            IpAddresses = new List<string> { "142.250.80.46", "2607:f8b0:4004:800::200e" },
            Error = null
        };
        _networkService.ResolveDns("google.com").Returns(expectedResult);

        // Act
        var result = _sut.ResolveDns("google.com");

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _networkService.Received(1).ResolveDns("google.com");
    }

    [Fact]
    public void ResolveDns_WhenHostNotFound_ReturnsError()
    {
        // Arrange
        var expectedResult = new DnsResult
        {
            HostName = "nonexistent.invalid",
            IpAddresses = new List<string>(),
            Error = "No such host is known"
        };
        _networkService.ResolveDns("nonexistent.invalid").Returns(expectedResult);

        // Act
        var result = _sut.ResolveDns("nonexistent.invalid");

        // Assert
        result.IpAddresses.Should().BeEmpty();
        result.Error.Should().Contain("No such host");
    }

    [Fact]
    public void ResolveDns_PassesHostnameToService()
    {
        // Arrange
        var hostname = "microsoft.com";
        _networkService.ResolveDns(hostname).Returns(new DnsResult { HostName = hostname });

        // Act
        _ = _sut.ResolveDns(hostname);

        // Assert
        _networkService.Received(1).ResolveDns(hostname);
    }

    [Fact]
    public void GetNetworkAdapters_ReturnsAdaptersFromService()
    {
        // Arrange
        var expectedAdapters = new[]
        {
            new NetworkAdapterInfo
            {
                Name = "Ethernet",
                Description = "Intel Ethernet Controller",
                Status = "Up",
                IpAddresses = new List<string> { "192.168.1.100" },
                MacAddress = "00:11:22:33:44:55"
            },
            new NetworkAdapterInfo
            {
                Name = "Wi-Fi",
                Description = "Intel Wi-Fi 6",
                Status = "Down",
                IpAddresses = new List<string>(),
                MacAddress = "66:77:88:99:AA:BB"
            }
        };
        _networkService.GetNetworkAdapters().Returns(expectedAdapters);

        // Act
        var result = _sut.GetNetworkAdapters();

        // Assert
        result.Should().BeEquivalentTo(expectedAdapters);
        _networkService.Received(1).GetNetworkAdapters();
    }

    [Fact]
    public void GetNetworkAdapters_WhenNoAdapters_ReturnsEmpty()
    {
        // Arrange
        _networkService.GetNetworkAdapters().Returns(Enumerable.Empty<NetworkAdapterInfo>());

        // Act
        var result = _sut.GetNetworkAdapters();

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("localhost", 80)]
    [InlineData("127.0.0.1", 443)]
    [InlineData("::1", 8080)]
    public void TestConnection_WithVariousHostsAndPorts_PassesCorrectParameters(string host, int port)
    {
        // Arrange
        _networkService.TestConnection(host, port, 5000).Returns(new ConnectivityResult { Host = host, Port = port });

        // Act
        var result = _sut.TestConnection(host, port);

        // Assert
        result.Host.Should().Be(host);
        result.Port.Should().Be(port);
    }

    [Fact]
    public void TestConnection_WithEmptyHost_ThrowsInvalidParameterException()
    {
        // Act
        Action act = () => _sut.TestConnection("", 80);

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*host*");
    }

    [Fact]
    public void TestConnection_WithInvalidPort_ThrowsInvalidParameterException()
    {
        // Act
        Action act = () => _sut.TestConnection("localhost", 0);

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*port*");
    }

    [Fact]
    public void TestConnection_WithInvalidTimeout_ThrowsInvalidParameterException()
    {
        // Act
        Action act = () => _sut.TestConnection("localhost", 80, 0);

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*timeoutMs*");
    }

    [Fact]
    public void ResolveDns_WithEmptyHostname_ThrowsInvalidParameterException()
    {
        // Act
        Action act = () => _sut.ResolveDns("");

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*hostName*");
    }

    [Fact]
    public void GetNetworkAdapters_WhenServiceThrows_ThrowsWindowsApiException()
    {
        // Arrange
        _networkService.GetNetworkAdapters().Returns(_ => throw new InvalidOperationException("Test error"));

        // Act
        Action act = () => _sut.GetNetworkAdapters();

        // Assert
        act.Should().Throw<WindowsApiException>();
    }
}
