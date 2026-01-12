namespace WindowsInvestigator.McpServer.Services;

/// <summary>
/// Windows service information.
/// </summary>
public class ServiceInfo
{
    public string Name { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Status { get; set; } = "";
    public string StartType { get; set; } = "";
    public string? Description { get; set; }
    public string? PathName { get; set; }
    public string? ServiceAccount { get; set; }
}

/// <summary>
/// Abstraction for Windows service access.
/// </summary>
public interface IServiceInfoService
{
    /// <summary>
    /// Gets all services on the system.
    /// </summary>
    IEnumerable<ServiceInfo> GetAllServices();

    /// <summary>
    /// Gets a specific service by name.
    /// </summary>
    ServiceInfo? GetService(string serviceName);

    /// <summary>
    /// Gets services matching a name pattern.
    /// </summary>
    IEnumerable<ServiceInfo> SearchServices(string namePattern);
}
