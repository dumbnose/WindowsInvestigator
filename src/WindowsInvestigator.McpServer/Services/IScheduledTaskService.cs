namespace WindowsInvestigator.McpServer.Services;

/// <summary>
/// Scheduled task state.
/// </summary>
public enum ScheduledTaskState
{
    Unknown,
    Disabled,
    Queued,
    Ready,
    Running
}

/// <summary>
/// Scheduled task information.
/// </summary>
public class ScheduledTaskInfo
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public string? Description { get; set; }
    public ScheduledTaskState State { get; set; }
    public DateTime? LastRunTime { get; set; }
    public int? LastRunResult { get; set; }
    public string? LastRunResultDescription { get; set; }
    public DateTime? NextRunTime { get; set; }
    public string? Author { get; set; }
    public string? UserId { get; set; }
    public string? Triggers { get; set; }
    public string? Actions { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsHidden { get; set; }
}

/// <summary>
/// Scheduled task run history entry.
/// </summary>
public class ScheduledTaskRun
{
    public string TaskName { get; set; } = "";
    public string TaskPath { get; set; } = "";
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int ResultCode { get; set; }
    public string? ResultDescription { get; set; }
    public bool IsSuccess { get; set; }
}

/// <summary>
/// Abstraction for Scheduled Task access.
/// </summary>
public interface IScheduledTaskService
{
    /// <summary>
    /// Gets all scheduled tasks.
    /// </summary>
    IEnumerable<ScheduledTaskInfo> GetTasks(bool includeHidden = false);

    /// <summary>
    /// Gets a specific scheduled task by path.
    /// </summary>
    ScheduledTaskInfo? GetTask(string taskPath);

    /// <summary>
    /// Searches for scheduled tasks by name pattern.
    /// </summary>
    IEnumerable<ScheduledTaskInfo> SearchTasks(string pattern);

    /// <summary>
    /// Gets tasks that have failed recently.
    /// </summary>
    IEnumerable<ScheduledTaskInfo> GetFailedTasks();

    /// <summary>
    /// Gets the run history for a specific task.
    /// </summary>
    IEnumerable<ScheduledTaskRun> GetTaskHistory(string taskPath, int maxResults = 20);
}
