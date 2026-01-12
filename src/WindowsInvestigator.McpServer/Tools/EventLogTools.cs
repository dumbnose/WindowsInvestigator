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
        [Description("Filter by event source")] string? source = null,
        [Description("Maximum number of events to return")] int maxResults = 50)
    {
        return _eventLogService.QueryEvents(logName, level, source, maxResults);
    }
}
