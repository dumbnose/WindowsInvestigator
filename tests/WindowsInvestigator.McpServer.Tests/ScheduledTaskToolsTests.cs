using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WindowsInvestigator.McpServer.Exceptions;
using WindowsInvestigator.McpServer.Services;
using WindowsInvestigator.McpServer.Tools;

namespace WindowsInvestigator.McpServer.Tests;

public class ScheduledTaskToolsTests
{
    private readonly IScheduledTaskService _taskService;
    private readonly ScheduledTaskTools _sut;

    public ScheduledTaskToolsTests()
    {
        _taskService = Substitute.For<IScheduledTaskService>();
        _sut = new ScheduledTaskTools(_taskService, NullLogger<ScheduledTaskTools>.Instance);
    }

    [Fact]
    public void ListScheduledTasks_WithDefaultParams_ReturnsTasksFromService()
    {
        // Arrange
        var expectedTasks = new[]
        {
            new ScheduledTaskInfo { Name = "Task1", Path = "\\Task1", State = ScheduledTaskState.Ready },
            new ScheduledTaskInfo { Name = "Task2", Path = "\\Microsoft\\Task2", State = ScheduledTaskState.Disabled }
        };
        _taskService.GetTasks(false).Returns(expectedTasks);

        // Act
        var result = _sut.ListScheduledTasks();

        // Assert
        result.Should().BeEquivalentTo(expectedTasks);
        _taskService.Received(1).GetTasks(false);
    }

    [Fact]
    public void ListScheduledTasks_WithIncludeHidden_PassesValueToService()
    {
        // Arrange
        _taskService.GetTasks(true).Returns(Enumerable.Empty<ScheduledTaskInfo>());

        // Act
        var result = _sut.ListScheduledTasks(includeHidden: true);

        // Assert
        _taskService.Received(1).GetTasks(true);
    }

    [Fact]
    public void GetScheduledTask_WithValidPath_ReturnsTask()
    {
        // Arrange
        var expectedTask = new ScheduledTaskInfo
        {
            Name = "WindowsUpdate",
            Path = "\\Microsoft\\Windows\\WindowsUpdate\\Scheduled Start",
            State = ScheduledTaskState.Ready
        };
        _taskService.GetTask("\\Microsoft\\Windows\\WindowsUpdate\\Scheduled Start").Returns(expectedTask);

        // Act
        var result = _sut.GetScheduledTask("\\Microsoft\\Windows\\WindowsUpdate\\Scheduled Start");

        // Assert
        result.Should().BeEquivalentTo(expectedTask);
    }

    [Fact]
    public void GetScheduledTask_WithEmptyPath_ThrowsInvalidParameterException()
    {
        // Act
        var act = () => _sut.GetScheduledTask("");

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*Task path cannot be empty*");
    }

    [Fact]
    public void GetScheduledTask_WithNullPath_ThrowsInvalidParameterException()
    {
        // Act
        var act = () => _sut.GetScheduledTask(null!);

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*Task path cannot be empty*");
    }

    [Fact]
    public void SearchScheduledTasks_WithValidPattern_ReturnsTasks()
    {
        // Arrange
        var expectedTasks = new[]
        {
            new ScheduledTaskInfo { Name = "WindowsUpdate", Path = "\\Microsoft\\Windows\\WindowsUpdate\\Scheduled Start" }
        };
        _taskService.SearchTasks("Update").Returns(expectedTasks);

        // Act
        var result = _sut.SearchScheduledTasks("Update");

        // Assert
        result.Should().BeEquivalentTo(expectedTasks);
        _taskService.Received(1).SearchTasks("Update");
    }

    [Fact]
    public void SearchScheduledTasks_WithEmptyPattern_ThrowsInvalidParameterException()
    {
        // Act
        var act = () => _sut.SearchScheduledTasks("");

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*Search pattern cannot be empty*");
    }

    [Fact]
    public void SearchScheduledTasks_WithInvalidRegex_ThrowsInvalidParameterException()
    {
        // Act
        var act = () => _sut.SearchScheduledTasks("[invalid");

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*Invalid regex*");
    }

    [Fact]
    public void GetFailedTasks_ReturnsFromService()
    {
        // Arrange
        var expectedTasks = new[]
        {
            new ScheduledTaskInfo { Name = "FailedTask", LastRunResult = -1 }
        };
        _taskService.GetFailedTasks().Returns(expectedTasks);

        // Act
        var result = _sut.GetFailedTasks();

        // Assert
        result.Should().BeEquivalentTo(expectedTasks);
        _taskService.Received(1).GetFailedTasks();
    }

    [Fact]
    public void GetTaskHistory_WithValidPath_ReturnsHistory()
    {
        // Arrange
        var expectedHistory = new[]
        {
            new ScheduledTaskRun { TaskName = "Task1", StartTime = DateTime.Now.AddHours(-1), IsSuccess = true },
            new ScheduledTaskRun { TaskName = "Task1", StartTime = DateTime.Now.AddHours(-2), IsSuccess = false }
        };
        _taskService.GetTaskHistory("\\Task1", 20).Returns(expectedHistory);

        // Act
        var result = _sut.GetTaskHistory("\\Task1");

        // Assert
        result.Should().BeEquivalentTo(expectedHistory);
        _taskService.Received(1).GetTaskHistory("\\Task1", 20);
    }

    [Fact]
    public void GetTaskHistory_WithEmptyPath_ThrowsInvalidParameterException()
    {
        // Act
        var act = () => _sut.GetTaskHistory("");

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*Task path cannot be empty*");
    }

    [Fact]
    public void GetTaskHistory_WithZeroMaxResults_ThrowsInvalidParameterException()
    {
        // Act
        var act = () => _sut.GetTaskHistory("\\Task1", 0);

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*Maximum results must be greater than 0*");
    }

    [Fact]
    public void ListScheduledTasks_WhenServiceThrows_ThrowsWindowsApiException()
    {
        // Arrange
        _taskService.GetTasks(false).Returns(_ => throw new InvalidOperationException("Test error"));

        // Act
        var act = () => _sut.ListScheduledTasks();

        // Assert
        act.Should().Throw<WindowsApiException>();
    }
}
