using System.Text.RegularExpressions;

namespace WindowsInvestigator.McpServer.Services;

/// <summary>
/// Windows file log service implementation.
/// </summary>
public class WindowsFileLogService : IFileLogService
{
    private static readonly string[] LogExtensions = { ".log", ".txt", ".evtx", ".etl" };
    
    private static readonly (string Path, string Category)[] CommonLogLocations =
    {
        (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Temp", "User Temp"),
        (Path.GetTempPath(), "System Temp"),
        (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Microsoft\Windows\INetCache", "IE Cache"),
        (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\CrashDumps", "Crash Dumps"),
        (@"C:\Windows\Logs", "Windows Logs"),
        (@"C:\Windows\Panther", "Windows Setup"),
        (@"C:\Windows\SoftwareDistribution\ReportingEvents.log", "Windows Update"),
        (@"C:\Windows\debug", "Windows Debug"),
        (@"C:\Windows\System32\LogFiles", "System LogFiles"),
        (@"C:\ProgramData\Microsoft\Windows\WER", "Windows Error Reporting"),
        (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Microsoft\CLR_v4.0", "CLR Logs"),
    };

    public IEnumerable<LogFileInfo> DiscoverLogs(bool includeSystemLogs = true, int maxFiles = 100)
    {
        var logs = new List<LogFileInfo>();

        foreach (var (path, category) in CommonLogLocations)
        {
            if (!includeSystemLogs && path.StartsWith(@"C:\Windows", StringComparison.OrdinalIgnoreCase))
                continue;

            try
            {
                if (File.Exists(path))
                {
                    // It's a file path
                    var fileInfo = new FileInfo(path);
                    logs.Add(new LogFileInfo
                    {
                        Path = path,
                        Name = fileInfo.Name,
                        SizeBytes = fileInfo.Length,
                        LastModified = fileInfo.LastWriteTime,
                        Category = category
                    });
                }
                else if (Directory.Exists(path))
                {
                    // It's a directory - scan for log files
                    var files = Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                        .Where(f => LogExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                        .Take(maxFiles - logs.Count);

                    foreach (var file in files)
                    {
                        try
                        {
                            var fileInfo = new FileInfo(file);
                            logs.Add(new LogFileInfo
                            {
                                Path = file,
                                Name = fileInfo.Name,
                                SizeBytes = fileInfo.Length,
                                LastModified = fileInfo.LastWriteTime,
                                Category = category
                            });

                            if (logs.Count >= maxFiles)
                                break;
                        }
                        catch
                        {
                            // Skip files we can't access
                        }
                    }
                }
            }
            catch
            {
                // Skip directories we can't access
            }

            if (logs.Count >= maxFiles)
                break;
        }

        return logs.OrderByDescending(l => l.LastModified).ToList();
    }

    public LogFileContent ReadLog(string path, int? tailLines = null, string? searchPattern = null, int maxLines = 500)
    {
        var result = new LogFileContent { Path = path };

        try
        {
            if (!File.Exists(path))
            {
                result.Error = "File not found";
                return result;
            }

            var allLines = File.ReadAllLines(path);
            result.TotalLines = allLines.Length;

            IEnumerable<string> lines = allLines;

            // Apply tail if specified
            if (tailLines.HasValue && tailLines.Value < allLines.Length)
            {
                lines = allLines.Skip(allLines.Length - tailLines.Value);
            }

            // Apply search pattern if specified
            if (!string.IsNullOrEmpty(searchPattern))
            {
                try
                {
                    var regex = new Regex(searchPattern, RegexOptions.IgnoreCase);
                    lines = lines.Where(line => regex.IsMatch(line));
                }
                catch (ArgumentException)
                {
                    result.Error = "Invalid regex pattern";
                    return result;
                }
            }

            // Apply max lines limit
            result.Lines = lines.Take(maxLines).ToList();
            result.ReturnedLines = result.Lines.Count;
        }
        catch (UnauthorizedAccessException)
        {
            result.Error = "Access denied";
        }
        catch (IOException ex)
        {
            result.Error = $"IO error: {ex.Message}";
        }
        catch (Exception ex)
        {
            result.Error = $"Error: {ex.Message}";
        }

        return result;
    }
}
