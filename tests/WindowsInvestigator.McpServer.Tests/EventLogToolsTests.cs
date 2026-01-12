using FluentAssertions;
using NSubstitute;
using WindowsInvestigator.McpServer.Services;
using WindowsInvestigator.McpServer.Tools;

namespace WindowsInvestigator.McpServer.Tests;

public class EventLogToolsTests
{
    private readonly IEventLogService _eventLogService;
    private readonly EventLogTools _sut;

    public EventLogToolsTests()
    {
        _eventLogService = Substitute.For<IEventLogService>();
        _sut = new EventLogTools(_eventLogService);
    }

    [Fact]
    public void ListEventLogs_ReturnsLogNamesFromService()
    {
        // Arrange
        var expectedLogs = new[] { "Application", "Security", "System" };
        _eventLogService.GetLogNames().Returns(expectedLogs);

        // Act
        var result = _sut.ListEventLogs();

        // Assert
        result.Should().BeEquivalentTo(expectedLogs);
        _eventLogService.Received(1).GetLogNames();
    }

    [Fact]
    public void ListEventLogs_WhenServiceReturnsEmpty_ReturnsEmptyCollection()
    {
        // Arrange
        _eventLogService.GetLogNames().Returns(Enumerable.Empty<string>());

        // Act
        var result = _sut.ListEventLogs();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void QueryEventLog_PassesParametersToService()
    {
        // Arrange
        var logName = "Application";
        var level = "Error";
        var source = "TestSource";
        var maxResults = 25;
        
        var expectedEvents = new[]
        {
            new EventLogEntry
            {
                TimeCreated = DateTime.Now,
                Level = "Error",
                Source = "TestSource",
                EventId = 1001,
                Message = "Test error message"
            }
        };
        
        _eventLogService.QueryEvents(logName, level, source, maxResults).Returns(expectedEvents);

        // Act
        var result = _sut.QueryEventLog(logName, level, source, maxResults);

        // Assert
        result.Should().BeEquivalentTo(expectedEvents);
        _eventLogService.Received(1).QueryEvents(logName, level, source, maxResults);
    }

    [Fact]
    public void QueryEventLog_WithDefaultParameters_UsesDefaults()
    {
        // Arrange
        var logName = "System";
        var expectedEvents = Array.Empty<EventLogEntry>();
        
        _eventLogService.QueryEvents(logName, null, null, 50).Returns(expectedEvents);

        // Act
        var result = _sut.QueryEventLog(logName);

        // Assert
        result.Should().BeEmpty();
        _eventLogService.Received(1).QueryEvents(logName, null, null, 50);
    }

    [Fact]
    public void QueryEventLog_WhenServiceReturnsMultipleEvents_ReturnsAllEvents()
    {
        // Arrange
        var events = new[]
        {
            new EventLogEntry { EventId = 1, Level = "Error", Message = "Error 1" },
            new EventLogEntry { EventId = 2, Level = "Warning", Message = "Warning 1" },
            new EventLogEntry { EventId = 3, Level = "Information", Message = "Info 1" }
        };
        
        _eventLogService.QueryEvents("Application", null, null, 50).Returns(events);

        // Act
        var result = _sut.QueryEventLog("Application");

        // Assert
        result.Should().HaveCount(3);
        result.Select(e => e.EventId).Should().ContainInOrder(1, 2, 3);
    }
}
