using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using WindowsInvestigator.McpServer.Exceptions;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.Tools;

[McpServerToolType]
public sealed class ReliabilityTools
{
    private readonly IReliabilityService _reliabilityService;
    private readonly ILogger<ReliabilityTools> _logger;

    public ReliabilityTools(IReliabilityService reliabilityService, ILogger<ReliabilityTools> logger)
    {
        _reliabilityService = reliabilityService;
        _logger = logger;
    }

    [McpServerTool, Description("Gets reliability events (crashes, hangs, failures) from Reliability Monitor")]
    public IEnumerable<ReliabilityEvent> GetReliabilityEvents(
        [Description("Start time for the query (ISO 8601 format, e.g., '2024-01-01T00:00:00'). Defaults to 30 days ago.")] DateTime? startTime = null,
        [Description("End time for the query (ISO 8601 format). Defaults to now.")] DateTime? endTime = null,
        [Description("Maximum number of events to return (default: 50)")] int maxResults = 50)
    {
        if (maxResults <= 0)
        {
            throw new InvalidParameterException("maxResults", "Maximum results must be greater than 0");
        }

        try
        {
            _logger.LogDebug("Getting reliability events from {Start} to {End}", startTime, endTime);
            var events = _reliabilityService.GetReliabilityEvents(startTime, endTime, maxResults).ToList();
            _logger.LogInformation("Found {Count} reliability events", events.Count);
            return events;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to get reliability events");
            throw new WindowsApiException("ReliabilityMonitor.GetEvents", ex);
        }
    }

    [McpServerTool, Description("Gets recent application crashes from Reliability Monitor")]
    public IEnumerable<ReliabilityEvent> GetApplicationCrashes(
        [Description("Maximum number of crashes to return (default: 20)")] int maxResults = 20)
    {
        if (maxResults <= 0)
        {
            throw new InvalidParameterException("maxResults", "Maximum results must be greater than 0");
        }

        try
        {
            _logger.LogDebug("Getting application crashes (max {MaxResults})", maxResults);
            var crashes = _reliabilityService.GetApplicationCrashes(maxResults).ToList();
            _logger.LogInformation("Found {Count} application crashes", crashes.Count);
            return crashes;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to get application crashes");
            throw new WindowsApiException("ReliabilityMonitor.GetCrashes", ex);
        }
    }

    [McpServerTool, Description("Gets recent application hangs from Reliability Monitor")]
    public IEnumerable<ReliabilityEvent> GetApplicationHangs(
        [Description("Maximum number of hangs to return (default: 20)")] int maxResults = 20)
    {
        if (maxResults <= 0)
        {
            throw new InvalidParameterException("maxResults", "Maximum results must be greater than 0");
        }

        try
        {
            _logger.LogDebug("Getting application hangs (max {MaxResults})", maxResults);
            var hangs = _reliabilityService.GetApplicationHangs(maxResults).ToList();
            _logger.LogInformation("Found {Count} application hangs", hangs.Count);
            return hangs;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to get application hangs");
            throw new WindowsApiException("ReliabilityMonitor.GetHangs", ex);
        }
    }

    [McpServerTool, Description("Gets system failures (BSODs, unexpected shutdowns) from Reliability Monitor")]
    public IEnumerable<ReliabilityEvent> GetSystemFailures(
        [Description("Maximum number of failures to return (default: 20)")] int maxResults = 20)
    {
        if (maxResults <= 0)
        {
            throw new InvalidParameterException("maxResults", "Maximum results must be greater than 0");
        }

        try
        {
            _logger.LogDebug("Getting system failures (max {MaxResults})", maxResults);
            var failures = _reliabilityService.GetSystemFailures(maxResults).ToList();
            _logger.LogInformation("Found {Count} system failures", failures.Count);
            return failures;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to get system failures");
            throw new WindowsApiException("ReliabilityMonitor.GetSystemFailures", ex);
        }
    }

    [McpServerTool, Description("Gets daily reliability scores showing system stability over time")]
    public IEnumerable<ReliabilityScore> GetReliabilityScores(
        [Description("Number of days of history to retrieve (default: 30)")] int days = 30)
    {
        if (days <= 0)
        {
            throw new InvalidParameterException("days", "Days must be greater than 0");
        }

        try
        {
            _logger.LogDebug("Getting reliability scores for {Days} days", days);
            var scores = _reliabilityService.GetReliabilityScores(days).ToList();
            _logger.LogInformation("Retrieved {Count} daily reliability scores", scores.Count);
            return scores;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to get reliability scores");
            throw new WindowsApiException("ReliabilityMonitor.GetScores", ex);
        }
    }
}
