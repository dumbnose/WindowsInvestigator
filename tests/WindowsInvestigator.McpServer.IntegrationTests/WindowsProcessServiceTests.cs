using FluentAssertions;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.IntegrationTests;

public class WindowsProcessServiceTests
{
    private readonly WindowsProcessService _sut;

    public WindowsProcessServiceTests()
    {
        _sut = new WindowsProcessService();
    }

    [Fact]
    public void GetProcesses_ReturnsRunningProcesses()
    {
        // Act
        var result = _sut.GetProcesses().ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCountGreaterThan(10); // There should be many processes on any Windows system
    }

    [Fact]
    public void GetProcesses_ReturnsOrderedByName()
    {
        // Act
        var result = _sut.GetProcesses().ToList();

        // Assert
        result.Select(p => p.Name).Should().BeInAscendingOrder(StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetProcesses_IncludesSystemProcess()
    {
        // Act
        var result = _sut.GetProcesses().ToList();

        // Assert
        result.Should().Contain(p => p.Name.Equals("System", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetProcesses_ProcessesHaveValidData()
    {
        // Act
        var result = _sut.GetProcesses().ToList();

        // Assert
        result.Should().AllSatisfy(p =>
        {
            p.ProcessId.Should().BeGreaterThanOrEqualTo(0); // PID 0 is System Idle Process
            p.Name.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public void GetProcesses_ProcessesHaveMemoryInfo()
    {
        // Act
        var result = _sut.GetProcesses().ToList();

        // Assert
        // At least some processes should have working set info
        result.Should().Contain(p => p.WorkingSetBytes > 0);
    }

    [Fact]
    public void GetProcesses_ProcessesHaveThreadCount()
    {
        // Act
        var result = _sut.GetProcesses().ToList();

        // Assert
        // Most processes should have at least one thread
        result.Should().Contain(p => p.ThreadCount > 0);
    }

    [Fact]
    public void GetProcess_WithCurrentProcessId_ReturnsProcess()
    {
        // Arrange
        var currentPid = Environment.ProcessId;

        // Act
        var result = _sut.GetProcess(currentPid);

        // Assert
        result.Should().NotBeNull();
        result!.ProcessId.Should().Be(currentPid);
        result.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetProcess_WithNonExistentId_ReturnsNull()
    {
        // Arrange - use an unlikely process ID
        var nonExistentPid = 999999;

        // Act
        var result = _sut.GetProcess(nonExistentPid);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetProcess_ReturnsDetailedInfo()
    {
        // Arrange
        var currentPid = Environment.ProcessId;

        // Act
        var result = _sut.GetProcess(currentPid);

        // Assert
        result.Should().NotBeNull();
        result!.WorkingSetBytes.Should().BeGreaterThan(0);
        result.ThreadCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void SearchProcesses_WithSystemPattern_FindsSystemProcess()
    {
        // Arrange
        var pattern = "^System$";

        // Act
        var result = _sut.SearchProcesses(pattern).ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(p => p.Name.Equals("System", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SearchProcesses_WithCommonPattern_FindsProcesses()
    {
        // Arrange - search for svchost which should exist on any Windows system
        var pattern = "svchost";

        // Act
        var result = _sut.SearchProcesses(pattern).ToList();

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void SearchProcesses_WithNoMatches_ReturnsEmpty()
    {
        // Arrange
        var pattern = "NonExistentProcessName12345XYZ";

        // Act
        var result = _sut.SearchProcesses(pattern).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SearchProcesses_WithInvalidRegex_ReturnsEmpty()
    {
        // Arrange
        var pattern = "[invalid";

        // Act
        var result = _sut.SearchProcesses(pattern).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SearchProcesses_ReturnsOrderedResults()
    {
        // Arrange
        var pattern = ".*";  // Match all

        // Act
        var result = _sut.SearchProcesses(pattern).Take(50).ToList();

        // Assert
        result.Select(p => p.Name).Should().BeInAscendingOrder(StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetProcessSummary_ReturnsSummary()
    {
        // Act
        var result = _sut.GetProcessSummary();

        // Assert
        result.Should().NotBeNull();
        result.TotalProcesses.Should().BeGreaterThan(0);
        result.TotalThreads.Should().BeGreaterThan(0);
        result.TotalWorkingSetBytes.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetProcessSummary_HasTopCpuProcesses()
    {
        // Act
        var result = _sut.GetProcessSummary();

        // Assert
        result.TopCpuProcesses.Should().NotBeEmpty();
        result.TopCpuProcesses.Should().HaveCountLessThanOrEqualTo(10);
    }

    [Fact]
    public void GetProcessSummary_HasTopMemoryProcesses()
    {
        // Act
        var result = _sut.GetProcessSummary();

        // Assert
        result.TopMemoryProcesses.Should().NotBeEmpty();
        result.TopMemoryProcesses.Should().HaveCountLessThanOrEqualTo(10);
    }

    [Fact]
    public void GetProcessSummary_TopMemoryProcessesAreSorted()
    {
        // Act
        var result = _sut.GetProcessSummary();

        // Assert
        result.TopMemoryProcesses.Select(p => p.WorkingSetBytes)
            .Should().BeInDescendingOrder();
    }

    [Fact]
    public void GetProcess_ForSystemProcess_ReturnsInfo()
    {
        // Arrange - System process always has PID 4
        var systemPid = 4;

        // Act
        var result = _sut.GetProcess(systemPid);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("System");
    }

    [Theory]
    [InlineData("dotnet")]
    [InlineData("svchost")]
    [InlineData("explorer")]
    public void SearchProcesses_WithCommonProcessNames_MayFindProcesses(string processName)
    {
        // Act
        var result = _sut.SearchProcesses(processName).ToList();

        // Assert - these may or may not be running, just verify no error
        result.Should().NotBeNull();
    }

    [Fact]
    public void GetProcesses_DoesNotContainDuplicates()
    {
        // Act
        var result = _sut.GetProcesses().ToList();

        // Assert - each PID should be unique
        result.Select(p => p.ProcessId).Should().OnlyHaveUniqueItems();
    }
}
