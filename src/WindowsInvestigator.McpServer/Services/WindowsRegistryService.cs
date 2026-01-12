using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace WindowsInvestigator.McpServer.Services;

/// <summary>
/// Windows Registry service implementation.
/// </summary>
public class WindowsRegistryService : IRegistryService
{
    private static readonly Dictionary<string, RegistryKey> RootKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        { "HKEY_LOCAL_MACHINE", Registry.LocalMachine },
        { "HKLM", Registry.LocalMachine },
        { "HKEY_CURRENT_USER", Registry.CurrentUser },
        { "HKCU", Registry.CurrentUser },
        { "HKEY_CLASSES_ROOT", Registry.ClassesRoot },
        { "HKCR", Registry.ClassesRoot },
        { "HKEY_USERS", Registry.Users },
        { "HKU", Registry.Users },
        { "HKEY_CURRENT_CONFIG", Registry.CurrentConfig },
        { "HKCC", Registry.CurrentConfig },
    };

    public RegistryKeyInfo? GetKey(string path)
    {
        var (rootKey, subPath) = ParsePath(path);
        if (rootKey == null)
            return null;

        try
        {
            using var key = rootKey.OpenSubKey(subPath);
            if (key == null)
                return null;

            return new RegistryKeyInfo
            {
                Path = path,
                SubKeyNames = key.GetSubKeyNames().OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList(),
                Values = GetValuesFromKey(key)
            };
        }
        catch (System.Security.SecurityException)
        {
            return null;
        }
    }

    public RegistryValueInfo? GetValue(string path, string valueName)
    {
        var (rootKey, subPath) = ParsePath(path);
        if (rootKey == null)
            return null;

        try
        {
            using var key = rootKey.OpenSubKey(subPath);
            if (key == null)
                return null;

            var value = key.GetValue(valueName);
            if (value == null && !key.GetValueNames().Contains(valueName, StringComparer.OrdinalIgnoreCase))
                return null;

            var kind = key.GetValueKind(valueName);
            return new RegistryValueInfo
            {
                Name = valueName,
                Type = kind.ToString(),
                Value = value,
                DisplayValue = FormatValue(value, kind)
            };
        }
        catch (System.Security.SecurityException)
        {
            return null;
        }
    }

    public IEnumerable<string> SearchKeys(string basePath, string pattern, int maxResults = 100)
    {
        var (rootKey, subPath) = ParsePath(basePath);
        if (rootKey == null)
            yield break;

        Regex regex;
        try
        {
            regex = new Regex(pattern, RegexOptions.IgnoreCase);
        }
        catch (ArgumentException)
        {
            yield break;
        }

        var results = new List<string>();
        SearchKeysRecursive(rootKey, subPath, basePath, regex, results, maxResults);
        
        foreach (var result in results)
            yield return result;
    }

    public IEnumerable<RegistryValueInfo> SearchValues(string basePath, string pattern, int maxResults = 100)
    {
        var (rootKey, subPath) = ParsePath(basePath);
        if (rootKey == null)
            yield break;

        Regex regex;
        try
        {
            regex = new Regex(pattern, RegexOptions.IgnoreCase);
        }
        catch (ArgumentException)
        {
            yield break;
        }

        var results = new List<RegistryValueInfo>();
        SearchValuesRecursive(rootKey, subPath, basePath, regex, results, maxResults);
        
        foreach (var result in results)
            yield return result;
    }

    private static (RegistryKey? Root, string SubPath) ParsePath(string path)
    {
        var parts = path.Split(new[] { '\\' }, 2);
        if (parts.Length == 0)
            return (null, "");

        var rootName = parts[0];
        var subPath = parts.Length > 1 ? parts[1] : "";

        if (RootKeys.TryGetValue(rootName, out var rootKey))
            return (rootKey, subPath);

        return (null, "");
    }

    private static List<RegistryValueInfo> GetValuesFromKey(RegistryKey key)
    {
        var values = new List<RegistryValueInfo>();
        foreach (var valueName in key.GetValueNames().OrderBy(n => n, StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                var value = key.GetValue(valueName);
                var kind = key.GetValueKind(valueName);
                values.Add(new RegistryValueInfo
                {
                    Name = valueName,
                    Type = kind.ToString(),
                    Value = value,
                    DisplayValue = FormatValue(value, kind)
                });
            }
            catch
            {
                // Skip values we can't read
            }
        }
        return values;
    }

    private static string? FormatValue(object? value, RegistryValueKind kind)
    {
        if (value == null)
            return null;

        return kind switch
        {
            RegistryValueKind.Binary when value is byte[] bytes => BitConverter.ToString(bytes).Replace("-", " "),
            RegistryValueKind.MultiString when value is string[] strings => string.Join("; ", strings),
            RegistryValueKind.DWord or RegistryValueKind.QWord => $"{value} (0x{value:X})",
            _ => value.ToString()
        };
    }

    private void SearchKeysRecursive(RegistryKey rootKey, string subPath, string fullPath, Regex regex, List<string> results, int maxResults)
    {
        if (results.Count >= maxResults)
            return;

        try
        {
            using var key = string.IsNullOrEmpty(subPath) ? rootKey : rootKey.OpenSubKey(subPath);
            if (key == null)
                return;

            foreach (var subKeyName in key.GetSubKeyNames())
            {
                if (results.Count >= maxResults)
                    return;

                var subKeyPath = string.IsNullOrEmpty(subPath) ? subKeyName : $"{subPath}\\{subKeyName}";
                var subKeyFullPath = $"{fullPath}\\{subKeyName}";

                if (regex.IsMatch(subKeyName))
                {
                    results.Add(subKeyFullPath);
                }

                // Recursively search subkeys (limit depth to prevent infinite loops)
                if (subKeyPath.Count(c => c == '\\') < 10)
                {
                    SearchKeysRecursive(rootKey, subKeyPath, subKeyFullPath, regex, results, maxResults);
                }
            }
        }
        catch
        {
            // Skip keys we can't access
        }
    }

    private void SearchValuesRecursive(RegistryKey rootKey, string subPath, string fullPath, Regex regex, List<RegistryValueInfo> results, int maxResults)
    {
        if (results.Count >= maxResults)
            return;

        try
        {
            using var key = string.IsNullOrEmpty(subPath) ? rootKey : rootKey.OpenSubKey(subPath);
            if (key == null)
                return;

            // Search values in current key
            foreach (var valueName in key.GetValueNames())
            {
                if (results.Count >= maxResults)
                    return;

                try
                {
                    var value = key.GetValue(valueName);
                    var kind = key.GetValueKind(valueName);
                    var displayValue = FormatValue(value, kind);

                    if (regex.IsMatch(valueName) || (displayValue != null && regex.IsMatch(displayValue)))
                    {
                        results.Add(new RegistryValueInfo
                        {
                            Name = $"{fullPath}\\{valueName}",
                            Type = kind.ToString(),
                            Value = value,
                            DisplayValue = displayValue
                        });
                    }
                }
                catch
                {
                    // Skip values we can't read
                }
            }

            // Recursively search subkeys
            foreach (var subKeyName in key.GetSubKeyNames())
            {
                if (results.Count >= maxResults)
                    return;

                var subKeyPath = string.IsNullOrEmpty(subPath) ? subKeyName : $"{subPath}\\{subKeyName}";
                var subKeyFullPath = $"{fullPath}\\{subKeyName}";

                // Limit depth to prevent infinite loops
                if (subKeyPath.Count(c => c == '\\') < 10)
                {
                    SearchValuesRecursive(rootKey, subKeyPath, subKeyFullPath, regex, results, maxResults);
                }
            }
        }
        catch
        {
            // Skip keys we can't access
        }
    }
}
