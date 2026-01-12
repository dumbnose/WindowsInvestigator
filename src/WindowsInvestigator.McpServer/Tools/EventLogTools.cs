using ModelContextProtocol.Server;
using System.ComponentModel;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.Tools;

[McpServerToolType]
public sealed class EventLogTools
{
    private readonly IEventLogService _eventLogService;

    public EventLogTools(IEventLogService eventLogService)
    {
        _eventLogService = eventLogService;
    }

    [McpServerTool, Description("Lists all available Windows Event Log names")]
    public IEnumerable<string> ListEventLogs()
    {
        return _eventLogService.GetLogNames();
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
        return _eventLogService.QueryEvents(logName, level, source, maxResults, reverseChronological, startTime, endTime);
    }
}
