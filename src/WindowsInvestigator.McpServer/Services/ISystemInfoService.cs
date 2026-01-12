namespace WindowsInvestigator.McpServer.Services;

/// <summary>
/// System information data.
/// </summary>
public class SystemInfo
{
    public string ComputerName { get; set; } = "";
    public string OsName { get; set; } = "";
    public string OsVersion { get; set; } = "";
    public string OsBuild { get; set; } = "";
    public string OsArchitecture { get; set; } = "";
    public DateTime LastBootTime { get; set; }
    public TimeSpan Uptime { get; set; }
    public string UserName { get; set; } = "";
    public string UserDomain { get; set; } = "";
    public int ProcessorCount { get; set; }
    public long TotalMemoryMB { get; set; }
    public long AvailableMemoryMB { get; set; }
}

/// <summary>
/// Abstraction for system information access.
/// </summary>
public interface ISystemInfoService
{
    /// <summary>
    /// Gets comprehensive system information.
    /// </summary>
    SystemInfo GetSystemInfo();
}
