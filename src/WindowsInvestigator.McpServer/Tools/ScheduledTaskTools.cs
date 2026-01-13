using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.RegularExpressions;
using WindowsInvestigator.McpServer.Exceptions;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.Tools;

[McpServerToolType]
public sealed class ScheduledTaskTools
{
    private readonly IScheduledTaskService _taskService;
    private readonly ILogger<ScheduledTaskTools> _logger;

    public ScheduledTaskTools(IScheduledTaskService taskService, ILogger<ScheduledTaskTools> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    [McpServerTool, Description("Lists all scheduled tasks on the system")]
    public IEnumerable<ScheduledTaskInfo> ListScheduledTasks(
        [Description("Include hidden tasks (default: false)")] bool includeHidden = false)
    {
        try
        {
            _logger.LogDebug("Listing scheduled tasks (includeHidden: {IncludeHidden})", includeHidden);
            var tasks = _taskService.GetTasks(includeHidden).ToList();
            _logger.LogInformation("Found {Count} scheduled tasks", tasks.Count);
            return tasks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list scheduled tasks");
            throw new WindowsApiException("TaskScheduler.GetTasks", ex);
        }
    }

    [McpServerTool, Description("Gets information about a specific scheduled task")]
    public ScheduledTaskInfo? GetScheduledTask(
        [Description("The full path of the task (e.g., '\\Microsoft\\Windows\\WindowsUpdate\\Scheduled Start')")] string taskPath)
    {
        if (string.IsNullOrWhiteSpace(taskPath))
        {
            throw new InvalidParameterException("taskPath", "Task path cannot be empty");
        }

        try
        {
            _logger.LogDebug("Getting scheduled task: {TaskPath}", taskPath);
            var task = _taskService.GetTask(taskPath);
            
            if (task == null)
            {
                _logger.LogWarning("Scheduled task not found: {TaskPath}", taskPath);
            }
            else
            {
                _logger.LogInformation("Retrieved task {TaskName} (State: {State})", task.Name, task.State);
            }
            
            return task;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to get scheduled task: {TaskPath}", taskPath);
            throw new WindowsApiException("TaskScheduler.GetTask", ex);
        }
    }

    [McpServerTool, Description("Searches for scheduled tasks matching a pattern")]
    public IEnumerable<ScheduledTaskInfo> SearchScheduledTasks(
        [Description("Regex pattern to match task name, path, or description")] string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new InvalidParameterException("pattern", "Search pattern cannot be empty");
        }

        // Validate regex pattern
        try
        {
            _ = new Regex(pattern, RegexOptions.IgnoreCase);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidParameterException("pattern", $"Invalid regex: {ex.Message}");
        }

        try
        {
            _logger.LogDebug("Searching scheduled tasks for pattern: {Pattern}", pattern);
            var tasks = _taskService.SearchTasks(pattern).ToList();
            _logger.LogInformation("Found {Count} tasks matching pattern '{Pattern}'", tasks.Count, pattern);
            return tasks;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to search scheduled tasks with pattern: {Pattern}", pattern);
            throw new WindowsApiException("TaskScheduler.SearchTasks", ex);
        }
    }

    [McpServerTool, Description("Gets scheduled tasks that have failed recently")]
    public IEnumerable<ScheduledTaskInfo> GetFailedTasks()
    {
        try
        {
            _logger.LogDebug("Getting failed scheduled tasks");
            var tasks = _taskService.GetFailedTasks().ToList();
            _logger.LogInformation("Found {Count} failed tasks", tasks.Count);
            return tasks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get failed scheduled tasks");
            throw new WindowsApiException("TaskScheduler.GetFailedTasks", ex);
        }
    }

    [McpServerTool, Description("Gets the run history for a specific scheduled task")]
    public IEnumerable<ScheduledTaskRun> GetTaskHistory(
        [Description("The full path of the task")] string taskPath,
        [Description("Maximum number of history entries to return (default: 20)")] int maxResults = 20)
    {
        if (string.IsNullOrWhiteSpace(taskPath))
        {
            throw new InvalidParameterException("taskPath", "Task path cannot be empty");
        }

        if (maxResults <= 0)
        {
            throw new InvalidParameterException("maxResults", "Maximum results must be greater than 0");
        }

        try
        {
            _logger.LogDebug("Getting history for task: {TaskPath} (max {MaxResults})", taskPath, maxResults);
            var history = _taskService.GetTaskHistory(taskPath, maxResults).ToList();
            _logger.LogInformation("Retrieved {Count} history entries for task {TaskPath}", history.Count, taskPath);
            return history;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to get history for task: {TaskPath}", taskPath);
            throw new WindowsApiException("TaskScheduler.GetTaskHistory", ex);
        }
    }
}
