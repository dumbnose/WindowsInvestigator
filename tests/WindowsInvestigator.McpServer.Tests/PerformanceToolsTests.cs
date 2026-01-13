using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WindowsInvestigator.McpServer.Exceptions;
using WindowsInvestigator.McpServer.Services;
using WindowsInvestigator.McpServer.Tools;

namespace WindowsInvestigator.McpServer.Tests;

public class PerformanceToolsTests
{
    private readonly IPerformanceService _performanceService;
    private readonly PerformanceTools _sut;

    public PerformanceToolsTests()
    {
        _performanceService = Substitute.For<IPerformanceService>();
        _sut = new PerformanceTools(_performanceService, NullLogger<PerformanceTools>.Instance);
    }

    [Fact]
    public void GetPerformanceSnapshot_ReturnsSnapshotFromService()
    {
        // Arrange
        var expectedSnapshot = new PerformanceSnapshot
        {
            Timestamp = DateTime.Now,
            CpuUsagePercent = 25.5f,
            MemoryUsagePercent = 60.0f,
            ProcessCount = 150
        };
        _performanceService.GetPerformanceSnapshot().Returns(expectedSnapshot);

        // Act
        var result = _sut.GetPerformanceSnapshot();

        // Assert
        result.Should().BeEquivalentTo(expectedSnapshot);
        _performanceService.Received(1).GetPerformanceSnapshot();
    }

    [Fact]
    public void ListPerformanceCategories_ReturnsCategoriesFromService()
    {
        // Arrange
        var expectedCategories = new[]
        {
            new PerformanceCategory { Name = "Processor", Counters = new List<string> { "% Processor Time" } },
            new PerformanceCategory { Name = "Memory", Counters = new List<string> { "Available Bytes" } }
        };
        _performanceService.GetCategories().Returns(expectedCategories);

        // Act
        var result = _sut.ListPerformanceCategories();

        // Assert
        result.Should().BeEquivalentTo(expectedCategories);
        _performanceService.Received(1).GetCategories();
    }

    [Fact]
    public void GetPerformanceCounter_WithValidParams_ReturnsCounterValue()
    {
        // Arrange
        var expectedValue = new PerformanceCounterValue
        {
            CategoryName = "Processor",
            CounterName = "% Processor Time",
            InstanceName = "_Total",
            Value = 50.5f
        };
        _performanceService.GetCounterValue("Processor", "% Processor Time", "_Total").Returns(expectedValue);

        // Act
        var result = _sut.GetPerformanceCounter("Processor", "% Processor Time", "_Total");

        // Assert
        result.Should().BeEquivalentTo(expectedValue);
        _performanceService.Received(1).GetCounterValue("Processor", "% Processor Time", "_Total");
    }

    [Fact]
    public void GetPerformanceCounter_WithEmptyCategoryName_ThrowsInvalidParameterException()
    {
        // Act
        var act = () => _sut.GetPerformanceCounter("", "% Processor Time");

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*Category name cannot be empty*");
    }

    [Fact]
    public void GetPerformanceCounter_WithEmptyCounterName_ThrowsInvalidParameterException()
    {
        // Act
        var act = () => _sut.GetPerformanceCounter("Processor", "");

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*Counter name cannot be empty*");
    }

    [Fact]
    public void GetCategoryCounters_WithValidCategory_ReturnsCounters()
    {
        // Arrange
        var expectedCounters = new[]
        {
            new PerformanceCounterValue { CategoryName = "Memory", CounterName = "Available Bytes", Value = 1000000 },
            new PerformanceCounterValue { CategoryName = "Memory", CounterName = "Committed Bytes", Value = 5000000 }
        };
        _performanceService.GetCategoryCounters("Memory", null).Returns(expectedCounters);

        // Act
        var result = _sut.GetCategoryCounters("Memory");

        // Assert
        result.Should().BeEquivalentTo(expectedCounters);
        _performanceService.Received(1).GetCategoryCounters("Memory", null);
    }

    [Fact]
    public void GetCategoryCounters_WithEmptyCategoryName_ThrowsInvalidParameterException()
    {
        // Act
        var act = () => _sut.GetCategoryCounters("");

        // Assert
        act.Should().Throw<InvalidParameterException>()
            .WithMessage("*Category name cannot be empty*");
    }

    [Fact]
    public void GetPerformanceSnapshot_WhenServiceThrows_ThrowsWindowsApiException()
    {
        // Arrange
        _performanceService.GetPerformanceSnapshot().Returns(_ => throw new InvalidOperationException("Test error"));

        // Act
        var act = () => _sut.GetPerformanceSnapshot();

        // Assert
        act.Should().Throw<WindowsApiException>();
    }
}
