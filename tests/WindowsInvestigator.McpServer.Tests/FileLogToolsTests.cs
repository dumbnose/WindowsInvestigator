using FluentAssertions;
using NSubstitute;
using WindowsInvestigator.McpServer.Services;
using WindowsInvestigator.McpServer.Tools;

namespace WindowsInvestigator.McpServer.Tests;

public class FileLogToolsTests
{
    private readonly IFileLogService _fileLogService;
    private readonly FileLogTools _sut;

    public FileLogToolsTests()
    {
        _fileLogService = Substitute.For<IFileLogService>();
        _sut = new FileLogTools(_fileLogService);
    }

    [Fact]
    public void DiscoverLogs_ReturnsLogsFromService()
    {
        // Arrange
        var expectedLogs = new[]
        {
            new LogFileInfo { Path = @"C:\Logs\app.log", Name = "app.log", SizeBytes = 1024, Category = "Application" },
            new LogFileInfo { Path = @"C:\Logs\error.log", Name = "error.log", SizeBytes = 2048, Category = "Application" }
        };
        _fileLogService.DiscoverLogs(true, 100).Returns(expectedLogs);

        // Act
        var result = _sut.DiscoverLogs();

        // Assert
        result.Should().BeEquivalentTo(expectedLogs);
        _fileLogService.Received(1).DiscoverLogs(true, 100);
    }

    [Fact]
    public void DiscoverLogs_WithCustomParameters_PassesToService()
    {
        // Arrange
        _fileLogService.DiscoverLogs(false, 50).Returns(Enumerable.Empty<LogFileInfo>());

        // Act
        var result = _sut.DiscoverLogs(includeSystemLogs: false, maxFiles: 50);

        // Assert
        _fileLogService.Received(1).DiscoverLogs(false, 50);
    }

    [Fact]
    public void DiscoverLogs_WhenEmpty_ReturnsEmptyCollection()
    {
        // Arrange
        _fileLogService.DiscoverLogs(true, 100).Returns(Enumerable.Empty<LogFileInfo>());

        // Act
        var result = _sut.DiscoverLogs();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ReadLog_ReturnsContentFromService()
    {
        // Arrange
        var path = @"C:\Logs\app.log";
        var expectedContent = new LogFileContent
        {
            Path = path,
            TotalLines = 100,
            ReturnedLines = 50,
            Lines = Enumerable.Range(1, 50).Select(i => $"Line {i}").ToList()
        };
        _fileLogService.ReadLog(path, null, null, 500).Returns(expectedContent);

        // Act
        var result = _sut.ReadLog(path);

        // Assert
        result.Should().BeEquivalentTo(expectedContent);
        _fileLogService.Received(1).ReadLog(path, null, null, 500);
    }

    [Fact]
    public void ReadLog_WithTailLines_PassesToService()
    {
        // Arrange
        var path = @"C:\Logs\app.log";
        _fileLogService.ReadLog(path, 100, null, 500).Returns(new LogFileContent { Path = path });

        // Act
        var result = _sut.ReadLog(path, tailLines: 100);

        // Assert
        _fileLogService.Received(1).ReadLog(path, 100, null, 500);
    }

    [Fact]
    public void ReadLog_WithSearchPattern_PassesToService()
    {
        // Arrange
        var path = @"C:\Logs\app.log";
        var pattern = "ERROR|WARN";
        _fileLogService.ReadLog(path, null, pattern, 500).Returns(new LogFileContent { Path = path });

        // Act
        var result = _sut.ReadLog(path, searchPattern: pattern);

        // Assert
        _fileLogService.Received(1).ReadLog(path, null, pattern, 500);
    }

    [Fact]
    public void ReadLog_WithMaxLines_PassesToService()
    {
        // Arrange
        var path = @"C:\Logs\app.log";
        _fileLogService.ReadLog(path, null, null, 200).Returns(new LogFileContent { Path = path });

        // Act
        var result = _sut.ReadLog(path, maxLines: 200);

        // Assert
        _fileLogService.Received(1).ReadLog(path, null, null, 200);
    }

    [Fact]
    public void ReadLog_WithAllParameters_PassesToService()
    {
        // Arrange
        var path = @"C:\Logs\app.log";
        var tailLines = 50;
        var pattern = "Exception";
        var maxLines = 100;
        _fileLogService.ReadLog(path, tailLines, pattern, maxLines).Returns(new LogFileContent { Path = path });

        // Act
        var result = _sut.ReadLog(path, tailLines, pattern, maxLines);

        // Assert
        _fileLogService.Received(1).ReadLog(path, tailLines, pattern, maxLines);
    }

    [Fact]
    public void ReadLog_WhenServiceReturnsError_ReturnsError()
    {
        // Arrange
        var path = @"C:\Logs\missing.log";
        var expectedContent = new LogFileContent
        {
            Path = path,
            Error = "File not found"
        };
        _fileLogService.ReadLog(path, null, null, 500).Returns(expectedContent);

        // Act
        var result = _sut.ReadLog(path);

        // Assert
        result.Error.Should().Be("File not found");
    }
}
