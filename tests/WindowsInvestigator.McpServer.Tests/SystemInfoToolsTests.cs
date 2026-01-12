using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WindowsInvestigator.McpServer.Exceptions;
using WindowsInvestigator.McpServer.Services;
using WindowsInvestigator.McpServer.Tools;

namespace WindowsInvestigator.McpServer.Tests;

public class SystemInfoToolsTests
{
    private readonly ISystemInfoService _systemInfoService;
    private readonly SystemInfoTools _sut;

    public SystemInfoToolsTests()
    {
        _systemInfoService = Substitute.For<ISystemInfoService>();
        _sut = new SystemInfoTools(_systemInfoService, NullLogger<SystemInfoTools>.Instance);
    }

    [Fact]
    public void GetSystemInfo_ReturnsSystemInfoFromService()
    {
        // Arrange
        var expectedInfo = new SystemInfo
        {
            ComputerName = "TESTPC",
            OsName = "Windows 11 Pro",
            OsVersion = "10.0.22631",
            OsBuild = "22631",
            OsArchitecture = "64-bit",
            LastBootTime = DateTime.Now.AddDays(-5),
            Uptime = TimeSpan.FromDays(5),
            UserName = "TestUser",
            UserDomain = "TESTDOMAIN",
            ProcessorCount = 8,
            TotalMemoryMB = 32768,
            AvailableMemoryMB = 16384
        };
        _systemInfoService.GetSystemInfo().Returns(expectedInfo);

        // Act
        var result = _sut.GetSystemInfo();

        // Assert
        result.Should().BeEquivalentTo(expectedInfo);
        _systemInfoService.Received(1).GetSystemInfo();
    }

    [Fact]
    public void GetSystemInfo_ReturnsAllProperties()
    {
        // Arrange
        var expectedInfo = new SystemInfo
        {
            ComputerName = "PC1",
            OsName = "Windows 10 Enterprise",
            OsVersion = "10.0.19045",
            OsBuild = "19045",
            OsArchitecture = "64-bit",
            LastBootTime = new DateTime(2026, 1, 1, 8, 0, 0),
            Uptime = TimeSpan.FromHours(48),
            UserName = "Admin",
            UserDomain = "CORP",
            ProcessorCount = 4,
            TotalMemoryMB = 16384,
            AvailableMemoryMB = 8192
        };
        _systemInfoService.GetSystemInfo().Returns(expectedInfo);

        // Act
        var result = _sut.GetSystemInfo();

        // Assert
        result.ComputerName.Should().Be("PC1");
        result.OsName.Should().Be("Windows 10 Enterprise");
        result.OsVersion.Should().Be("10.0.19045");
        result.OsBuild.Should().Be("19045");
        result.OsArchitecture.Should().Be("64-bit");
        result.LastBootTime.Should().Be(new DateTime(2026, 1, 1, 8, 0, 0));
        result.Uptime.Should().Be(TimeSpan.FromHours(48));
        result.UserName.Should().Be("Admin");
        result.UserDomain.Should().Be("CORP");
        result.ProcessorCount.Should().Be(4);
        result.TotalMemoryMB.Should().Be(16384);
        result.AvailableMemoryMB.Should().Be(8192);
    }

    [Fact]
    public void GetSystemInfo_CallsServiceExactlyOnce()
    {
        // Arrange
        _systemInfoService.GetSystemInfo().Returns(new SystemInfo());

        // Act
        _ = _sut.GetSystemInfo();

        // Assert
        _systemInfoService.Received(1).GetSystemInfo();
    }

    [Fact]
    public void GetSystemInfo_WhenServiceThrows_ThrowsWindowsApiException()
    {
        // Arrange
        _systemInfoService.GetSystemInfo().Returns(_ => throw new InvalidOperationException("Test error"));

        // Act
        Action act = () => _sut.GetSystemInfo();

        // Assert
        act.Should().Throw<WindowsApiException>();
    }
}
