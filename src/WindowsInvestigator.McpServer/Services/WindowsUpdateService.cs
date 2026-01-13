using System.Management;

namespace WindowsInvestigator.McpServer.Services;

/// <summary>
/// Windows implementation of Windows Update information access.
/// </summary>
public class WindowsUpdateService : IWindowsUpdateService
{
    public WindowsUpdateStatus GetUpdateStatus()
    {
        var status = new WindowsUpdateStatus
        {
            LastCheckTime = DateTime.MinValue
        };

        try
        {
            // Get update session info from WMI
            using var searcher = new ManagementObjectSearcher(
                "root\\CIMV2",
                "SELECT * FROM Win32_QuickFixEngineering");

            var updates = searcher.Get().Cast<ManagementObject>().ToList();
            status.InstalledUpdatesCount = updates.Count;

            // Get the most recent install time
            var lastInstall = updates
                .Select(u => ParseDateTime(u["InstalledOn"]?.ToString()))
                .Where(d => d.HasValue)
                .OrderByDescending(d => d)
                .FirstOrDefault();

            status.LastInstallTime = lastInstall;
        }
        catch { }

        try
        {
            // Check for pending reboot
            status.IsRebootRequired = CheckRebootRequired();
        }
        catch { }

        return status;
    }

    public IEnumerable<WindowsUpdateInfo> GetUpdateHistory(int maxResults = 50)
    {
        var results = new List<WindowsUpdateInfo>();

        try
        {
            using var searcher = new ManagementObjectSearcher(
                "root\\CIMV2",
                "SELECT * FROM Win32_QuickFixEngineering");

            foreach (ManagementObject update in searcher.Get())
            {
                var info = new WindowsUpdateInfo
                {
                    UpdateId = update["HotFixID"]?.ToString() ?? "",
                    Title = update["Description"]?.ToString() ?? "",
                    KBArticleId = update["HotFixID"]?.ToString(),
                    InstalledOn = ParseDateTime(update["InstalledOn"]?.ToString()),
                    IsInstalled = true,
                    Category = update["Description"]?.ToString()
                };

                if (!string.IsNullOrEmpty(update["Caption"]?.ToString()))
                {
                    info.SupportUrl = update["Caption"]?.ToString();
                }

                results.Add(info);

                if (results.Count >= maxResults)
                    break;
            }
        }
        catch { }

        return results.OrderByDescending(u => u.InstalledOn).ToList();
    }

    public IEnumerable<WindowsUpdateInfo> GetPendingUpdates()
    {
        // Note: Getting pending updates requires the Windows Update Agent API
        // which is COM-based. For now, we'll check the registry for pending updates.
        var results = new List<WindowsUpdateInfo>();

        try
        {
            // Check for pending updates in registry
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired");
            
            if (key != null)
            {
                // There are pending updates requiring reboot
                results.Add(new WindowsUpdateInfo
                {
                    UpdateId = "PendingReboot",
                    Title = "Updates pending reboot",
                    Description = "One or more updates are waiting for system restart to complete installation",
                    IsInstalled = false,
                    IsMandatory = true
                });
            }
        }
        catch { }

        return results;
    }

    public IEnumerable<WindowsUpdateFailure> GetUpdateFailures(int maxResults = 20)
    {
        var results = new List<WindowsUpdateFailure>();

        try
        {
            // Query Windows Update event log for failures
            using var searcher = new ManagementObjectSearcher(
                "root\\CIMV2",
                @"SELECT * FROM Win32_NTLogEvent WHERE Logfile='System' AND SourceName='Microsoft-Windows-WindowsUpdateClient' AND EventType=1");

            foreach (ManagementObject evt in searcher.Get())
            {
                try
                {
                    var failure = new WindowsUpdateFailure
                    {
                        UpdateId = evt["RecordNumber"]?.ToString() ?? "",
                        Title = "Windows Update Error",
                        FailureTime = ManagementDateTimeConverter.ToDateTime(evt["TimeGenerated"]?.ToString() ?? ""),
                        ErrorDescription = evt["Message"]?.ToString()
                    };

                    // Try to extract error code from message
                    var message = evt["Message"]?.ToString() ?? "";
                    var errorMatch = System.Text.RegularExpressions.Regex.Match(message, @"0x[0-9A-Fa-f]+");
                    if (errorMatch.Success)
                    {
                        failure.ErrorCode = errorMatch.Value;
                    }

                    results.Add(failure);

                    if (results.Count >= maxResults)
                        break;
                }
                catch { }
            }
        }
        catch { }

        return results.OrderByDescending(f => f.FailureTime).ToList();
    }

    private static DateTime? ParseDateTime(string? dateStr)
    {
        if (string.IsNullOrEmpty(dateStr))
            return null;

        if (DateTime.TryParse(dateStr, out var result))
            return result;

        // Handle WMI date format
        try
        {
            return ManagementDateTimeConverter.ToDateTime(dateStr);
        }
        catch
        {
            return null;
        }
    }

    private static bool CheckRebootRequired()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired");
            return key != null;
        }
        catch
        {
            return false;
        }
    }
}
