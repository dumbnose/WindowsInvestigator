using FluentAssertions;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.IntegrationTests;

/// <summary>
/// Integration tests that run against real Windows Event Logs.
/// These tests verify that the service correctly interacts with Windows APIs.
/// </summary>
public class WindowsEventLogServiceTests
{
    private readonly WindowsEventLogService _sut;

    public WindowsEventLogServiceTests()
    {
        _sut = new WindowsEventLogService();
    }

    [Fact]
    public void GetLogNames_ReturnsStandardWindowsLogs()
    {
        // Act
        var logNames = _sut.GetLogNames().ToList();

        // Assert
        logNames.Should().NotBeEmpty();
        logNames.Should().Contain("Application");
        logNames.Should().Contain("System");
        logNames.Should().Contain("Security");
    }

    [Fact]
    public void GetLogNames_ReturnsOrderedList()
    {
        // Act
        var logNames = _sut.GetLogNames().ToList();

        // Assert
        logNames.Should().BeInAscendingOrder(StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void QueryEvents_FromSystemLog_ReturnsEvents()
    {
        // Act
        var events = _sut.QueryEvents("System", maxResults: 10).ToList();

        // Assert
        events.Should().NotBeEmpty();
        events.Should().HaveCountLessThanOrEqualTo(10);
        events.Should().AllSatisfy(e =>
        {
            e.Level.Should().NotBeNullOrEmpty();
            e.Source.Should().NotBeNull();
        });
    }

    [Fact]
    public void QueryEvents_FromApplicationLog_ReturnsEvents()
    {
        // Act
        var events = _sut.QueryEvents("Application", maxResults: 10).ToList();

        // Assert
        events.Should().NotBeEmpty();
        events.Should().HaveCountLessThanOrEqualTo(10);
    }

    [Fact]
    public void QueryEvents_WithLevelFilter_ReturnsFilteredEvents()
    {
        // Act - Query for errors only
        var events = _sut.QueryEvents("Application", level: "Error", maxResults: 5).ToList();

        // Assert - All returned events should be errors (or empty if no errors exist)
        events.Should().AllSatisfy(e => e.Level.Should().Be("Error"));
    }

    [Fact]
    public void QueryEvents_RespectsMaxResults()
    {
        // Act
        var events = _sut.QueryEvents("System", maxResults: 3).ToList();

        // Assert
        events.Should().HaveCountLessThanOrEqualTo(3);
    }

    [Fact]
    public void QueryEvents_EventsHaveTimeCreated()
    {
        // Act
        var events = _sut.QueryEvents("System", maxResults: 5).ToList();

        // Assert
        events.Should().AllSatisfy(e => e.TimeCreated.Should().NotBeNull());
    }

    [Fact]
    public void QueryEvents_EventsHaveEventId()
    {
        // Act
        var events = _sut.QueryEvents("System", maxResults: 5).ToList();

        // Assert
        events.Should().AllSatisfy(e => e.EventId.Should().BeGreaterThanOrEqualTo(0));
    }

    [Fact]
    public void QueryEvents_WithInvalidLogName_ThrowsException()
    {
        // Act
        var act = () => _sut.QueryEvents("NonExistentLogName12345").ToList();

        // Assert
        act.Should().Throw<Exception>();
    }

    [Theory]
    [InlineData("Critical")]
    [InlineData("Error")]
    [InlineData("Warning")]
    [InlineData("Information")]
    public void QueryEvents_WithValidLevel_DoesNotThrow(string level)
    {
        // Act
        var act = () => _sut.QueryEvents("System", level: level, maxResults: 1).ToList();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void QueryEvents_WithReverseChronologicalTrue_ReturnsMostRecentFirst()
    {
        // Act
        var events = _sut.QueryEvents("System", maxResults: 10, reverseChronological: true).ToList();

        // Assert
        if (events.Count > 1)
        {
            // Events should be in descending order by time
            for (int i = 0; i < events.Count - 1; i++)
            {
                events[i].TimeCreated.Should().BeOnOrAfter(events[i + 1].TimeCreated!.Value);
            }
        }
    }

    [Fact]
    public void QueryEvents_WithReverseChronologicalFalse_ReturnsOldestFirst()
    {
        // Act
        var events = _sut.QueryEvents("System", maxResults: 10, reverseChronological: false).ToList();

        // Assert
        if (events.Count > 1)
        {
            // Events should be in ascending order by time
            for (int i = 0; i < events.Count - 1; i++)
            {
                events[i].TimeCreated.Should().BeOnOrBefore(events[i + 1].TimeCreated!.Value);
            }
        }
    }

    [Fact]
    public void QueryEvents_DefaultReverseChronological_ReturnsMostRecentFirst()
    {
        // Act - default should be reverseChronological: true
        var events = _sut.QueryEvents("System", maxResults: 10).ToList();

        // Assert
        if (events.Count > 1)
        {
            // Events should be in descending order by time (most recent first)
            for (int i = 0; i < events.Count - 1; i++)
            {
                events[i].TimeCreated.Should().BeOnOrAfter(events[i + 1].TimeCreated!.Value);
            }
        }
    }
}
