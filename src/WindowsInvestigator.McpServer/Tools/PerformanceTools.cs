using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using WindowsInvestigator.McpServer.Exceptions;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.Tools;

[McpServerToolType]
public sealed class PerformanceTools
{
    private readonly IPerformanceService _performanceService;
    private readonly ILogger<PerformanceTools> _logger;

    public PerformanceTools(IPerformanceService performanceService, ILogger<PerformanceTools> logger)
    {
        _performanceService = performanceService;
        _logger = logger;
    }

    [McpServerTool, Description("Gets a snapshot of current system performance including CPU, memory, disk, and network usage")]
    public PerformanceSnapshot GetPerformanceSnapshot()
    {
        try
        {
            _logger.LogDebug("Getting performance snapshot");
            var snapshot = _performanceService.GetPerformanceSnapshot();
            _logger.LogInformation("Performance snapshot: CPU {Cpu}%, Memory {Memory}%, {Processes} processes",
                snapshot.CpuUsagePercent.ToString("F1"), snapshot.MemoryUsagePercent.ToString("F1"), snapshot.ProcessCount);
            return snapshot;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get performance snapshot");
            throw new WindowsApiException("PerformanceCounter", ex);
        }
    }

    [McpServerTool, Description("Lists available performance counter categories")]
    public IEnumerable<PerformanceCategory> ListPerformanceCategories()
    {
        try
        {
            _logger.LogDebug("Listing performance categories");
            var categories = _performanceService.GetCategories().ToList();
            _logger.LogInformation("Found {Count} performance counter categories", categories.Count);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list performance categories");
            throw new WindowsApiException("PerformanceCounterCategory", ex);
        }
    }

    [McpServerTool, Description("Gets the value of a specific performance counter")]
    public PerformanceCounterValue? GetPerformanceCounter(
        [Description("The performance counter category name (e.g., 'Processor', 'Memory', 'PhysicalDisk')")] string categoryName,
        [Description("The counter name within the category (e.g., '% Processor Time', 'Available Bytes')")] string counterName,
        [Description("Optional instance name (e.g., '_Total' for all instances, or a specific instance)")] string? instanceName = null)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            throw new InvalidParameterException("categoryName", "Category name cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(counterName))
        {
            throw new InvalidParameterException("counterName", "Counter name cannot be empty");
        }

        try
        {
            _logger.LogDebug("Getting counter {Category}\\{Counter}\\{Instance}", categoryName, counterName, instanceName ?? "(default)");
            var value = _performanceService.GetCounterValue(categoryName, counterName, instanceName);
            
            if (value != null)
            {
                _logger.LogInformation("Counter {Category}\\{Counter} = {Value}", categoryName, counterName, value.Value);
            }
            else
            {
                _logger.LogWarning("Counter {Category}\\{Counter} not found", categoryName, counterName);
            }
            
            return value;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to get counter {Category}\\{Counter}", categoryName, counterName);
            throw new WindowsApiException("PerformanceCounter", ex);
        }
    }

    [McpServerTool, Description("Gets all counter values for a performance category")]
    public IEnumerable<PerformanceCounterValue> GetCategoryCounters(
        [Description("The performance counter category name")] string categoryName,
        [Description("Optional instance name")] string? instanceName = null)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            throw new InvalidParameterException("categoryName", "Category name cannot be empty");
        }

        try
        {
            _logger.LogDebug("Getting all counters for category {Category}", categoryName);
            var counters = _performanceService.GetCategoryCounters(categoryName, instanceName).ToList();
            _logger.LogInformation("Found {Count} counters in category {Category}", counters.Count, categoryName);
            return counters;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to get counters for category {Category}", categoryName);
            throw new WindowsApiException("PerformanceCounterCategory", ex);
        }
    }
}
