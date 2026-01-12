using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.RegularExpressions;
using WindowsInvestigator.McpServer.Exceptions;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.Tools;

[McpServerToolType]
public sealed class FileLogTools
{
    private readonly IFileLogService _fileLogService;
    private readonly ILogger<FileLogTools> _logger;

    public FileLogTools(IFileLogService fileLogService, ILogger<FileLogTools> logger)
    {
        _fileLogService = fileLogService;
        _logger = logger;
    }

    [McpServerTool, Description("Discovers log files in common Windows locations")]
    public IEnumerable<LogFileInfo> DiscoverLogs(
        [Description("Include system log locations like C:\\Windows\\Logs (default: true)")] bool includeSystemLogs = true,
        [Description("Maximum number of files to return (default: 100)")] int maxFiles = 100)
    {
        if (maxFiles <= 0)
        {
            throw new InvalidParameterException("maxFiles", "Must be greater than 0");
        }

        try
        {
            _logger.LogDebug("Discovering log files (includeSystemLogs={IncludeSystem}, maxFiles={MaxFiles})", 
                includeSystemLogs, maxFiles);
            var logs = _fileLogService.DiscoverLogs(includeSystemLogs, maxFiles).ToList();
            _logger.LogInformation("Discovered {Count} log files", logs.Count);
            return logs;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to discover log files");
            throw new WindowsApiException("Directory.EnumerateFiles", ex);
        }
    }

    [McpServerTool, Description("Reads content from a log file")]
    public LogFileContent ReadLog(
        [Description("Full path to the log file")] string path,
        [Description("Number of lines to read from end of file (null = read from start)")] int? tailLines = null,
        [Description("Regex pattern to filter lines")] string? searchPattern = null,
        [Description("Maximum number of lines to return (default: 500)")] int maxLines = 500)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidParameterException("path", "Path cannot be empty");
        }

        if (maxLines <= 0)
        {
            throw new InvalidParameterException("maxLines", "Must be greater than 0");
        }

        if (tailLines.HasValue && tailLines.Value <= 0)
        {
            throw new InvalidParameterException("tailLines", "Must be greater than 0");
        }

        // Validate regex pattern if provided
        if (!string.IsNullOrEmpty(searchPattern))
        {
            try
            {
                _ = new Regex(searchPattern, RegexOptions.IgnoreCase);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidParameterException("searchPattern", $"Invalid regex: {ex.Message}");
            }
        }

        try
        {
            _logger.LogDebug("Reading log file {Path} (tail={TailLines}, pattern={Pattern}, max={MaxLines})", 
                path, tailLines?.ToString() ?? "all", searchPattern ?? "none", maxLines);
            
            var result = _fileLogService.ReadLog(path, tailLines, searchPattern, maxLines);
            
            if (!string.IsNullOrEmpty(result.Error))
            {
                _logger.LogWarning("Error reading log file {Path}: {Error}", path, result.Error);
            }
            else
            {
                _logger.LogInformation("Read {Lines} lines from {Path} (total: {TotalLines})", 
                    result.ReturnedLines, path, result.TotalLines);
            }
            
            return result;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to read log file {Path}", path);
            
            if (ex is UnauthorizedAccessException)
            {
                throw new AccessDeniedException(path, ex);
            }
            
            throw new WindowsApiException("File.ReadAllLines", ex);
        }
    }
}
