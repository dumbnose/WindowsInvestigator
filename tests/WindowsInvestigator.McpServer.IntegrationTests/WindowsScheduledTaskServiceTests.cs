using FluentAssertions;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.IntegrationTests;

public class WindowsScheduledTaskServiceTests
{
    private readonly WindowsScheduledTaskService _sut = new();

    [Fact]
    public void GetTasks_ReturnsMultipleTasks()
    {
        // Act
        var result = _sut.GetTasks(false).Take(50).ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Count.Should().BeGreaterThan(10); // Windows has many built-in tasks
    }

    [Fact]
    public void GetTasks_TasksHaveRequiredFields()
    {
        // Act
        var result = _sut.GetTasks(false).Take(10).ToList();

        // Assert
        foreach (var task in result)
        {
            task.Name.Should().NotBeNullOrEmpty();
            task.Path.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void GetTasks_WithIncludeHidden_ReturnsMoreTasks()
    {
        // Act
        var withoutHidden = _sut.GetTasks(false).Count();
        var withHidden = _sut.GetTasks(true).Count();

        // Assert
        withHidden.Should().BeGreaterThanOrEqualTo(withoutHidden);
    }

    [Fact]
    public void GetTask_ForKnownTask_ReturnsTask()
    {
        // Arrange - Get a task from the list first
        var anyTask = _sut.GetTasks(false).FirstOrDefault();
        if (anyTask == null) return; // Skip if no tasks available

        // Act
        var result = _sut.GetTask(anyTask.Path);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(anyTask.Name);
        result.Path.Should().Be(anyTask.Path);
    }

    [Fact]
    public void GetTask_ForNonExistentTask_ReturnsNull()
    {
        // Act
        var result = _sut.GetTask("\\NonExistent\\Task\\Path\\12345");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void SearchTasks_WithCommonPattern_ReturnsResults()
    {
        // Act - Search for "Windows" which is a common prefix
        var result = _sut.SearchTasks("Windows").Take(20).ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(t => 
            t.Name.Contains("Windows", StringComparison.OrdinalIgnoreCase) ||
            t.Path.Contains("Windows", StringComparison.OrdinalIgnoreCase) ||
            (t.Description != null && t.Description.Contains("Windows", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void SearchTasks_WithNoMatchPattern_ReturnsEmpty()
    {
        // Act
        var result = _sut.SearchTasks("ZZZNonExistentPattern12345").ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SearchTasks_WithRegexPattern_Works()
    {
        // Act - Search with regex pattern
        var result = _sut.SearchTasks("^[A-Z]").Take(10).ToList();

        // Assert
        result.Should().NotBeNull();
        // May or may not find matches depending on task names
    }

    [Fact]
    public void GetFailedTasks_ReturnsValidCollection()
    {
        // Act
        var result = _sut.GetFailedTasks().ToList();

        // Assert
        result.Should().NotBeNull();
        // Failed tasks should have non-zero, non-running result codes
        foreach (var task in result)
        {
            task.LastRunResult.Should().NotBe(0);
            task.LastRunResult.Should().NotBe(0x00041301); // Not "currently running"
        }
    }

    [Fact]
    public void GetTaskHistory_ForExistingTask_ReturnsHistory()
    {
        // Arrange - Get a task that has been run
        var taskWithHistory = _sut.GetTasks(false)
            .FirstOrDefault(t => t.LastRunTime != null && t.LastRunTime != DateTime.MinValue);
        
        if (taskWithHistory == null) return; // Skip if no suitable task

        // Act
        var result = _sut.GetTaskHistory(taskWithHistory.Path, 10).ToList();

        // Assert
        result.Should().NotBeNull();
        // Should have at least one entry if task has run
        if (result.Any())
        {
            result.First().TaskPath.Should().Be(taskWithHistory.Path);
        }
    }

    [Fact]
    public void GetTaskHistory_ForNonExistentTask_ReturnsEmpty()
    {
        // Act
        var result = _sut.GetTaskHistory("\\NonExistent\\Task", 10).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetTasks_TaskStateValuesAreValid()
    {
        // Act
        var result = _sut.GetTasks(false).Take(20).ToList();

        // Assert
        foreach (var task in result)
        {
            task.State.Should().BeOneOf(
                ScheduledTaskState.Unknown,
                ScheduledTaskState.Disabled,
                ScheduledTaskState.Queued,
                ScheduledTaskState.Ready,
                ScheduledTaskState.Running);
        }
    }

    [Fact]
    public void GetTasks_EnabledTasksHaveReadyOrRunningState()
    {
        // Act
        var result = _sut.GetTasks(false).Where(t => t.IsEnabled).Take(10).ToList();

        // Assert
        foreach (var task in result)
        {
            task.State.Should().BeOneOf(ScheduledTaskState.Ready, ScheduledTaskState.Running, ScheduledTaskState.Queued);
        }
    }

    [Fact]
    public void GetTasks_DisabledTasksHaveDisabledState()
    {
        // Act
        var result = _sut.GetTasks(false).Where(t => !t.IsEnabled).Take(10).ToList();

        // Assert
        foreach (var task in result)
        {
            task.State.Should().Be(ScheduledTaskState.Disabled);
        }
    }
}
