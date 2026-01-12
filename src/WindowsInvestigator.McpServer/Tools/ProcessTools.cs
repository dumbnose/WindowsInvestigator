using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.RegularExpressions;
using WindowsInvestigator.McpServer.Exceptions;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.Tools;

[McpServerToolType]
public sealed class ProcessTools
{
    private readonly IProcessService _processService;
    private readonly ILogger<ProcessTools> _logger;

    public ProcessTools(IProcessService processService, ILogger<ProcessTools> logger)
    {
        _processService = processService;
        _logger = logger;
    }

    [McpServerTool, Description("Lists all running processes on the system")]
    public IEnumerable<ProcessInfo> ListProcesses()
    {
        try
        {
            _logger.LogDebug("Listing all processes");
            var processes = _processService.GetProcesses().ToList();
            _logger.LogInformation("Found {Count} running processes", processes.Count);
            return processes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list processes");
            throw new WindowsApiException("Process.GetProcesses", ex);
        }
    }

    [McpServerTool, Description("Gets information about a specific process by ID")]
    public ProcessInfo? GetProcess(
        [Description("The process ID")] int processId)
    {
        if (processId <= 0)
        {
            throw new InvalidParameterException("processId", "Process ID must be greater than 0");
        }

        try
        {
            _logger.LogDebug("Getting process info for PID {ProcessId}", processId);
            var process = _processService.GetProcess(processId);
            
            if (process == null)
            {
                _logger.LogWarning("Process {ProcessId} not found", processId);
            }
            else
            {
                _logger.LogInformation("Retrieved process {ProcessName} (PID: {ProcessId})", process.Name, processId);
            }
            
            return process;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to get process {ProcessId}", processId);
            throw new WindowsApiException("Process.GetProcessById", ex);
        }
    }

    [McpServerTool, Description("Searches for processes matching a pattern")]
    public IEnumerable<ProcessInfo> SearchProcesses(
        [Description("Regex pattern to match process name or command line")] string pattern)
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
            _logger.LogDebug("Searching processes for pattern '{Pattern}'", pattern);
            var processes = _processService.SearchProcesses(pattern).ToList();
            _logger.LogInformation("Found {Count} processes matching pattern '{Pattern}'", processes.Count, pattern);
            return processes;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to search processes with pattern '{Pattern}'", pattern);
            throw new WindowsApiException("Process.SearchProcesses", ex);
        }
    }

    [McpServerTool, Description("Gets a summary of system resource usage including top CPU and memory consumers")]
    public ProcessSummary GetProcessSummary()
    {
        try
        {
            _logger.LogDebug("Getting process summary");
            var summary = _processService.GetProcessSummary();
            _logger.LogInformation("Process summary: {TotalProcesses} processes, {TotalThreads} threads, {TotalMemoryMB} MB working set",
                summary.TotalProcesses, summary.TotalThreads, summary.TotalWorkingSetBytes / (1024 * 1024));
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get process summary");
            throw new WindowsApiException("Process.GetProcessSummary", ex);
        }
    }
}
