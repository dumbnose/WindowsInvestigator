using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WindowsInvestigator.McpServer.Exceptions;
using WindowsInvestigator.McpServer.Services;
using WindowsInvestigator.McpServer.Tools;

namespace WindowsInvestigator.McpServer.Tests;

public class ProcessToolsTests
{
    private readonly IProcessService _processService;
    private readonly ProcessTools _sut;

    public ProcessToolsTests()
    {
        _processService = Substitute.For<IProcessService>();
        _sut = new ProcessTools(_processService, NullLogger<ProcessTools>.Instance);
    }

    [Fact]
    public void ListProcesses_ReturnsProcessesFromService()
    {
        // Arrange
        var expectedProcesses = new[]
        {
            new ProcessInfo { ProcessId = 1, Name = "System" },
            new ProcessInfo { ProcessId = 4, Name = "Registry" },
            new ProcessInfo { ProcessId = 100, Name = "svchost" }
        };
        _processService.GetProcesses().Returns(expectedProcesses);

        // Act
        var result = _sut.ListProcesses();

        // Assert
        result.Should().BeEquivalentTo(expectedProcesses);
        _processService.Received(1).GetProcesses();
    }

    [Fact]
    public void ListProcesses_WhenEmpty_ReturnsEmptyCollection()
    {
        // Arrange
        _processService.GetProcesses().Returns(Enumerable.Empty<ProcessInfo>());

        // Act
        var result = _sut.ListProcesses();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetProcess_WithValidId_ReturnsProcess()
    {
        // Arrange
        var processId = 1234;
        var expectedProcess = new ProcessInfo
        {
            ProcessId = processId,
            Name = "notepad",
            WorkingSetBytes = 50 * 1024 * 1024,
            ThreadCount = 5,
            HandleCount = 100
        };
        _processService.GetProcess(processId).Returns(expectedProcess);

        // Act
        var result = _sut.GetProcess(processId);

        // Assert
        result.Should().BeEquivalentTo(expectedProcess);
        _processService.Received(1).GetProcess(processId);
    }

    [Fact]
    public void GetProcess_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var processId = 99999;
        _processService.GetProcess(processId).Returns((ProcessInfo?)null);

        // Act
        var result = _sut.GetProcess(processId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetProcess_WithInvalidId_ThrowsInvalidParameterException()
    {
        // Act
        Action act = () => _sut.GetProcess(0);

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*processId*");
    }

    [Fact]
    public void GetProcess_WithNegativeId_ThrowsInvalidParameterException()
    {
        // Act
        Action act = () => _sut.GetProcess(-1);

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*processId*");
    }

    [Fact]
    public void SearchProcesses_WithPattern_ReturnsMatchingProcesses()
    {
        // Arrange
        var pattern = "svc.*";
        var expectedProcesses = new[]
        {
            new ProcessInfo { ProcessId = 100, Name = "svchost" },
            new ProcessInfo { ProcessId = 200, Name = "svcscan" }
        };
        _processService.SearchProcesses(pattern).Returns(expectedProcesses);

        // Act
        var result = _sut.SearchProcesses(pattern);

        // Assert
        result.Should().BeEquivalentTo(expectedProcesses);
        _processService.Received(1).SearchProcesses(pattern);
    }

    [Fact]
    public void SearchProcesses_WithEmptyPattern_ThrowsInvalidParameterException()
    {
        // Act
        Action act = () => _sut.SearchProcesses("");

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*pattern*");
    }

    [Fact]
    public void SearchProcesses_WithInvalidRegex_ThrowsInvalidParameterException()
    {
        // Act
        Action act = () => _sut.SearchProcesses("[invalid");

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*pattern*Invalid regex*");
    }

    [Fact]
    public void GetProcessSummary_ReturnsSummaryFromService()
    {
        // Arrange
        var expectedSummary = new ProcessSummary
        {
            TotalProcesses = 150,
            TotalThreads = 2000,
            TotalWorkingSetBytes = 8L * 1024 * 1024 * 1024, // 8 GB
            TopCpuProcesses = new List<ProcessInfo>
            {
                new() { ProcessId = 1, Name = "chrome", CpuPercent = 25 }
            },
            TopMemoryProcesses = new List<ProcessInfo>
            {
                new() { ProcessId = 2, Name = "devenv", WorkingSetBytes = 2L * 1024 * 1024 * 1024 }
            }
        };
        _processService.GetProcessSummary().Returns(expectedSummary);

        // Act
        var result = _sut.GetProcessSummary();

        // Assert
        result.Should().BeEquivalentTo(expectedSummary);
        _processService.Received(1).GetProcessSummary();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(12345)]
    public void GetProcess_WithVariousIds_PassesCorrectId(int processId)
    {
        // Arrange
        _processService.GetProcess(processId).Returns(new ProcessInfo { ProcessId = processId });

        // Act
        var result = _sut.GetProcess(processId);

        // Assert
        result!.ProcessId.Should().Be(processId);
    }

    [Fact]
    public void ListProcesses_WhenServiceThrows_ThrowsWindowsApiException()
    {
        // Arrange
        _processService.GetProcesses().Returns(_ => throw new InvalidOperationException("Test error"));

        // Act
        Action act = () => _sut.ListProcesses();

        // Assert
        act.Should().Throw<WindowsApiException>();
    }

    [Fact]
    public void GetProcessSummary_WhenServiceThrows_ThrowsWindowsApiException()
    {
        // Arrange
        _processService.GetProcessSummary().Returns(_ => throw new InvalidOperationException("Test error"));

        // Act
        Action act = () => _sut.GetProcessSummary();

        // Assert
        act.Should().Throw<WindowsApiException>();
    }

    [Fact]
    public void SearchProcesses_WhenServiceThrows_ThrowsWindowsApiException()
    {
        // Arrange
        _processService.SearchProcesses(Arg.Any<string>()).Returns(_ => throw new InvalidOperationException("Test error"));

        // Act
        Action act = () => _sut.SearchProcesses("test");

        // Assert
        act.Should().Throw<WindowsApiException>();
    }
}
