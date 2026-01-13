using FluentAssertions;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.IntegrationTests;

public class WindowsUpdateServiceTests
{
    private readonly WindowsUpdateService _sut = new();

    [Fact]
    public void GetUpdateStatus_ReturnsValidStatus()
    {
        // Act
        var result = _sut.GetUpdateStatus();

        // Assert
        result.Should().NotBeNull();
        result.InstalledUpdatesCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void GetUpdateHistory_ReturnsUpdates()
    {
        // Act
        var result = _sut.GetUpdateHistory(10).ToList();

        // Assert
        result.Should().NotBeNull();
        // May be empty on some systems, but shouldn't throw
    }

    [Fact]
    public void GetUpdateHistory_WithMaxResults_RespectsLimit()
    {
        // Act
        var result = _sut.GetUpdateHistory(5).ToList();

        // Assert
        result.Should().HaveCountLessThanOrEqualTo(5);
    }

    [Fact]
    public void GetUpdateHistory_UpdatesHaveRequiredFields()
    {
        // Act
        var result = _sut.GetUpdateHistory(10).ToList();

        // Assert
        foreach (var update in result)
        {
            update.UpdateId.Should().NotBeNullOrEmpty();
            update.IsInstalled.Should().BeTrue(); // History is installed updates
        }
    }

    [Fact]
    public void GetPendingUpdates_ReturnsValidCollection()
    {
        // Act
        var result = _sut.GetPendingUpdates().ToList();

        // Assert
        result.Should().NotBeNull();
        // May be empty if no pending updates
    }

    [Fact]
    public void GetUpdateFailures_ReturnsValidCollection()
    {
        // Act
        var result = _sut.GetUpdateFailures(10).ToList();

        // Assert
        result.Should().NotBeNull();
        // May be empty if no failures
    }

    [Fact]
    public void GetUpdateFailures_WithMaxResults_RespectsLimit()
    {
        // Act
        var result = _sut.GetUpdateFailures(3).ToList();

        // Assert
        result.Should().HaveCountLessThanOrEqualTo(3);
    }
}
