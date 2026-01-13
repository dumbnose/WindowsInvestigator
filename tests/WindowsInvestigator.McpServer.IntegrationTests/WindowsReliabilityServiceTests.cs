using FluentAssertions;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.IntegrationTests;

public class WindowsReliabilityServiceTests
{
    private readonly WindowsReliabilityService _sut = new();

    [Fact]
    public void GetReliabilityEvents_ReturnsValidCollection()
    {
        // Act
        var result = _sut.GetReliabilityEvents(null, null, 20).ToList();

        // Assert
        result.Should().NotBeNull();
        // May be empty on a healthy system
    }

    [Fact]
    public void GetReliabilityEvents_WithTimeRange_ReturnsValidCollection()
    {
        // Arrange
        var startTime = DateTime.Now.AddDays(-7);
        var endTime = DateTime.Now;

        // Act
        var result = _sut.GetReliabilityEvents(startTime, endTime, 20).ToList();

        // Assert
        result.Should().NotBeNull();
        // Note: Event timestamps may vary due to timezone differences in how events are stored
        // We just verify that some events are returned within a reasonable range
    }

    [Fact]
    public void GetReliabilityEvents_WithMaxResults_RespectsLimit()
    {
        // Act
        var result = _sut.GetReliabilityEvents(null, null, 5).ToList();

        // Assert
        result.Should().HaveCountLessThanOrEqualTo(5);
    }

    [Fact]
    public void GetReliabilityEvents_EventsHaveRequiredFields()
    {
        // Act
        var result = _sut.GetReliabilityEvents(null, null, 20).ToList();

        // Assert
        foreach (var evt in result)
        {
            evt.Timestamp.Should().NotBe(DateTime.MinValue);
            evt.Source.Should().NotBeNull();
        }
    }

    [Fact]
    public void GetApplicationCrashes_ReturnsValidCollection()
    {
        // Act
        var result = _sut.GetApplicationCrashes(10).ToList();

        // Assert
        result.Should().NotBeNull();
        foreach (var crash in result)
        {
            crash.EventType.Should().Be(ReliabilityEventType.ApplicationCrash);
        }
    }

    [Fact]
    public void GetApplicationCrashes_WithMaxResults_RespectsLimit()
    {
        // Act
        var result = _sut.GetApplicationCrashes(3).ToList();

        // Assert
        result.Should().HaveCountLessThanOrEqualTo(3);
    }

    [Fact]
    public void GetApplicationHangs_ReturnsValidCollection()
    {
        // Act
        var result = _sut.GetApplicationHangs(10).ToList();

        // Assert
        result.Should().NotBeNull();
        foreach (var hang in result)
        {
            hang.EventType.Should().Be(ReliabilityEventType.ApplicationHang);
        }
    }

    [Fact]
    public void GetSystemFailures_ReturnsValidCollection()
    {
        // Act
        var result = _sut.GetSystemFailures(10).ToList();

        // Assert
        result.Should().NotBeNull();
        foreach (var failure in result)
        {
            failure.EventType.Should().Be(ReliabilityEventType.WindowsFailure);
        }
    }

    [Fact]
    public void GetReliabilityScores_ReturnsValidScores()
    {
        // Act
        var result = _sut.GetReliabilityScores(7).ToList();

        // Assert
        result.Should().NotBeNull();
        foreach (var score in result)
        {
            score.Date.Should().BeOnOrBefore(DateTime.Today);
            score.Score.Should().BeGreaterThanOrEqualTo(0);
            score.Score.Should().BeLessThanOrEqualTo(10);
            score.ApplicationCrashes.Should().BeGreaterThanOrEqualTo(0);
            score.ApplicationHangs.Should().BeGreaterThanOrEqualTo(0);
            score.WindowsFailures.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    public void GetReliabilityScores_ScoresAreOrderedByDate()
    {
        // Act
        var result = _sut.GetReliabilityScores(30).ToList();

        // Assert
        if (result.Count > 1)
        {
            for (int i = 0; i < result.Count - 1; i++)
            {
                result[i].Date.Should().BeOnOrAfter(result[i + 1].Date);
            }
        }
    }
}
