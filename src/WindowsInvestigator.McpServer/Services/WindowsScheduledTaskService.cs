using System.Text.RegularExpressions;
using Microsoft.Win32.TaskScheduler;
using Task = Microsoft.Win32.TaskScheduler.Task;

namespace WindowsInvestigator.McpServer.Services;

/// <summary>
/// Windows implementation of Scheduled Task access.
/// </summary>
public class WindowsScheduledTaskService : IScheduledTaskService
{
    public IEnumerable<ScheduledTaskInfo> GetTasks(bool includeHidden = false)
    {
        var results = new List<ScheduledTaskInfo>();

        try
        {
            using var ts = new TaskService();
            CollectTasks(ts.RootFolder, results, includeHidden);
        }
        catch { }

        return results;
    }

    public ScheduledTaskInfo? GetTask(string taskPath)
    {
        try
        {
            using var ts = new TaskService();
            var task = ts.GetTask(taskPath);
            
            if (task == null)
                return null;

            return ConvertTask(task);
        }
        catch
        {
            return null;
        }
    }

    public IEnumerable<ScheduledTaskInfo> SearchTasks(string pattern)
    {
        var results = new List<ScheduledTaskInfo>();
        var regex = new Regex(pattern, RegexOptions.IgnoreCase);

        try
        {
            using var ts = new TaskService();
            SearchTasksRecursive(ts.RootFolder, regex, results);
        }
        catch { }

        return results;
    }

    public IEnumerable<ScheduledTaskInfo> GetFailedTasks()
    {
        var results = new List<ScheduledTaskInfo>();

        try
        {
            using var ts = new TaskService();
            CollectFailedTasks(ts.RootFolder, results);
        }
        catch { }

        return results;
    }

    public IEnumerable<ScheduledTaskRun> GetTaskHistory(string taskPath, int maxResults = 20)
    {
        var results = new List<ScheduledTaskRun>();

        try
        {
            using var ts = new TaskService();
            var task = ts.GetTask(taskPath);
            
            if (task == null)
                return results;

            // Get task history from event log
            var history = task.GetRunTimes(DateTime.Now.AddDays(-30), DateTime.Now, (uint)maxResults);
            
            foreach (var runTime in history.Take(maxResults))
            {
                results.Add(new ScheduledTaskRun
                {
                    TaskName = task.Name,
                    TaskPath = task.Path,
                    StartTime = runTime,
                    ResultCode = 0,
                    IsSuccess = true
                });
            }

            // Also check last run result
            if (task.LastRunTime != DateTime.MinValue)
            {
                var lastRun = new ScheduledTaskRun
                {
                    TaskName = task.Name,
                    TaskPath = task.Path,
                    StartTime = task.LastRunTime,
                    ResultCode = task.LastTaskResult,
                    ResultDescription = GetResultDescription(task.LastTaskResult),
                    IsSuccess = task.LastTaskResult == 0
                };

                // Insert at beginning if not already there
                if (!results.Any(r => r.StartTime == lastRun.StartTime))
                {
                    results.Insert(0, lastRun);
                }
            }
        }
        catch { }

        return results.OrderByDescending(r => r.StartTime).Take(maxResults).ToList();
    }

    private void CollectTasks(TaskFolder folder, List<ScheduledTaskInfo> results, bool includeHidden)
    {
        try
        {
            foreach (var task in folder.Tasks)
            {
                try
                {
                    if (!includeHidden && task.Definition.Settings.Hidden)
                        continue;

                    results.Add(ConvertTask(task));
                }
                catch { }
            }

            foreach (var subFolder in folder.SubFolders)
            {
                CollectTasks(subFolder, results, includeHidden);
            }
        }
        catch { }
    }

    private void SearchTasksRecursive(TaskFolder folder, Regex pattern, List<ScheduledTaskInfo> results)
    {
        try
        {
            foreach (var task in folder.Tasks)
            {
                try
                {
                    if (pattern.IsMatch(task.Name) || pattern.IsMatch(task.Path) || 
                        (task.Definition.RegistrationInfo.Description != null && 
                         pattern.IsMatch(task.Definition.RegistrationInfo.Description)))
                    {
                        results.Add(ConvertTask(task));
                    }
                }
                catch { }
            }

            foreach (var subFolder in folder.SubFolders)
            {
                SearchTasksRecursive(subFolder, pattern, results);
            }
        }
        catch { }
    }

    private void CollectFailedTasks(TaskFolder folder, List<ScheduledTaskInfo> results)
    {
        try
        {
            foreach (var task in folder.Tasks)
            {
                try
                {
                    // Check if last run failed (non-zero result, excluding "task is running" code)
                    if (task.LastTaskResult != 0 && task.LastTaskResult != 0x00041301)
                    {
                        results.Add(ConvertTask(task));
                    }
                }
                catch { }
            }

            foreach (var subFolder in folder.SubFolders)
            {
                CollectFailedTasks(subFolder, results);
            }
        }
        catch { }
    }

    private ScheduledTaskInfo ConvertTask(Task task)
    {
        var info = new ScheduledTaskInfo
        {
            Name = task.Name,
            Path = task.Path,
            State = ConvertState(task.State),
            LastRunTime = task.LastRunTime == DateTime.MinValue ? null : task.LastRunTime,
            LastRunResult = task.LastTaskResult,
            LastRunResultDescription = GetResultDescription(task.LastTaskResult),
            NextRunTime = task.NextRunTime == DateTime.MinValue ? null : task.NextRunTime,
            IsEnabled = task.Enabled,
            IsHidden = task.Definition.Settings.Hidden
        };

        try
        {
            info.Description = task.Definition.RegistrationInfo.Description;
            info.Author = task.Definition.RegistrationInfo.Author;
            
            // Get user context
            info.UserId = task.Definition.Principal.UserId;

            // Get triggers summary
            var triggers = task.Definition.Triggers;
            if (triggers.Count > 0)
            {
                info.Triggers = string.Join("; ", triggers.Take(3).Select(t => t.ToString()));
            }

            // Get actions summary
            var actions = task.Definition.Actions;
            if (actions.Count > 0)
            {
                info.Actions = string.Join("; ", actions.Take(3).Select(a => a.ToString()));
            }
        }
        catch { }

        return info;
    }

    private static ScheduledTaskState ConvertState(TaskState state)
    {
        return state switch
        {
            TaskState.Disabled => ScheduledTaskState.Disabled,
            TaskState.Queued => ScheduledTaskState.Queued,
            TaskState.Ready => ScheduledTaskState.Ready,
            TaskState.Running => ScheduledTaskState.Running,
            _ => ScheduledTaskState.Unknown
        };
    }

    private static string GetResultDescription(int resultCode)
    {
        // Use unchecked to handle negative int values from HRESULT codes
        return unchecked((uint)resultCode) switch
        {
            0 => "Success",
            0x00041300 => "Task is ready to run at its next scheduled time",
            0x00041301 => "Task is currently running",
            0x00041302 => "Task is disabled",
            0x00041303 => "Task has not yet run",
            0x00041304 => "No more runs scheduled",
            0x00041305 => "Triggered by event",
            0x00041306 => "Task terminated by user",
            0x00041307 => "No valid triggers",
            0x00041308 => "Event triggers do not have set run times",
            0x8004130F => "Credentials became corrupted",
            0x8004131F => "Instance already running",
            0x80041326 => "Task not started",
            0x80070005 => "Access denied",
            _ => $"Error code: 0x{resultCode:X8}"
        };
    }
}
