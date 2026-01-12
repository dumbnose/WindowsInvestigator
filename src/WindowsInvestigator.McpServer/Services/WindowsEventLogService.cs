using System.Diagnostics.Eventing.Reader;

namespace WindowsInvestigator.McpServer.Services;

/// <summary>
/// Windows Event Log service implementation using the Windows Event Log API.
/// </summary>
public class WindowsEventLogService : IEventLogService
{
    public IEnumerable<string> GetLogNames()
    {
        using var session = new EventLogSession();
        return session.GetLogNames().OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public IEnumerable<EventLogEntry> QueryEvents(string logName, string? level = null, string? source = null, int maxResults = 50)
    {
        var query = BuildQuery(level, source);
        
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

    private static string BuildQuery(string? level, string? source)
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
