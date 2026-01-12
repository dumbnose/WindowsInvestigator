using ModelContextProtocol.Server;
using System.ComponentModel;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.Tools;

[McpServerToolType]
public sealed class NetworkTools
{
    private readonly INetworkService _networkService;

    public NetworkTools(INetworkService networkService)
    {
        _networkService = networkService;
    }

    [McpServerTool, Description("Tests TCP connectivity to a host and port")]
    public ConnectivityResult TestConnection(
        [Description("The hostname or IP address to connect to")] string host,
        [Description("The TCP port to connect to")] int port,
        [Description("Timeout in milliseconds (default: 5000)")] int timeoutMs = 5000)
    {
        return _networkService.TestConnection(host, port, timeoutMs);
    }

    [McpServerTool, Description("Resolves a hostname to IP addresses")]
    public DnsResult ResolveDns(
        [Description("The hostname to resolve")] string hostName)
    {
        return _networkService.ResolveDns(hostName);
    }

    [McpServerTool, Description("Gets information about all network adapters")]
    public IEnumerable<NetworkAdapterInfo> GetNetworkAdapters()
    {
        return _networkService.GetNetworkAdapters();
    }
}
