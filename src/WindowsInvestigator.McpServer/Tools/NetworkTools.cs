using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using WindowsInvestigator.McpServer.Exceptions;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.Tools;

[McpServerToolType]
public sealed class NetworkTools
{
    private readonly INetworkService _networkService;
    private readonly ILogger<NetworkTools> _logger;

    public NetworkTools(INetworkService networkService, ILogger<NetworkTools> logger)
    {
        _networkService = networkService;
        _logger = logger;
    }

    [McpServerTool, Description("Tests TCP connectivity to a host and port")]
    public ConnectivityResult TestConnection(
        [Description("The hostname or IP address to connect to")] string host,
        [Description("The TCP port to connect to")] int port,
        [Description("Timeout in milliseconds (default: 5000)")] int timeoutMs = 5000)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            throw new InvalidParameterException("host", "Host cannot be empty");
        }

        if (port < 1 || port > 65535)
        {
            throw new InvalidParameterException("port", "Port must be between 1 and 65535");
        }

        if (timeoutMs <= 0)
        {
            throw new InvalidParameterException("timeoutMs", "Timeout must be greater than 0");
        }

        try
        {
            _logger.LogDebug("Testing connection to {Host}:{Port} with timeout {TimeoutMs}ms", host, port, timeoutMs);
            var result = _networkService.TestConnection(host, port, timeoutMs);
            
            if (result.TcpConnected)
            {
                _logger.LogInformation("Connection to {Host}:{Port} succeeded in {LatencyMs}ms", host, port, result.LatencyMs);
            }
            else
            {
                _logger.LogWarning("Connection to {Host}:{Port} failed: {Error}", host, port, result.Error);
            }
            
            return result;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to test connection to {Host}:{Port}", host, port);
            throw new WindowsApiException("TcpClient", ex);
        }
    }

    [McpServerTool, Description("Resolves a hostname to IP addresses")]
    public DnsResult ResolveDns(
        [Description("The hostname to resolve")] string hostName)
    {
        if (string.IsNullOrWhiteSpace(hostName))
        {
            throw new InvalidParameterException("hostName", "Hostname cannot be empty");
        }

        try
        {
            _logger.LogDebug("Resolving DNS for {HostName}", hostName);
            var result = _networkService.ResolveDns(hostName);
            
            if (result.Error == null)
            {
                _logger.LogInformation("Resolved {HostName} to {AddressCount} addresses", hostName, result.IpAddresses?.Count ?? 0);
            }
            else
            {
                _logger.LogWarning("DNS resolution for {HostName} failed: {Error}", hostName, result.Error);
            }
            
            return result;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to resolve DNS for {HostName}", hostName);
            throw new WindowsApiException("Dns.GetHostAddresses", ex);
        }
    }

    [McpServerTool, Description("Gets information about all network adapters")]
    public IEnumerable<NetworkAdapterInfo> GetNetworkAdapters()
    {
        try
        {
            _logger.LogDebug("Getting network adapters");
            var adapters = _networkService.GetNetworkAdapters().ToList();
            _logger.LogInformation("Found {Count} network adapters", adapters.Count);
            return adapters;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get network adapters");
            throw new WindowsApiException("NetworkInterface.GetAllNetworkInterfaces", ex);
        }
    }
}
