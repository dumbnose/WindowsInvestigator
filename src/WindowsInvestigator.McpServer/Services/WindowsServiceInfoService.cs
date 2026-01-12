using System.Management;
using System.ServiceProcess;
using System.Text.RegularExpressions;

namespace WindowsInvestigator.McpServer.Services;

/// <summary>
/// Windows service information service implementation.
/// </summary>
public class WindowsServiceInfoService : IServiceInfoService
{
    public IEnumerable<ServiceInfo> GetAllServices()
    {
        return ServiceController.GetServices()
            .Select(MapToServiceInfo)
            .OrderBy(s => s.Name)
            .ToList();
    }

    public ServiceInfo? GetService(string serviceName)
    {
        try
        {
            using var service = new ServiceController(serviceName);
            return MapToServiceInfo(service);
        }
        catch
        {
            return null;
        }
    }

    public IEnumerable<ServiceInfo> SearchServices(string namePattern)
    {
        var regex = new Regex(namePattern, RegexOptions.IgnoreCase);
        return ServiceController.GetServices()
            .Where(s => regex.IsMatch(s.ServiceName) || regex.IsMatch(s.DisplayName))
            .Select(MapToServiceInfo)
            .OrderBy(s => s.Name)
            .ToList();
    }

    private static ServiceInfo MapToServiceInfo(ServiceController service)
    {
        var info = new ServiceInfo
        {
            Name = service.ServiceName,
            DisplayName = service.DisplayName,
            Status = service.Status.ToString(),
            StartType = service.StartType.ToString()
        };

        // Get additional details from WMI
        try
        {
            using var searcher = new ManagementObjectSearcher(
                $"SELECT Description, PathName, StartName FROM Win32_Service WHERE Name = '{service.ServiceName}'");
            
            foreach (ManagementObject svc in searcher.Get())
            {
                info.Description = svc["Description"]?.ToString();
                info.PathName = svc["PathName"]?.ToString();
                info.ServiceAccount = svc["StartName"]?.ToString();
            }
        }
        catch
        {
            // WMI might fail, continue with basic info
        }

        return info;
    }
}
