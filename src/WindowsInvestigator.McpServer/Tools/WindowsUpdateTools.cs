using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using WindowsInvestigator.McpServer.Exceptions;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.Tools;

[McpServerToolType]
public sealed class WindowsUpdateTools
{
    private readonly IWindowsUpdateService _updateService;
    private readonly ILogger<WindowsUpdateTools> _logger;

    public WindowsUpdateTools(IWindowsUpdateService updateService, ILogger<WindowsUpdateTools> logger)
    {
        _updateService = updateService;
        _logger = logger;
    }

    [McpServerTool, Description("Gets the current Windows Update status including pending updates and reboot requirements")]
    public WindowsUpdateStatus GetUpdateStatus()
    {
        try
        {
            _logger.LogDebug("Getting Windows Update status");
            var status = _updateService.GetUpdateStatus();
            _logger.LogInformation("Update status: {InstalledCount} installed, {PendingCount} pending, reboot required: {RebootRequired}",
                status.InstalledUpdatesCount, status.PendingUpdatesCount, status.IsRebootRequired);
            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Windows Update status");
            throw new WindowsApiException("WindowsUpdate.GetStatus", ex);
        }
    }

    [McpServerTool, Description("Gets the history of installed Windows updates")]
    public IEnumerable<WindowsUpdateInfo> GetUpdateHistory(
        [Description("Maximum number of updates to return (default: 50)")] int maxResults = 50)
    {
        if (maxResults <= 0)
        {
            throw new InvalidParameterException("maxResults", "Maximum results must be greater than 0");
        }

        try
        {
            _logger.LogDebug("Getting update history (max {MaxResults})", maxResults);
            var updates = _updateService.GetUpdateHistory(maxResults).ToList();
            _logger.LogInformation("Found {Count} installed updates", updates.Count);
            return updates;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to get update history");
            throw new WindowsApiException("WindowsUpdate.GetHistory", ex);
        }
    }

    [McpServerTool, Description("Gets pending Windows updates waiting to be installed")]
    public IEnumerable<WindowsUpdateInfo> GetPendingUpdates()
    {
        try
        {
            _logger.LogDebug("Getting pending updates");
            var updates = _updateService.GetPendingUpdates().ToList();
            _logger.LogInformation("Found {Count} pending updates", updates.Count);
            return updates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get pending updates");
            throw new WindowsApiException("WindowsUpdate.GetPending", ex);
        }
    }

    [McpServerTool, Description("Gets recent Windows Update failures")]
    public IEnumerable<WindowsUpdateFailure> GetUpdateFailures(
        [Description("Maximum number of failures to return (default: 20)")] int maxResults = 20)
    {
        if (maxResults <= 0)
        {
            throw new InvalidParameterException("maxResults", "Maximum results must be greater than 0");
        }

        try
        {
            _logger.LogDebug("Getting update failures (max {MaxResults})", maxResults);
            var failures = _updateService.GetUpdateFailures(maxResults).ToList();
            _logger.LogInformation("Found {Count} update failures", failures.Count);
            return failures;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to get update failures");
            throw new WindowsApiException("WindowsUpdate.GetFailures", ex);
        }
    }
}
