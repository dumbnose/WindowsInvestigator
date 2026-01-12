using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;

namespace WindowsInvestigator.McpServer.Tools;

[McpServerToolType]
public sealed class EventLogTools
{
    [McpServerTool, Description("Lists all available Windows Event Log names")]
    public static IEnumerable<string> ListEventLogs()
    {
        using var session = new EventLogSession();
        return session.GetLogNames().OrderBy(n => n).ToList();
    }

    [McpServerTool, Description("Queries events from a Windows Event Log")]
    public static IEnumerable<EventLogEntry> QueryEventLog(
        [Description("Name of the event log (e.g., System, Application, Security)")] string logName,
        [Description("Filter by level: Critical, Error, Warning, Information, Verbose")] string? level = null,
        [Description("Filter by event source")] string? source = null,
        [Description("Maximum number of events to return")] int maxResults = 50)
    {
        var query = BuildQuery(logName, level, source);
        
        using var reader = new EventLogReader(new EventLogQuery(logName, PathType.LogName, query));
        
        var events = new List<EventLogEntry>();
        EventRecord? record;
        
        while ((record = reader.ReadEvent()) != null && events.Count < maxResults)
        {
            events.Add(new EventLogEntry
            {
                TimeCreated = record.TimeCreated,
                Level = GetLevelName(record.Level),
                Source = record.ProviderName,
                EventId = record.Id,
                Message = TryGetMessage(record)
            });
            record.Dispose();
        }
        
        return events;
    }

    private static string BuildQuery(string logName, string? level, string? source)
    {
        var conditions = new List<string>();
        
        if (!string.IsNullOrEmpty(level))
        {
            var levelValue = level.ToLowerInvariant() switch
            {
                "critical" => 1,
                "error" => 2,
                "warning" => 3,
                "information" => 4,
                "verbose" => 5,
                _ => (int?)null
            };
            
            if (levelValue.HasValue)
            {
                conditions.Add($"Level={levelValue}");
            }
        }
        
        if (!string.IsNullOrEmpty(source))
        {
            conditions.Add($"Provider[@Name='{source}']");
        }
        
        if (conditions.Count == 0)
        {
            return "*";
        }
        
        return $"*[System[{string.Join(" and ", conditions)}]]";
    }

    private static string GetLevelName(byte? level) => level switch
    {
        1 => "Critical",
        2 => "Error",
        3 => "Warning",
        4 => "Information",
        5 => "Verbose",
        _ => "Unknown"
    };

    private static string TryGetMessage(EventRecord record)
    {
        try
        {
            return record.FormatDescription() ?? "(No message available)";
        }
        catch
        {
            return "(Unable to format message)";
        }
    }
}

public class EventLogEntry
{
    public DateTime? TimeCreated { get; set; }
    public string Level { get; set; } = "";
    public string? Source { get; set; }
    public int EventId { get; set; }
    public string Message { get; set; } = "";
}
