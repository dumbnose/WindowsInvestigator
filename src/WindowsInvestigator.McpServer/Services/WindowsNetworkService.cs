using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace WindowsInvestigator.McpServer.Services;

/// <summary>
/// Windows network service implementation.
/// </summary>
public class WindowsNetworkService : INetworkService
{
    public ConnectivityResult TestConnection(string host, int port, int timeoutMs = 5000)
    {
        var result = new ConnectivityResult
        {
            Host = host,
            Port = port
        };

        try
        {
            using var client = new TcpClient();
            var stopwatch = Stopwatch.StartNew();
            
            var connectTask = client.ConnectAsync(host, port);
            if (connectTask.Wait(timeoutMs))
            {
                stopwatch.Stop();
                result.TcpConnected = client.Connected;
                result.LatencyMs = (int)stopwatch.ElapsedMilliseconds;
            }
            else
            {
                result.TcpConnected = false;
                result.Error = "Connection timed out";
            }
        }
        catch (Exception ex)
        {
            result.TcpConnected = false;
            result.Error = ex.Message;
        }

        return result;
    }

    public DnsResult ResolveDns(string hostName)
    {
        var result = new DnsResult { HostName = hostName };

        try
        {
            var addresses = Dns.GetHostAddresses(hostName);
            result.IpAddresses = addresses.Select(a => a.ToString()).ToList();
        }
        catch (Exception ex)
        {
            result.Error = ex.Message;
        }

        return result;
    }

    public IEnumerable<NetworkAdapterInfo> GetNetworkAdapters()
    {
        var adapters = new List<NetworkAdapterInfo>();

        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            var info = new NetworkAdapterInfo
            {
                Name = nic.Name,
                Description = nic.Description,
                Status = nic.OperationalStatus.ToString(),
                MacAddress = nic.GetPhysicalAddress().ToString()
            };

            var ipProps = nic.GetIPProperties();
            info.IpAddresses = ipProps.UnicastAddresses
                .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork || 
                            a.Address.AddressFamily == AddressFamily.InterNetworkV6)
                .Select(a => a.Address.ToString())
                .ToList();

            adapters.Add(info);
        }

        return adapters.OrderBy(a => a.Name).ToList();
    }
}
