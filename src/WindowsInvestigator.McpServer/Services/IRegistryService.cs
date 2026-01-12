namespace WindowsInvestigator.McpServer.Services;

/// <summary>
/// Registry key information.
/// </summary>
public class RegistryKeyInfo
{
    public string Path { get; set; } = "";
    public List<string> SubKeyNames { get; set; } = new();
    public List<RegistryValueInfo> Values { get; set; } = new();
}

/// <summary>
/// Registry value information.
/// </summary>
public class RegistryValueInfo
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public object? Value { get; set; }
    public string? DisplayValue { get; set; }
}

/// <summary>
/// Abstraction for Windows Registry access.
/// </summary>
public interface IRegistryService
{
    /// <summary>
    /// Gets information about a registry key including its subkeys and values.
    /// </summary>
    /// <param name="path">Registry path (e.g., HKLM\SOFTWARE\Microsoft)</param>
    RegistryKeyInfo? GetKey(string path);

    /// <summary>
    /// Gets a specific value from a registry key.
    /// </summary>
    /// <param name="path">Registry path</param>
    /// <param name="valueName">Name of the value (empty string for default value)</param>
    RegistryValueInfo? GetValue(string path, string valueName);

    /// <summary>
    /// Searches for registry keys matching a pattern.
    /// </summary>
    /// <param name="basePath">Base registry path to search from</param>
    /// <param name="pattern">Regex pattern to match key names</param>
    /// <param name="maxResults">Maximum number of results</param>
    IEnumerable<string> SearchKeys(string basePath, string pattern, int maxResults = 100);

    /// <summary>
    /// Searches for registry values matching a pattern.
    /// </summary>
    /// <param name="basePath">Base registry path to search from</param>
    /// <param name="pattern">Regex pattern to match value names or data</param>
    /// <param name="maxResults">Maximum number of results</param>
    IEnumerable<RegistryValueInfo> SearchValues(string basePath, string pattern, int maxResults = 100);
}
