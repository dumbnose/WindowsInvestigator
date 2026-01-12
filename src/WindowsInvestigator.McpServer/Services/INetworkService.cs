namespace WindowsInvestigator.McpServer.Services;

/// <summary>
/// Network connectivity test result.
/// </summary>
public class ConnectivityResult
{
    public string Host { get; set; } = "";
    public int Port { get; set; }
    public bool TcpConnected { get; set; }
    public int LatencyMs { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// DNS resolution result.
/// </summary>
public class DnsResult
{
    public string HostName { get; set; } = "";
    public List<string> IpAddresses { get; set; } = new();
    public string? Error { get; set; }
}

/// <summary>
/// Abstraction for network diagnostics.
/// </summary>
public interface INetworkService
{
    /// <summary>
    /// Tests TCP connectivity to a host and port.
    /// </summary>
    ConnectivityResult TestConnection(string host, int port, int timeoutMs = 5000);

    /// <summary>
    /// Resolves a hostname to IP addresses.
    /// </summary>
    DnsResult ResolveDns(string hostName);

    /// <summary>
    /// Gets the current network adapter information.
    /// </summary>
    IEnumerable<NetworkAdapterInfo> GetNetworkAdapters();
}

/// <summary>
/// Network adapter information.
/// </summary>
public class NetworkAdapterInfo
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Status { get; set; } = "";
    public List<string> IpAddresses { get; set; } = new();
    public string? MacAddress { get; set; }
}
