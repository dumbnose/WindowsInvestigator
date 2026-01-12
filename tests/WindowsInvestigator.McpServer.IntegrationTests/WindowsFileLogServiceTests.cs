using FluentAssertions;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.IntegrationTests;

/// <summary>
/// Integration tests that run against real Windows file system.
/// </summary>
public class WindowsFileLogServiceTests : IDisposable
{
    private readonly WindowsFileLogService _sut;
    private readonly string _testDir;
    private readonly string _testLogFile;

    public WindowsFileLogServiceTests()
    {
        _sut = new WindowsFileLogService();
        
        // Create a temp directory and test log file for testing
        _testDir = Path.Combine(Path.GetTempPath(), $"WindowsInvestigatorTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDir);
        
        _testLogFile = Path.Combine(_testDir, "test.log");
        File.WriteAllLines(_testLogFile, new[]
        {
            "2026-01-12 10:00:00 INFO Application started",
            "2026-01-12 10:00:01 DEBUG Loading configuration",
            "2026-01-12 10:00:02 INFO Configuration loaded",
            "2026-01-12 10:00:03 WARN Low memory warning",
            "2026-01-12 10:00:04 ERROR Failed to connect to database",
            "2026-01-12 10:00:05 INFO Retrying connection",
            "2026-01-12 10:00:06 INFO Connected to database",
            "2026-01-12 10:00:07 DEBUG Query executed",
            "2026-01-12 10:00:08 ERROR Timeout waiting for response",
            "2026-01-12 10:00:09 INFO Application shutdown"
        });
    }

    public void Dispose()
    {
        // Clean up test directory
        try
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public void DiscoverLogs_ReturnsLogFiles()
    {
        // Act
        var result = _sut.DiscoverLogs(includeSystemLogs: true, maxFiles: 50).ToList();

        // Assert
        result.Should().NotBeNull();
        // May be empty if no log files exist in common locations
    }

    [Fact]
    public void DiscoverLogs_ReturnsOrderedByLastModified()
    {
        // Act
        var result = _sut.DiscoverLogs(includeSystemLogs: true, maxFiles: 50).ToList();

        // Assert
        if (result.Count > 1)
        {
            for (int i = 0; i < result.Count - 1; i++)
            {
                result[i].LastModified.Should().BeOnOrAfter(result[i + 1].LastModified);
            }
        }
    }

    [Fact]
    public void DiscoverLogs_RespectsMaxFiles()
    {
        // Act
        var result = _sut.DiscoverLogs(includeSystemLogs: true, maxFiles: 5).ToList();

        // Assert
        result.Count.Should().BeLessThanOrEqualTo(5);
    }

    [Fact]
    public void DiscoverLogs_LogFilesHaveRequiredProperties()
    {
        // Act
        var result = _sut.DiscoverLogs(includeSystemLogs: true, maxFiles: 20).ToList();

        // Assert
        result.Should().AllSatisfy(log =>
        {
            log.Path.Should().NotBeNullOrEmpty();
            log.Name.Should().NotBeNullOrEmpty();
            log.SizeBytes.Should().BeGreaterThanOrEqualTo(0);
        });
    }

    [Fact]
    public void ReadLog_ReadsEntireFile()
    {
        // Act
        var result = _sut.ReadLog(_testLogFile);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.TotalLines.Should().Be(10);
        result.ReturnedLines.Should().Be(10);
        result.Lines.Should().HaveCount(10);
    }

    [Fact]
    public void ReadLog_WithTailLines_ReturnsLastNLines()
    {
        // Act
        var result = _sut.ReadLog(_testLogFile, tailLines: 3);

        // Assert
        result.Error.Should().BeNull();
        result.TotalLines.Should().Be(10);
        result.ReturnedLines.Should().Be(3);
        result.Lines.Should().HaveCount(3);
        result.Lines[0].Should().Contain("Query executed");
        result.Lines[2].Should().Contain("Application shutdown");
    }

    [Fact]
    public void ReadLog_WithSearchPattern_FiltersLines()
    {
        // Act
        var result = _sut.ReadLog(_testLogFile, searchPattern: "ERROR");

        // Assert
        result.Error.Should().BeNull();
        result.ReturnedLines.Should().Be(2);
        result.Lines.Should().AllSatisfy(line => line.Should().Contain("ERROR"));
    }

    [Fact]
    public void ReadLog_WithRegexSearchPattern_FiltersLines()
    {
        // Act
        var result = _sut.ReadLog(_testLogFile, searchPattern: "ERROR|WARN");

        // Assert
        result.Error.Should().BeNull();
        result.ReturnedLines.Should().Be(3);
        result.Lines.Should().AllSatisfy(line => 
            (line.Contains("ERROR") || line.Contains("WARN")).Should().BeTrue());
    }

    [Fact]
    public void ReadLog_WithMaxLines_LimitsOutput()
    {
        // Act
        var result = _sut.ReadLog(_testLogFile, maxLines: 5);

        // Assert
        result.Error.Should().BeNull();
        result.TotalLines.Should().Be(10);
        result.ReturnedLines.Should().Be(5);
        result.Lines.Should().HaveCount(5);
    }

    [Fact]
    public void ReadLog_WithTailAndSearch_CombinesFilters()
    {
        // Act - get last 5 lines, then filter for INFO
        var result = _sut.ReadLog(_testLogFile, tailLines: 5, searchPattern: "INFO");

        // Assert
        result.Error.Should().BeNull();
        result.Lines.Should().AllSatisfy(line => line.Should().Contain("INFO"));
        result.ReturnedLines.Should().BeLessThanOrEqualTo(5);
    }

    [Fact]
    public void ReadLog_WithNonExistentFile_ReturnsError()
    {
        // Act
        var result = _sut.ReadLog(@"C:\NonExistent\path\file.log");

        // Assert
        result.Error.Should().Be("File not found");
        result.Lines.Should().BeEmpty();
    }

    [Fact]
    public void ReadLog_WithInvalidRegex_ReturnsError()
    {
        // Act
        var result = _sut.ReadLog(_testLogFile, searchPattern: "[invalid(regex");

        // Assert
        result.Error.Should().Be("Invalid regex pattern");
    }

    [Fact]
    public void ReadLog_PreservesLineOrder()
    {
        // Act
        var result = _sut.ReadLog(_testLogFile);

        // Assert
        result.Lines[0].Should().Contain("Application started");
        result.Lines[9].Should().Contain("Application shutdown");
    }

    [Fact]
    public void ReadLog_CaseInsensitiveSearch()
    {
        // Act
        var result = _sut.ReadLog(_testLogFile, searchPattern: "error");

        // Assert
        result.Error.Should().BeNull();
        result.ReturnedLines.Should().Be(2);
    }
}
