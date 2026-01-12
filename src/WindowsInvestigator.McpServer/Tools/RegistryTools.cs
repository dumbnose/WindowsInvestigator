using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.RegularExpressions;
using WindowsInvestigator.McpServer.Exceptions;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.Tools;

[McpServerToolType]
public sealed class RegistryTools
{
    private readonly IRegistryService _registryService;
    private readonly ILogger<RegistryTools> _logger;

    public RegistryTools(IRegistryService registryService, ILogger<RegistryTools> logger)
    {
        _registryService = registryService;
        _logger = logger;
    }

    [McpServerTool, Description("Gets information about a registry key including subkeys and values")]
    public RegistryKeyInfo? GetRegistryKey(
        [Description("Registry path (e.g., HKLM\\SOFTWARE\\Microsoft, HKCU\\Software)")] string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidParameterException("path", "Registry path cannot be empty");
        }

        try
        {
            _logger.LogDebug("Getting registry key {Path}", path);
            var key = _registryService.GetKey(path);
            
            if (key == null)
            {
                _logger.LogWarning("Registry key {Path} not found", path);
            }
            else
            {
                _logger.LogInformation("Retrieved registry key {Path} with {SubKeyCount} subkeys and {ValueCount} values",
                    path, key.SubKeyNames.Count, key.Values.Count);
            }
            
            return key;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to get registry key {Path}", path);
            throw new WindowsApiException("Registry.OpenSubKey", ex);
        }
    }

    [McpServerTool, Description("Gets a specific value from a registry key")]
    public RegistryValueInfo? GetRegistryValue(
        [Description("Registry path (e.g., HKLM\\SOFTWARE\\Microsoft)")] string path,
        [Description("Name of the value (empty string for default value)")] string valueName)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidParameterException("path", "Registry path cannot be empty");
        }

        try
        {
            _logger.LogDebug("Getting registry value {ValueName} from {Path}", valueName, path);
            var value = _registryService.GetValue(path, valueName);
            
            if (value == null)
            {
                _logger.LogWarning("Registry value {ValueName} not found at {Path}", valueName, path);
            }
            else
            {
                _logger.LogInformation("Retrieved registry value {ValueName} from {Path} (Type: {Type})",
                    valueName, path, value.Type);
            }
            
            return value;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to get registry value {ValueName} from {Path}", valueName, path);
            throw new WindowsApiException("Registry.GetValue", ex);
        }
    }

    [McpServerTool, Description("Searches for registry keys matching a pattern")]
    public IEnumerable<string> SearchRegistryKeys(
        [Description("Base registry path to search from (e.g., HKLM\\SOFTWARE)")] string basePath,
        [Description("Regex pattern to match key names")] string pattern,
        [Description("Maximum number of results (default: 100)")] int maxResults = 100)
    {
        if (string.IsNullOrWhiteSpace(basePath))
        {
            throw new InvalidParameterException("basePath", "Base path cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new InvalidParameterException("pattern", "Search pattern cannot be empty");
        }

        if (maxResults <= 0)
        {
            throw new InvalidParameterException("maxResults", "Must be greater than 0");
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
            _logger.LogDebug("Searching registry keys in {BasePath} for pattern '{Pattern}'", basePath, pattern);
            var keys = _registryService.SearchKeys(basePath, pattern, maxResults).ToList();
            _logger.LogInformation("Found {Count} registry keys matching pattern '{Pattern}' in {BasePath}",
                keys.Count, pattern, basePath);
            return keys;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to search registry keys in {BasePath}", basePath);
            throw new WindowsApiException("Registry.SearchKeys", ex);
        }
    }

    [McpServerTool, Description("Searches for registry values matching a pattern")]
    public IEnumerable<RegistryValueInfo> SearchRegistryValues(
        [Description("Base registry path to search from (e.g., HKLM\\SOFTWARE)")] string basePath,
        [Description("Regex pattern to match value names or data")] string pattern,
        [Description("Maximum number of results (default: 100)")] int maxResults = 100)
    {
        if (string.IsNullOrWhiteSpace(basePath))
        {
            throw new InvalidParameterException("basePath", "Base path cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new InvalidParameterException("pattern", "Search pattern cannot be empty");
        }

        if (maxResults <= 0)
        {
            throw new InvalidParameterException("maxResults", "Must be greater than 0");
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
            _logger.LogDebug("Searching registry values in {BasePath} for pattern '{Pattern}'", basePath, pattern);
            var values = _registryService.SearchValues(basePath, pattern, maxResults).ToList();
            _logger.LogInformation("Found {Count} registry values matching pattern '{Pattern}' in {BasePath}",
                values.Count, pattern, basePath);
            return values;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to search registry values in {BasePath}", basePath);
            throw new WindowsApiException("Registry.SearchValues", ex);
        }
    }
}
