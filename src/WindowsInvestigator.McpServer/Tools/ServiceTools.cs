using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.RegularExpressions;
using WindowsInvestigator.McpServer.Exceptions;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.Tools;

[McpServerToolType]
public sealed class ServiceTools
{
    private readonly IServiceInfoService _serviceInfoService;
    private readonly ILogger<ServiceTools> _logger;

    public ServiceTools(IServiceInfoService serviceInfoService, ILogger<ServiceTools> logger)
    {
        _serviceInfoService = serviceInfoService;
        _logger = logger;
    }

    [McpServerTool, Description("Lists all Windows services on the system")]
    public IEnumerable<ServiceInfo> ListServices()
    {
        try
        {
            _logger.LogDebug("Listing all Windows services");
            var services = _serviceInfoService.GetAllServices().ToList();
            _logger.LogInformation("Found {Count} Windows services", services.Count);
            return services;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list Windows services");
            throw new WindowsApiException("ServiceController.GetServices", ex);
        }
    }

    [McpServerTool, Description("Gets information about a specific Windows service")]
    public ServiceInfo? GetService(
        [Description("The name of the service (e.g., Spooler, wuauserv)")] string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            throw new InvalidParameterException("serviceName", "Service name cannot be empty");
        }

        try
        {
            _logger.LogDebug("Getting service info for {ServiceName}", serviceName);
            var service = _serviceInfoService.GetService(serviceName);
            
            if (service == null)
            {
                _logger.LogWarning("Service {ServiceName} not found", serviceName);
            }
            else
            {
                _logger.LogInformation("Retrieved service {ServiceName} (Status: {Status})", serviceName, service.Status);
            }
            
            return service;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to get service {ServiceName}", serviceName);
            throw new WindowsApiException("ServiceController", ex);
        }
    }

    [McpServerTool, Description("Searches for services matching a name pattern")]
    public IEnumerable<ServiceInfo> SearchServices(
        [Description("Regex pattern to match service name or display name")] string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new InvalidParameterException("pattern", "Search pattern cannot be empty");
        }

        // Validate regex pattern
        try
        {
            _ = new Regex(pattern, RegexOptions.IgnoreCase);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidParameterException("pattern", $"Invalid regex: {ex.Message}");
        }

        try
        {
            _logger.LogDebug("Searching for services matching pattern '{Pattern}'", pattern);
            var services = _serviceInfoService.SearchServices(pattern).ToList();
            _logger.LogInformation("Found {Count} services matching pattern '{Pattern}'", services.Count, pattern);
            return services;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to search services with pattern '{Pattern}'", pattern);
            throw new WindowsApiException("ServiceController", ex);
        }
    }
}
