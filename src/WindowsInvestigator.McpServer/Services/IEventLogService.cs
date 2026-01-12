namespace WindowsInvestigator.McpServer.Services;

/// <summary>
/// Represents a single event log entry returned by the service.
/// </summary>
public class EventLogEntry
{
    public DateTime? TimeCreated { get; set; }
    public string Level { get; set; } = "";
    public string? Source { get; set; }
    public int EventId { get; set; }
    public string Message { get; set; } = "";
}

/// <summary>
/// Abstraction for Windows Event Log access to enable unit testing.
/// </summary>
public interface IEventLogService
{
    /// <summary>
    /// Gets all available event log names on the system.
    /// </summary>
    IEnumerable<string> GetLogNames();

    /// <summary>
    /// Queries events from a specific event log with optional filters.
    /// </summary>
    /// <param name="logName">Name of the event log (e.g., System, Application)</param>
    /// <param name="level">Optional filter by level (Critical, Error, Warning, Information, Verbose)</param>
    /// <param name="source">Optional filter by event source/provider</param>
    /// <param name="maxResults">Maximum number of results to return</param>
    /// <param name="reverseChronological">If true, returns most recent events first (default: true)</param>
    /// <param name="startTime">Optional filter for events after this time (inclusive)</param>
    /// <param name="endTime">Optional filter for events before this time (inclusive)</param>
    IEnumerable<EventLogEntry> QueryEvents(
        string logName, 
        string? level = null, 
        string? source = null, 
        int maxResults = 50, 
        bool reverseChronological = true,
        DateTime? startTime = null,
        DateTime? endTime = null);
}
