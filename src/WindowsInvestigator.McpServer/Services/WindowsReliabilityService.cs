using System.Diagnostics.Eventing.Reader;

namespace WindowsInvestigator.McpServer.Services;

/// <summary>
/// Windows implementation of Reliability Monitor data access.
/// </summary>
public class WindowsReliabilityService : IReliabilityService
{
    // Event log and event IDs for reliability data
    private const string ApplicationLog = "Application";
    private const string SystemLog = "System";

    public IEnumerable<ReliabilityEvent> GetReliabilityEvents(DateTime? startTime = null, DateTime? endTime = null, int maxResults = 50)
    {
        var results = new List<ReliabilityEvent>();
        var start = startTime ?? DateTime.Now.AddDays(-30);
        var end = endTime ?? DateTime.Now;

        // Get application crashes
        results.AddRange(GetApplicationCrashesInternal(start, end, maxResults / 4));
        
        // Get application hangs
        results.AddRange(GetApplicationHangsInternal(start, end, maxResults / 4));
        
        // Get system failures
        results.AddRange(GetSystemFailuresInternal(start, end, maxResults / 4));
        
        // Get miscellaneous failures (WER events)
        results.AddRange(GetWerEventsInternal(start, end, maxResults / 4));

        return results.OrderByDescending(e => e.Timestamp).Take(maxResults).ToList();
    }

    public IEnumerable<ReliabilityEvent> GetApplicationCrashes(int maxResults = 20)
    {
        return GetApplicationCrashesInternal(DateTime.Now.AddDays(-30), DateTime.Now, maxResults);
    }

    public IEnumerable<ReliabilityEvent> GetApplicationHangs(int maxResults = 20)
    {
        return GetApplicationHangsInternal(DateTime.Now.AddDays(-30), DateTime.Now, maxResults);
    }

    public IEnumerable<ReliabilityEvent> GetSystemFailures(int maxResults = 20)
    {
        return GetSystemFailuresInternal(DateTime.Now.AddDays(-30), DateTime.Now, maxResults);
    }

    public IEnumerable<ReliabilityScore> GetReliabilityScores(int days = 30)
    {
        var scores = new List<ReliabilityScore>();
        var events = GetReliabilityEvents(DateTime.Now.AddDays(-days), DateTime.Now, 1000).ToList();

        // Group events by date and calculate scores
        var groupedEvents = events.GroupBy(e => e.Timestamp.Date);

        foreach (var group in groupedEvents.OrderByDescending(g => g.Key))
        {
            var dayEvents = group.ToList();
            var score = new ReliabilityScore
            {
                Date = group.Key,
                ApplicationCrashes = dayEvents.Count(e => e.EventType == ReliabilityEventType.ApplicationCrash),
                ApplicationHangs = dayEvents.Count(e => e.EventType == ReliabilityEventType.ApplicationHang),
                WindowsFailures = dayEvents.Count(e => e.EventType == ReliabilityEventType.WindowsFailure),
                MiscellaneousFailures = dayEvents.Count(e => e.EventType == ReliabilityEventType.MiscellaneousFailure)
            };

            // Calculate a simple reliability score (10 = perfect, decreases with failures)
            var totalFailures = score.ApplicationCrashes + score.ApplicationHangs + 
                               score.WindowsFailures * 2 + score.MiscellaneousFailures;
            score.Score = Math.Max(0, 10 - totalFailures);

            scores.Add(score);
        }

        return scores;
    }

    private List<ReliabilityEvent> GetApplicationCrashesInternal(DateTime start, DateTime end, int maxResults)
    {
        var results = new List<ReliabilityEvent>();

        try
        {
            // Query Application Error events (Event ID 1000)
            var query = $@"*[System[Provider[@Name='Application Error'] and (EventID=1000) and TimeCreated[@SystemTime >= '{start:yyyy-MM-ddTHH:mm:ss}Z' and @SystemTime <= '{end:yyyy-MM-ddTHH:mm:ss}Z']]]";
            
            using var logReader = new EventLogReader(new EventLogQuery(ApplicationLog, PathType.LogName, query));
            
            EventRecord? record;
            while ((record = logReader.ReadEvent()) != null && results.Count < maxResults)
            {
                using (record)
                {
                    var props = record.Properties;
                    results.Add(new ReliabilityEvent
                    {
                        Timestamp = record.TimeCreated ?? DateTime.MinValue,
                        EventType = ReliabilityEventType.ApplicationCrash,
                        Source = props.Count > 0 ? props[0].Value?.ToString() ?? "" : "",
                        FaultingModule = props.Count > 3 ? props[3].Value?.ToString() : null,
                        ExceptionCode = props.Count > 6 ? props[6].Value?.ToString() : null,
                        Version = props.Count > 1 ? props[1].Value?.ToString() : null,
                        Description = $"Application crashed: {(props.Count > 0 ? props[0].Value : "Unknown")}",
                        IsSuccess = false
                    });
                }
            }
        }
        catch { }

        return results;
    }

    private List<ReliabilityEvent> GetApplicationHangsInternal(DateTime start, DateTime end, int maxResults)
    {
        var results = new List<ReliabilityEvent>();

        try
        {
            // Query Application Hang events (Event ID 1002)
            var query = $@"*[System[Provider[@Name='Application Hang'] and (EventID=1002) and TimeCreated[@SystemTime >= '{start:yyyy-MM-ddTHH:mm:ss}Z' and @SystemTime <= '{end:yyyy-MM-ddTHH:mm:ss}Z']]]";
            
            using var logReader = new EventLogReader(new EventLogQuery(ApplicationLog, PathType.LogName, query));
            
            EventRecord? record;
            while ((record = logReader.ReadEvent()) != null && results.Count < maxResults)
            {
                using (record)
                {
                    var props = record.Properties;
                    results.Add(new ReliabilityEvent
                    {
                        Timestamp = record.TimeCreated ?? DateTime.MinValue,
                        EventType = ReliabilityEventType.ApplicationHang,
                        Source = props.Count > 0 ? props[0].Value?.ToString() ?? "" : "",
                        Version = props.Count > 1 ? props[1].Value?.ToString() : null,
                        Description = $"Application stopped responding: {(props.Count > 0 ? props[0].Value : "Unknown")}",
                        IsSuccess = false
                    });
                }
            }
        }
        catch { }

        return results;
    }

    private List<ReliabilityEvent> GetSystemFailuresInternal(DateTime start, DateTime end, int maxResults)
    {
        var results = new List<ReliabilityEvent>();

        try
        {
            // Query BugCheck/BSOD events (Event ID 1001 from BugCheck, 41 from Kernel-Power)
            var query = $@"*[System[(Provider[@Name='Microsoft-Windows-WER-SystemErrorReporting'] or (Provider[@Name='Microsoft-Windows-Kernel-Power'] and EventID=41)) and TimeCreated[@SystemTime >= '{start:yyyy-MM-ddTHH:mm:ss}Z' and @SystemTime <= '{end:yyyy-MM-ddTHH:mm:ss}Z']]]";
            
            using var logReader = new EventLogReader(new EventLogQuery(SystemLog, PathType.LogName, query));
            
            EventRecord? record;
            while ((record = logReader.ReadEvent()) != null && results.Count < maxResults)
            {
                using (record)
                {
                    results.Add(new ReliabilityEvent
                    {
                        Timestamp = record.TimeCreated ?? DateTime.MinValue,
                        EventType = ReliabilityEventType.WindowsFailure,
                        Source = record.ProviderName ?? "",
                        Description = record.FormatDescription() ?? "System failure occurred",
                        IsSuccess = false
                    });
                }
            }
        }
        catch { }

        return results;
    }

    private List<ReliabilityEvent> GetWerEventsInternal(DateTime start, DateTime end, int maxResults)
    {
        var results = new List<ReliabilityEvent>();

        try
        {
            // Query Windows Error Reporting events
            var query = $@"*[System[Provider[@Name='Windows Error Reporting'] and TimeCreated[@SystemTime >= '{start:yyyy-MM-ddTHH:mm:ss}Z' and @SystemTime <= '{end:yyyy-MM-ddTHH:mm:ss}Z']]]";
            
            using var logReader = new EventLogReader(new EventLogQuery(ApplicationLog, PathType.LogName, query));
            
            EventRecord? record;
            while ((record = logReader.ReadEvent()) != null && results.Count < maxResults)
            {
                using (record)
                {
                    var props = record.Properties;
                    results.Add(new ReliabilityEvent
                    {
                        Timestamp = record.TimeCreated ?? DateTime.MinValue,
                        EventType = ReliabilityEventType.MiscellaneousFailure,
                        Source = "Windows Error Reporting",
                        Description = record.FormatDescription() ?? "Error reported to WER",
                        IsSuccess = false
                    });
                }
            }
        }
        catch { }

        return results;
    }
}
