using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using WindowsInvestigator.McpServer.Exceptions;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.Tools;

[McpServerToolType]
public sealed class EventLogTools
{
    private readonly IEventLogService _eventLogService;
    private readonly ILogger<EventLogTools> _logger;

    public EventLogTools(IEventLogService eventLogService, ILogger<EventLogTools> logger)
    {
        _eventLogService = eventLogService;
        _logger = logger;
    }

    [McpServerTool, Description("Lists all available Windows Event Log names")]
    public IEnumerable<string> ListEventLogs()
    {
        try
        {
            _logger.LogDebug("Listing event logs");
            var logs = _eventLogService.GetLogNames().ToList();
            _logger.LogInformation("Found {Count} event logs", logs.Count);
            return logs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list event logs");
            throw new WindowsApiException("EventLogSession.GetLogNames", ex);
        }
    }

    [McpServerTool, Description("Queries events from a Windows Event Log")]
    public IEnumerable<EventLogEntry> QueryEventLog(
        [Description("Name of the event log (e.g., System, Application, Security)")] string logName,
        [Description("Filter by level: Critical, Error, Warning, Information, Verbose")] string? level = null,
        [Description("Filter by event source/provider name")] string? source = null,
        [Description("Maximum number of events to return")] int maxResults = 50,
        [Description("If true, returns most recent events first (default: true)")] bool reverseChronological = true,
        [Description("Filter for events after this time (ISO 8601 format, e.g., 2026-01-12T10:00:00)")] DateTime? startTime = null,
        [Description("Filter for events before this time (ISO 8601 format, e.g., 2026-01-12T12:00:00)")] DateTime? endTime = null)
    {
        if (string.IsNullOrWhiteSpace(logName))
        {
            throw new InvalidParameterException("logName", "Log name cannot be empty");
        }

        if (maxResults <= 0)
        {
            throw new InvalidParameterException("maxResults", "Must be greater than 0");
        }

        if (startTime.HasValue && endTime.HasValue && startTime > endTime)
        {
            throw new InvalidParameterException("startTime/endTime", "Start time must be before end time");
        }

        try
        {
            _logger.LogDebug("Querying event log {LogName} (level={Level}, source={Source}, max={MaxResults})", 
                logName, level ?? "any", source ?? "any", maxResults);
            
            var events = _eventLogService.QueryEvents(logName, level, source, maxResults, reverseChronological, startTime, endTime).ToList();
            
            _logger.LogInformation("Retrieved {Count} events from {LogName}", events.Count, logName);
            return events;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to query event log {LogName}", logName);
            
            if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) || 
                ex.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase))
            {
                throw new ResourceNotFoundException("Event log", logName);
            }
            
            throw new WindowsApiException("EventLogReader", ex);
        }
    }
}
