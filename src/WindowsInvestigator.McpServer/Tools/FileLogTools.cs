using ModelContextProtocol.Server;
using System.ComponentModel;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.Tools;

[McpServerToolType]
public sealed class FileLogTools
{
    private readonly IFileLogService _fileLogService;

    public FileLogTools(IFileLogService fileLogService)
    {
        _fileLogService = fileLogService;
    }

    [McpServerTool, Description("Discovers log files in common Windows locations")]
    public IEnumerable<LogFileInfo> DiscoverLogs(
        [Description("Include system log locations like C:\\Windows\\Logs (default: true)")] bool includeSystemLogs = true,
        [Description("Maximum number of files to return (default: 100)")] int maxFiles = 100)
    {
        return _fileLogService.DiscoverLogs(includeSystemLogs, maxFiles);
    }

    [McpServerTool, Description("Reads content from a log file")]
    public LogFileContent ReadLog(
        [Description("Full path to the log file")] string path,
        [Description("Number of lines to read from end of file (null = read from start)")] int? tailLines = null,
        [Description("Regex pattern to filter lines")] string? searchPattern = null,
        [Description("Maximum number of lines to return (default: 500)")] int maxLines = 500)
    {
        return _fileLogService.ReadLog(path, tailLines, searchPattern, maxLines);
    }
}
