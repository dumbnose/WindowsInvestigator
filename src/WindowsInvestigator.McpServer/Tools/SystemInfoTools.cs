using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using WindowsInvestigator.McpServer.Exceptions;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.Tools;

[McpServerToolType]
public sealed class SystemInfoTools
{
    private readonly ISystemInfoService _systemInfoService;
    private readonly ILogger<SystemInfoTools> _logger;

    public SystemInfoTools(ISystemInfoService systemInfoService, ILogger<SystemInfoTools> logger)
    {
        _systemInfoService = systemInfoService;
        _logger = logger;
    }

    [McpServerTool, Description("Gets comprehensive system information including OS version, hardware, and uptime")]
    public SystemInfo GetSystemInfo()
    {
        try
        {
            _logger.LogDebug("Getting system information");
            var info = _systemInfoService.GetSystemInfo();
            _logger.LogInformation("Retrieved system info for {ComputerName} running {OsName}", 
                info.ComputerName, info.OsName);
            return info;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get system information");
            throw new WindowsApiException("WMI/Registry", ex);
        }
    }
}
