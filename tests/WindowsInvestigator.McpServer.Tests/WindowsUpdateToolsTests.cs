using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WindowsInvestigator.McpServer.Exceptions;
using WindowsInvestigator.McpServer.Services;
using WindowsInvestigator.McpServer.Tools;

namespace WindowsInvestigator.McpServer.Tests;

public class WindowsUpdateToolsTests
{
    private readonly IWindowsUpdateService _updateService;
    private readonly WindowsUpdateTools _sut;

    public WindowsUpdateToolsTests()
    {
        _updateService = Substitute.For<IWindowsUpdateService>();
        _sut = new WindowsUpdateTools(_updateService, NullLogger<WindowsUpdateTools>.Instance);
    }

    [Fact]
    public void GetUpdateStatus_ReturnsStatusFromService()
    {
        // Arrange
        var expectedStatus = new WindowsUpdateStatus
        {
            InstalledUpdatesCount = 100,
            PendingUpdatesCount = 2,
            IsRebootRequired = true
        };
        _updateService.GetUpdateStatus().Returns(expectedStatus);

        // Act
        var result = _sut.GetUpdateStatus();

        // Assert
        result.Should().BeEquivalentTo(expectedStatus);
        _updateService.Received(1).GetUpdateStatus();
    }

    [Fact]
    public void GetUpdateHistory_WithDefaultParams_ReturnsUpdates()
    {
        // Arrange
        var expectedUpdates = new[]
        {
            new WindowsUpdateInfo { UpdateId = "KB123456", Title = "Security Update", IsInstalled = true },
            new WindowsUpdateInfo { UpdateId = "KB789012", Title = "Feature Update", IsInstalled = true }
        };
        _updateService.GetUpdateHistory(50).Returns(expectedUpdates);

        // Act
        var result = _sut.GetUpdateHistory();

        // Assert
        result.Should().BeEquivalentTo(expectedUpdates);
        _updateService.Received(1).GetUpdateHistory(50);
    }

    [Fact]
    public void GetUpdateHistory_WithMaxResults_PassesValueToService()
    {
        // Arrange
        _updateService.GetUpdateHistory(10).Returns(Enumerable.Empty<WindowsUpdateInfo>());

        // Act
        var result = _sut.GetUpdateHistory(10);

        // Assert
        _updateService.Received(1).GetUpdateHistory(10);
    }

    [Fact]
    public void GetUpdateHistory_WithZeroMaxResults_ThrowsInvalidParameterException()
    {
        // Act
        var act = () => _sut.GetUpdateHistory(0);

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*Maximum results must be greater than 0*");
    }

    [Fact]
    public void GetUpdateHistory_WithNegativeMaxResults_ThrowsInvalidParameterException()
    {
        // Act
        var act = () => _sut.GetUpdateHistory(-5);

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*Maximum results must be greater than 0*");
    }

    [Fact]
    public void GetPendingUpdates_ReturnsPendingFromService()
    {
        // Arrange
        var expectedUpdates = new[]
        {
            new WindowsUpdateInfo { UpdateId = "KB111111", Title = "Pending Update", IsInstalled = false }
        };
        _updateService.GetPendingUpdates().Returns(expectedUpdates);

        // Act
        var result = _sut.GetPendingUpdates();

        // Assert
        result.Should().BeEquivalentTo(expectedUpdates);
        _updateService.Received(1).GetPendingUpdates();
    }

    [Fact]
    public void GetUpdateFailures_WithDefaultParams_ReturnsFailures()
    {
        // Arrange
        var expectedFailures = new[]
        {
            new WindowsUpdateFailure { UpdateId = "1", Title = "Failed Update", ErrorCode = "0x80070005" }
        };
        _updateService.GetUpdateFailures(20).Returns(expectedFailures);

        // Act
        var result = _sut.GetUpdateFailures();

        // Assert
        result.Should().BeEquivalentTo(expectedFailures);
        _updateService.Received(1).GetUpdateFailures(20);
    }

    [Fact]
    public void GetUpdateFailures_WithZeroMaxResults_ThrowsInvalidParameterException()
    {
        // Act
        var act = () => _sut.GetUpdateFailures(0);

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*Maximum results must be greater than 0*");
    }

    [Fact]
    public void GetUpdateStatus_WhenServiceThrows_ThrowsWindowsApiException()
    {
        // Arrange
        _updateService.GetUpdateStatus().Returns(_ => throw new InvalidOperationException("Test error"));

        // Act
        var act = () => _sut.GetUpdateStatus();

        // Assert
        act.Should().Throw<WindowsApiException>();
    }
}
