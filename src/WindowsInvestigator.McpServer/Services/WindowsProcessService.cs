using System.Diagnostics;
using System.Management;
using System.Text.RegularExpressions;

namespace WindowsInvestigator.McpServer.Services;

/// <summary>
/// Windows process service implementation.
/// </summary>
public class WindowsProcessService : IProcessService
{
    public IEnumerable<ProcessInfo> GetProcesses()
    {
        var processes = Process.GetProcesses();
        var processInfos = new List<ProcessInfo>();

        foreach (var process in processes)
        {
            try
            {
                processInfos.Add(GetProcessInfoFromProcess(process));
            }
            catch
            {
                // Skip processes we can't access
            }
            finally
            {
                process.Dispose();
            }
        }

        return processInfos.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public ProcessInfo? GetProcess(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            var info = GetProcessInfoFromProcess(process);
            
            // Get additional info from WMI for the specific process
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    $"SELECT CommandLine, ExecutablePath FROM Win32_Process WHERE ProcessId = {processId}");
                
                foreach (ManagementObject obj in searcher.Get())
                {
                    info.CommandLine = obj["CommandLine"]?.ToString();
                    info.ExecutablePath = obj["ExecutablePath"]?.ToString();
                }
            }
            catch
            {
                // WMI info not available
            }

            return info;
        }
        catch (ArgumentException)
        {
            return null; // Process not found
        }
        catch (InvalidOperationException)
        {
            return null; // Process has exited
        }
    }

    public IEnumerable<ProcessInfo> SearchProcesses(string pattern)
    {
        Regex regex;
        try
        {
            regex = new Regex(pattern, RegexOptions.IgnoreCase);
        }
        catch (ArgumentException)
        {
            return Enumerable.Empty<ProcessInfo>();
        }

        var results = new List<ProcessInfo>();
        var processes = Process.GetProcesses();

        foreach (var process in processes)
        {
            try
            {
                bool matches = regex.IsMatch(process.ProcessName);
                
                if (!matches)
                {
                    // Try to match command line from WMI
                    try
                    {
                        using var searcher = new ManagementObjectSearcher(
                            $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}");
                        
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            var commandLine = obj["CommandLine"]?.ToString();
                            if (!string.IsNullOrEmpty(commandLine) && regex.IsMatch(commandLine))
                            {
                                matches = true;
                            }
                        }
                    }
                    catch
                    {
                        // WMI not available
                    }
                }

                if (matches)
                {
                    results.Add(GetProcessInfoFromProcess(process));
                }
            }
            catch
            {
                // Skip inaccessible processes
            }
            finally
            {
                process.Dispose();
            }
        }

        return results.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public ProcessSummary GetProcessSummary()
    {
        var processes = GetProcesses().ToList();
        
        return new ProcessSummary
        {
            TotalProcesses = processes.Count,
            TotalThreads = processes.Sum(p => p.ThreadCount),
            TotalWorkingSetBytes = processes.Sum(p => p.WorkingSetBytes),
            TopCpuProcesses = processes.OrderByDescending(p => p.CpuPercent).Take(10).ToList(),
            TopMemoryProcesses = processes.OrderByDescending(p => p.WorkingSetBytes).Take(10).ToList()
        };
    }

    private static ProcessInfo GetProcessInfoFromProcess(Process process)
    {
        var info = new ProcessInfo
        {
            ProcessId = process.Id,
            Name = process.ProcessName,
        };

        try
        {
            info.ThreadCount = process.Threads.Count;
        }
        catch { }

        try
        {
            info.HandleCount = process.HandleCount;
        }
        catch { }

        try
        {
            info.WorkingSetBytes = process.WorkingSet64;
        }
        catch { }

        try
        {
            info.PrivateMemoryBytes = process.PrivateMemorySize64;
        }
        catch { }

        try
        {
            info.VirtualMemoryBytes = process.VirtualMemorySize64;
        }
        catch { }

        try
        {
            info.StartTime = process.StartTime;
        }
        catch { }

        try
        {
            info.Priority = process.PriorityClass.ToString();
        }
        catch { }

        try
        {
            info.WindowTitle = string.IsNullOrWhiteSpace(process.MainWindowTitle) ? null : process.MainWindowTitle;
        }
        catch { }

        try
        {
            info.ExecutablePath = process.MainModule?.FileName;
        }
        catch { }

        return info;
    }
}
