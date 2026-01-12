namespace WindowsInvestigator.McpServer.Services;

/// <summary>
/// Process information.
/// </summary>
public class ProcessInfo
{
    public int ProcessId { get; set; }
    public string Name { get; set; } = "";
    public string? CommandLine { get; set; }
    public string? ExecutablePath { get; set; }
    public string? WorkingDirectory { get; set; }
    public int? ParentProcessId { get; set; }
    public DateTime? StartTime { get; set; }
    public string? UserName { get; set; }
    public long WorkingSetBytes { get; set; }
    public long PrivateMemoryBytes { get; set; }
    public long VirtualMemoryBytes { get; set; }
    public double CpuPercent { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
    public string? Priority { get; set; }
    public string? WindowTitle { get; set; }
}

/// <summary>
/// Summary of system resource usage.
/// </summary>
public class ProcessSummary
{
    public int TotalProcesses { get; set; }
    public int TotalThreads { get; set; }
    public long TotalWorkingSetBytes { get; set; }
    public List<ProcessInfo> TopCpuProcesses { get; set; } = new();
    public List<ProcessInfo> TopMemoryProcesses { get; set; } = new();
}

/// <summary>
/// Abstraction for process inspection.
/// </summary>
public interface IProcessService
{
    /// <summary>
    /// Gets a list of all running processes.
    /// </summary>
    IEnumerable<ProcessInfo> GetProcesses();

    /// <summary>
    /// Gets information about a specific process by ID.
    /// </summary>
    ProcessInfo? GetProcess(int processId);

    /// <summary>
    /// Searches for processes matching a pattern.
    /// </summary>
    /// <param name="pattern">Regex pattern to match process name or command line</param>
    IEnumerable<ProcessInfo> SearchProcesses(string pattern);

    /// <summary>
    /// Gets a summary of system resource usage.
    /// </summary>
    ProcessSummary GetProcessSummary();
}
