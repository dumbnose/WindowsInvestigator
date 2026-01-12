namespace WindowsInvestigator.McpServer.Services;

/// <summary>
/// Information about a discovered log file.
/// </summary>
public class LogFileInfo
{
    public string Path { get; set; } = "";
    public string Name { get; set; } = "";
    public long SizeBytes { get; set; }
    public DateTime LastModified { get; set; }
    public string? Category { get; set; }
}

/// <summary>
/// Result of reading a log file.
/// </summary>
public class LogFileContent
{
    public string Path { get; set; } = "";
    public int TotalLines { get; set; }
    public int ReturnedLines { get; set; }
    public List<string> Lines { get; set; } = new();
    public string? Error { get; set; }
}

/// <summary>
/// Abstraction for file-based log access.
/// </summary>
public interface IFileLogService
{
    /// <summary>
    /// Discovers log files in common Windows locations.
    /// </summary>
    /// <param name="includeSystemLogs">Include system log locations (requires elevation for some)</param>
    /// <param name="maxFiles">Maximum number of files to return</param>
    IEnumerable<LogFileInfo> DiscoverLogs(bool includeSystemLogs = true, int maxFiles = 100);

    /// <summary>
    /// Reads content from a log file.
    /// </summary>
    /// <param name="path">Path to the log file</param>
    /// <param name="tailLines">Number of lines to read from the end (null = read all, up to maxLines)</param>
    /// <param name="searchPattern">Optional regex pattern to filter lines</param>
    /// <param name="maxLines">Maximum number of lines to return</param>
    LogFileContent ReadLog(string path, int? tailLines = null, string? searchPattern = null, int maxLines = 500);
}
