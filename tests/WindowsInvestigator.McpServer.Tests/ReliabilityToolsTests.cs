using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WindowsInvestigator.McpServer.Exceptions;
using WindowsInvestigator.McpServer.Services;
using WindowsInvestigator.McpServer.Tools;

namespace WindowsInvestigator.McpServer.Tests;

public class ReliabilityToolsTests
{
    private readonly IReliabilityService _reliabilityService;
    private readonly ReliabilityTools _sut;

    public ReliabilityToolsTests()
    {
        _reliabilityService = Substitute.For<IReliabilityService>();
        _sut = new ReliabilityTools(_reliabilityService, NullLogger<ReliabilityTools>.Instance);
    }

    [Fact]
    public void GetReliabilityEvents_WithDefaultParams_ReturnsEvents()
    {
        // Arrange
        var expectedEvents = new[]
        {
            new ReliabilityEvent { Timestamp = DateTime.Now, EventType = ReliabilityEventType.ApplicationCrash, Source = "notepad.exe" },
            new ReliabilityEvent { Timestamp = DateTime.Now.AddHours(-1), EventType = ReliabilityEventType.ApplicationHang, Source = "explorer.exe" }
        };
        _reliabilityService.GetReliabilityEvents(null, null, 50).Returns(expectedEvents);

        // Act
        var result = _sut.GetReliabilityEvents();

        // Assert
        result.Should().BeEquivalentTo(expectedEvents);
        _reliabilityService.Received(1).GetReliabilityEvents(null, null, 50);
    }

    [Fact]
    public void GetReliabilityEvents_WithTimeRange_PassesValuesToService()
    {
        // Arrange
        var startTime = DateTime.Now.AddDays(-7);
        var endTime = DateTime.Now;
        _reliabilityService.GetReliabilityEvents(startTime, endTime, 100).Returns(Enumerable.Empty<ReliabilityEvent>());

        // Act
        var result = _sut.GetReliabilityEvents(startTime, endTime, 100);

        // Assert
        _reliabilityService.Received(1).GetReliabilityEvents(startTime, endTime, 100);
    }

    [Fact]
    public void GetReliabilityEvents_WithZeroMaxResults_ThrowsInvalidParameterException()
    {
        // Act
        var act = () => _sut.GetReliabilityEvents(maxResults: 0);

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*Maximum results must be greater than 0*");
    }

    [Fact]
    public void GetApplicationCrashes_ReturnsFromService()
    {
        // Arrange
        var expectedCrashes = new[]
        {
            new ReliabilityEvent { EventType = ReliabilityEventType.ApplicationCrash, Source = "app.exe" }
        };
        _reliabilityService.GetApplicationCrashes(20).Returns(expectedCrashes);

        // Act
        var result = _sut.GetApplicationCrashes();

        // Assert
        result.Should().BeEquivalentTo(expectedCrashes);
        _reliabilityService.Received(1).GetApplicationCrashes(20);
    }

    [Fact]
    public void GetApplicationCrashes_WithZeroMaxResults_ThrowsInvalidParameterException()
    {
        // Act
        var act = () => _sut.GetApplicationCrashes(0);

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*Maximum results must be greater than 0*");
    }

    [Fact]
    public void GetApplicationHangs_ReturnsFromService()
    {
        // Arrange
        var expectedHangs = new[]
        {
            new ReliabilityEvent { EventType = ReliabilityEventType.ApplicationHang, Source = "app.exe" }
        };
        _reliabilityService.GetApplicationHangs(20).Returns(expectedHangs);

        // Act
        var result = _sut.GetApplicationHangs();

        // Assert
        result.Should().BeEquivalentTo(expectedHangs);
        _reliabilityService.Received(1).GetApplicationHangs(20);
    }

    [Fact]
    public void GetApplicationHangs_WithZeroMaxResults_ThrowsInvalidParameterException()
    {
        // Act
        var act = () => _sut.GetApplicationHangs(0);

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*Maximum results must be greater than 0*");
    }

    [Fact]
    public void GetSystemFailures_ReturnsFromService()
    {
        // Arrange
        var expectedFailures = new[]
        {
            new ReliabilityEvent { EventType = ReliabilityEventType.WindowsFailure, Source = "Kernel-Power" }
        };
        _reliabilityService.GetSystemFailures(20).Returns(expectedFailures);

        // Act
        var result = _sut.GetSystemFailures();

        // Assert
        result.Should().BeEquivalentTo(expectedFailures);
        _reliabilityService.Received(1).GetSystemFailures(20);
    }

    [Fact]
    public void GetSystemFailures_WithZeroMaxResults_ThrowsInvalidParameterException()
    {
        // Act
        var act = () => _sut.GetSystemFailures(0);

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*Maximum results must be greater than 0*");
    }

    [Fact]
    public void GetReliabilityScores_ReturnsFromService()
    {
        // Arrange
        var expectedScores = new[]
        {
            new ReliabilityScore { Date = DateTime.Today, Score = 9.5, ApplicationCrashes = 1 },
            new ReliabilityScore { Date = DateTime.Today.AddDays(-1), Score = 10.0, ApplicationCrashes = 0 }
        };
        _reliabilityService.GetReliabilityScores(30).Returns(expectedScores);

        // Act
        var result = _sut.GetReliabilityScores();

        // Assert
        result.Should().BeEquivalentTo(expectedScores);
        _reliabilityService.Received(1).GetReliabilityScores(30);
    }

    [Fact]
    public void GetReliabilityScores_WithZeroDays_ThrowsInvalidParameterException()
    {
        // Act
        var act = () => _sut.GetReliabilityScores(0);

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*Days must be greater than 0*");
    }

    [Fact]
    public void GetReliabilityEvents_WhenServiceThrows_ThrowsWindowsApiException()
    {
        // Arrange
        _reliabilityService.GetReliabilityEvents(null, null, 50).Returns(_ => throw new InvalidOperationException("Test error"));

        // Act
        var act = () => _sut.GetReliabilityEvents();

        // Assert
        act.Should().Throw<WindowsApiException>();
    }
}
