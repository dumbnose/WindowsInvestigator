namespace WindowsInvestigator.McpServer.Services;

/// <summary>
/// Windows Update information.
/// </summary>
public class WindowsUpdateInfo
{
    public string UpdateId { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public DateTime? InstalledOn { get; set; }
    public string? Result { get; set; }
    public string? KBArticleId { get; set; }
    public string? SupportUrl { get; set; }
    public string? Category { get; set; }
    public bool IsInstalled { get; set; }
    public bool IsMandatory { get; set; }
}

/// <summary>
/// Windows Update status.
/// </summary>
public class WindowsUpdateStatus
{
    public DateTime LastCheckTime { get; set; }
    public DateTime? LastInstallTime { get; set; }
    public bool IsRebootRequired { get; set; }
    public int PendingUpdatesCount { get; set; }
    public int InstalledUpdatesCount { get; set; }
    public string? LastError { get; set; }
}

/// <summary>
/// Windows Update failure information.
/// </summary>
public class WindowsUpdateFailure
{
    public string UpdateId { get; set; } = "";
    public string Title { get; set; } = "";
    public DateTime FailureTime { get; set; }
    public string ErrorCode { get; set; } = "";
    public string? ErrorDescription { get; set; }
}

/// <summary>
/// Abstraction for Windows Update information.
/// </summary>
public interface IWindowsUpdateService
{
    /// <summary>
    /// Gets the current Windows Update status.
    /// </summary>
    WindowsUpdateStatus GetUpdateStatus();

    /// <summary>
    /// Gets the history of installed updates.
    /// </summary>
    IEnumerable<WindowsUpdateInfo> GetUpdateHistory(int maxResults = 50);

    /// <summary>
    /// Gets pending updates waiting to be installed.
    /// </summary>
    IEnumerable<WindowsUpdateInfo> GetPendingUpdates();

    /// <summary>
    /// Gets recent update failures.
    /// </summary>
    IEnumerable<WindowsUpdateFailure> GetUpdateFailures(int maxResults = 20);
}
