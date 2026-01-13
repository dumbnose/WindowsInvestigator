namespace WindowsInvestigator.McpServer.Services;

/// <summary>
/// Reliability event types.
/// </summary>
public enum ReliabilityEventType
{
    ApplicationCrash,
    ApplicationHang,
    WindowsFailure,
    HardwareFailure,
    MiscellaneousFailure,
    DriverInstall,
    ApplicationInstall,
    ApplicationUninstall,
    WindowsUpdate
}

/// <summary>
/// Reliability event from Reliability Monitor.
/// </summary>
public class ReliabilityEvent
{
    public DateTime Timestamp { get; set; }
    public ReliabilityEventType EventType { get; set; }
    public string Source { get; set; } = "";
    public string? Description { get; set; }
    public string? FaultingModule { get; set; }
    public string? ExceptionCode { get; set; }
    public string? Version { get; set; }
    public bool IsSuccess { get; set; }
}

/// <summary>
/// Daily reliability score.
/// </summary>
public class ReliabilityScore
{
    public DateTime Date { get; set; }
    public double Score { get; set; }
    public int ApplicationCrashes { get; set; }
    public int ApplicationHangs { get; set; }
    public int WindowsFailures { get; set; }
    public int MiscellaneousFailures { get; set; }
}

/// <summary>
/// Abstraction for Reliability Monitor data.
/// </summary>
public interface IReliabilityService
{
    /// <summary>
    /// Gets reliability events within a time range.
    /// </summary>
    IEnumerable<ReliabilityEvent> GetReliabilityEvents(DateTime? startTime = null, DateTime? endTime = null, int maxResults = 50);

    /// <summary>
    /// Gets application crashes.
    /// </summary>
    IEnumerable<ReliabilityEvent> GetApplicationCrashes(int maxResults = 20);

    /// <summary>
    /// Gets application hangs.
    /// </summary>
    IEnumerable<ReliabilityEvent> GetApplicationHangs(int maxResults = 20);

    /// <summary>
    /// Gets Windows system failures (BSODs, unexpected shutdowns).
    /// </summary>
    IEnumerable<ReliabilityEvent> GetSystemFailures(int maxResults = 20);

    /// <summary>
    /// Gets daily reliability scores for a time range.
    /// </summary>
    IEnumerable<ReliabilityScore> GetReliabilityScores(int days = 30);
}
