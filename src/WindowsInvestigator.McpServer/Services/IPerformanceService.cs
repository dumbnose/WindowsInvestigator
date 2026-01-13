namespace WindowsInvestigator.McpServer.Services;

/// <summary>
/// Performance counter value.
/// </summary>
public class PerformanceCounterValue
{
    public string CategoryName { get; set; } = "";
    public string CounterName { get; set; } = "";
    public string InstanceName { get; set; } = "";
    public float Value { get; set; }
    public string? Unit { get; set; }
}

/// <summary>
/// System performance snapshot.
/// </summary>
public class PerformanceSnapshot
{
    public DateTime Timestamp { get; set; }
    public float CpuUsagePercent { get; set; }
    public float MemoryUsagePercent { get; set; }
    public long AvailableMemoryBytes { get; set; }
    public long TotalMemoryBytes { get; set; }
    public float DiskReadBytesPerSec { get; set; }
    public float DiskWriteBytesPerSec { get; set; }
    public float NetworkSentBytesPerSec { get; set; }
    public float NetworkReceivedBytesPerSec { get; set; }
    public int ProcessCount { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
}

/// <summary>
/// Performance counter category information.
/// </summary>
public class PerformanceCategory
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public List<string> Counters { get; set; } = new();
    public List<string> Instances { get; set; } = new();
}

/// <summary>
/// Abstraction for performance counter access.
/// </summary>
public interface IPerformanceService
{
    /// <summary>
    /// Gets a snapshot of current system performance.
    /// </summary>
    PerformanceSnapshot GetPerformanceSnapshot();

    /// <summary>
    /// Lists available performance counter categories.
    /// </summary>
    IEnumerable<PerformanceCategory> GetCategories();

    /// <summary>
    /// Gets the value of a specific performance counter.
    /// </summary>
    PerformanceCounterValue? GetCounterValue(string categoryName, string counterName, string? instanceName = null);

    /// <summary>
    /// Gets all counter values for a category.
    /// </summary>
    IEnumerable<PerformanceCounterValue> GetCategoryCounters(string categoryName, string? instanceName = null);
}
