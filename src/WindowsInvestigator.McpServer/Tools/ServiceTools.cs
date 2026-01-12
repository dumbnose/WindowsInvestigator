using ModelContextProtocol.Server;
using System.ComponentModel;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.Tools;

[McpServerToolType]
public sealed class ServiceTools
{
    private readonly IServiceInfoService _serviceInfoService;

    public ServiceTools(IServiceInfoService serviceInfoService)
    {
        _serviceInfoService = serviceInfoService;
    }

    [McpServerTool, Description("Lists all Windows services on the system")]
    public IEnumerable<ServiceInfo> ListServices()
    {
        return _serviceInfoService.GetAllServices();
    }

    [McpServerTool, Description("Gets information about a specific Windows service")]
    public ServiceInfo? GetService(
        [Description("The name of the service (e.g., Spooler, wuauserv)")] string serviceName)
    {
        return _serviceInfoService.GetService(serviceName);
    }

    [McpServerTool, Description("Searches for services matching a name pattern")]
    public IEnumerable<ServiceInfo> SearchServices(
        [Description("Regex pattern to match service name or display name")] string pattern)
    {
        return _serviceInfoService.SearchServices(pattern);
    }
}
