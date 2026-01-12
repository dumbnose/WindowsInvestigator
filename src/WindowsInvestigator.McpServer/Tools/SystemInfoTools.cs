using ModelContextProtocol.Server;
using System.ComponentModel;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.Tools;

[McpServerToolType]
public sealed class SystemInfoTools
{
    private readonly ISystemInfoService _systemInfoService;

    public SystemInfoTools(ISystemInfoService systemInfoService)
    {
        _systemInfoService = systemInfoService;
    }

    [McpServerTool, Description("Gets comprehensive system information including OS version, hardware, and uptime")]
    public SystemInfo GetSystemInfo()
    {
        return _systemInfoService.GetSystemInfo();
    }
}
