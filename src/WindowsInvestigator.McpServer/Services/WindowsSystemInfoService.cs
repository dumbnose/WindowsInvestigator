using System.Management;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace WindowsInvestigator.McpServer.Services;

/// <summary>
/// Windows system information service implementation.
/// </summary>
public class WindowsSystemInfoService : ISystemInfoService
{
    public SystemInfo GetSystemInfo()
    {
        var info = new SystemInfo
        {
            ComputerName = Environment.MachineName,
            UserName = Environment.UserName,
            UserDomain = Environment.UserDomainName,
            ProcessorCount = Environment.ProcessorCount,
            OsArchitecture = RuntimeInformation.OSArchitecture.ToString()
        };

        // Get OS info from registry
        using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
        if (key != null)
        {
            info.OsName = key.GetValue("ProductName")?.ToString() ?? "Windows";
            info.OsBuild = key.GetValue("CurrentBuild")?.ToString() ?? "";
            var ubr = key.GetValue("UBR");
            if (ubr != null)
            {
                info.OsBuild += $".{ubr}";
            }
            info.OsVersion = key.GetValue("DisplayVersion")?.ToString() ?? "";
        }

        // Get boot time and memory using WMI
        try
        {
            using var osSearcher = new ManagementObjectSearcher("SELECT LastBootUpTime, TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");
            foreach (ManagementObject os in osSearcher.Get())
            {
                var lastBootStr = os["LastBootUpTime"]?.ToString();
                if (!string.IsNullOrEmpty(lastBootStr))
                {
                    info.LastBootTime = ManagementDateTimeConverter.ToDateTime(lastBootStr);
                    info.Uptime = DateTime.Now - info.LastBootTime;
                }

                if (os["TotalVisibleMemorySize"] != null)
                {
                    info.TotalMemoryMB = Convert.ToInt64(os["TotalVisibleMemorySize"]) / 1024;
                }

                if (os["FreePhysicalMemory"] != null)
                {
                    info.AvailableMemoryMB = Convert.ToInt64(os["FreePhysicalMemory"]) / 1024;
                }
            }
        }
        catch
        {
            // WMI might not be available, continue with partial info
        }

        return info;
    }
}
