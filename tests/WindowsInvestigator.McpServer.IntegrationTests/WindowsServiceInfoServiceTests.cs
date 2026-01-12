using FluentAssertions;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.IntegrationTests;

/// <summary>
/// Integration tests that run against real Windows services.
/// </summary>
public class WindowsServiceInfoServiceTests
{
    private readonly WindowsServiceInfoService _sut;

    public WindowsServiceInfoServiceTests()
    {
        _sut = new WindowsServiceInfoService();
    }

    [Fact]
    public void GetAllServices_ReturnsServices()
    {
        // Act
        var result = _sut.GetAllServices().ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Count.Should().BeGreaterThan(10); // Windows has many services
    }

    [Fact]
    public void GetAllServices_ReturnsOrderedByName()
    {
        // Act
        var result = _sut.GetAllServices().ToList();

        // Assert
        result.Select(s => s.Name).Should().BeInAscendingOrder(StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetAllServices_ContainsKnownWindowsServices()
    {
        // Act
        var result = _sut.GetAllServices().ToList();
        var serviceNames = result.Select(s => s.Name).ToList();

        // Assert - These services exist on all Windows systems
        serviceNames.Should().Contain("Spooler");
        serviceNames.Should().Contain("EventLog");
    }

    [Fact]
    public void GetAllServices_ServicesHaveRequiredProperties()
    {
        // Act
        var result = _sut.GetAllServices().Take(10).ToList();

        // Assert
        result.Should().AllSatisfy(s =>
        {
            s.Name.Should().NotBeNullOrEmpty();
            s.DisplayName.Should().NotBeNullOrEmpty();
            s.Status.Should().NotBeNullOrEmpty();
            s.StartType.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public void GetService_WithSpooler_ReturnsSpoolerService()
    {
        // Act
        var result = _sut.GetService("Spooler");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Spooler");
        result.DisplayName.Should().Contain("Print");
    }

    [Fact]
    public void GetService_WithEventLog_ReturnsEventLogService()
    {
        // Act
        var result = _sut.GetService("EventLog");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("EventLog");
        result.DisplayName.Should().Contain("Event Log");
    }

    [Fact]
    public void GetService_ReturnsServiceDetails()
    {
        // Act
        var result = _sut.GetService("Spooler");

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().BeOneOf("Running", "Stopped", "StartPending", "StopPending", "Paused");
        result.StartType.Should().BeOneOf("Automatic", "Manual", "Disabled", "Boot", "System");
    }

    [Fact]
    public void GetService_WithNonExistentService_ReturnsNull()
    {
        // Act
        var result = _sut.GetService("NonExistentService12345");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetService_IsCaseInsensitive()
    {
        // Act
        var result1 = _sut.GetService("spooler");
        var result2 = _sut.GetService("SPOOLER");
        var result3 = _sut.GetService("Spooler");

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result3.Should().NotBeNull();
        // All should return the same service (case-insensitive lookup)
        result1!.Name.Should().BeEquivalentTo(result2!.Name);
        result2.Name.Should().BeEquivalentTo(result3!.Name);
    }

    [Fact]
    public void SearchServices_WithPrintPattern_FindsPrintServices()
    {
        // Act
        var result = _sut.SearchServices(".*Print.*").ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(s => s.Name == "Spooler" || s.DisplayName.Contains("Print", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SearchServices_WithExactName_FindsService()
    {
        // Act
        var result = _sut.SearchServices("^Spooler$").ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Spooler");
    }

    [Fact]
    public void SearchServices_WithNoMatch_ReturnsEmpty()
    {
        // Act
        var result = _sut.SearchServices("NonExistentPattern12345").ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SearchServices_ReturnsOrderedByName()
    {
        // Act
        var result = _sut.SearchServices(".*").ToList();

        // Assert
        result.Select(s => s.Name).Should().BeInAscendingOrder(StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void SearchServices_IsCaseInsensitive()
    {
        // Act
        var result1 = _sut.SearchServices("spooler").ToList();
        var result2 = _sut.SearchServices("SPOOLER").ToList();

        // Assert
        result1.Should().NotBeEmpty();
        result2.Should().NotBeEmpty();
        result1.Count.Should().Be(result2.Count);
    }

    [Theory]
    [InlineData("wuauserv")]
    [InlineData("BITS")]
    [InlineData("EventLog")]
    public void GetService_WithKnownServices_ReturnsService(string serviceName)
    {
        // Act
        var result = _sut.GetService(serviceName);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().BeEquivalentTo(serviceName);
    }
}
