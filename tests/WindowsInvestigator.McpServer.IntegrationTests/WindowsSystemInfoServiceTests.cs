using FluentAssertions;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.IntegrationTests;

/// <summary>
/// Integration tests that run against real Windows system information.
/// </summary>
public class WindowsSystemInfoServiceTests
{
    private readonly WindowsSystemInfoService _sut;

    public WindowsSystemInfoServiceTests()
    {
        _sut = new WindowsSystemInfoService();
    }

    [Fact]
    public void GetSystemInfo_ReturnsNonNullResult()
    {
        // Act
        var result = _sut.GetSystemInfo();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void GetSystemInfo_ReturnsComputerName()
    {
        // Act
        var result = _sut.GetSystemInfo();

        // Assert
        result.ComputerName.Should().NotBeNullOrEmpty();
        result.ComputerName.Should().Be(Environment.MachineName);
    }

    [Fact]
    public void GetSystemInfo_ReturnsUserName()
    {
        // Act
        var result = _sut.GetSystemInfo();

        // Assert
        result.UserName.Should().NotBeNullOrEmpty();
        result.UserName.Should().Be(Environment.UserName);
    }

    [Fact]
    public void GetSystemInfo_ReturnsUserDomain()
    {
        // Act
        var result = _sut.GetSystemInfo();

        // Assert
        result.UserDomain.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetSystemInfo_ReturnsProcessorCount()
    {
        // Act
        var result = _sut.GetSystemInfo();

        // Assert
        result.ProcessorCount.Should().BeGreaterThan(0);
        result.ProcessorCount.Should().Be(Environment.ProcessorCount);
    }

    [Fact]
    public void GetSystemInfo_ReturnsOsArchitecture()
    {
        // Act
        var result = _sut.GetSystemInfo();

        // Assert
        result.OsArchitecture.Should().NotBeNullOrEmpty();
        result.OsArchitecture.Should().BeOneOf("X64", "X86", "Arm64", "Arm");
    }

    [Fact]
    public void GetSystemInfo_ReturnsOsName()
    {
        // Act
        var result = _sut.GetSystemInfo();

        // Assert
        result.OsName.Should().NotBeNullOrEmpty();
        result.OsName.Should().Contain("Windows");
    }

    [Fact]
    public void GetSystemInfo_ReturnsOsBuild()
    {
        // Act
        var result = _sut.GetSystemInfo();

        // Assert
        result.OsBuild.Should().NotBeNullOrEmpty();
        // Build should be numeric (possibly with UBR suffix like "22631.4562")
        result.OsBuild.Should().MatchRegex(@"^\d+");
    }

    [Fact]
    public void GetSystemInfo_ReturnsValidMemoryInfo()
    {
        // Act
        var result = _sut.GetSystemInfo();

        // Assert
        result.TotalMemoryMB.Should().BeGreaterThan(0);
        result.AvailableMemoryMB.Should().BeGreaterThan(0);
        result.AvailableMemoryMB.Should().BeLessThanOrEqualTo(result.TotalMemoryMB);
    }

    [Fact]
    public void GetSystemInfo_ReturnsValidUptime()
    {
        // Act
        var result = _sut.GetSystemInfo();

        // Assert
        result.Uptime.Should().BeGreaterThan(TimeSpan.Zero);
        result.LastBootTime.Should().BeBefore(DateTime.Now);
        result.LastBootTime.Should().BeAfter(DateTime.Now.AddYears(-1)); // Sanity check
    }

    [Fact]
    public void GetSystemInfo_UptimeMatchesLastBootTime()
    {
        // Act
        var result = _sut.GetSystemInfo();

        // Assert
        var calculatedUptime = DateTime.Now - result.LastBootTime;
        // Allow 5 second tolerance for execution time
        result.Uptime.Should().BeCloseTo(calculatedUptime, TimeSpan.FromSeconds(5));
    }
}
