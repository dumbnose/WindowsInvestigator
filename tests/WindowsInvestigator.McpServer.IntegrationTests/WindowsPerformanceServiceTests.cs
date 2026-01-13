using FluentAssertions;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.IntegrationTests;

public class WindowsPerformanceServiceTests
{
    private readonly WindowsPerformanceService _sut = new();

    [Fact]
    public void GetPerformanceSnapshot_ReturnsValidSnapshot()
    {
        // Act
        var result = _sut.GetPerformanceSnapshot();

        // Assert
        result.Should().NotBeNull();
        result.Timestamp.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        result.ProcessCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetPerformanceSnapshot_ReturnsCpuUsage()
    {
        // Act
        var result = _sut.GetPerformanceSnapshot();

        // Assert
        result.CpuUsagePercent.Should().BeGreaterThanOrEqualTo(0);
        result.CpuUsagePercent.Should().BeLessThanOrEqualTo(100);
    }

    [Fact]
    public void GetPerformanceSnapshot_ReturnsMemoryInfo()
    {
        // Act
        var result = _sut.GetPerformanceSnapshot();

        // Assert
        result.TotalMemoryBytes.Should().BeGreaterThan(0);
        result.AvailableMemoryBytes.Should().BeGreaterThan(0);
        result.AvailableMemoryBytes.Should().BeLessThanOrEqualTo(result.TotalMemoryBytes);
        result.MemoryUsagePercent.Should().BeGreaterThanOrEqualTo(0);
        result.MemoryUsagePercent.Should().BeLessThanOrEqualTo(100);
    }

    [Fact]
    public void GetCategories_ReturnsMultipleCategories()
    {
        // Act
        var result = _sut.GetCategories().ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCountGreaterThan(10); // Should have many categories
        // Check for some common category names (not just first 20)
        result.Should().Contain(c => c.Name.Contains("Processor") || c.Name.Contains("Memory") || c.Name.Contains("Process"));
    }

    [Fact]
    public void GetCategories_CategoriesHaveCounters()
    {
        // Act
        var processorCategory = _sut.GetCategories().FirstOrDefault(c => c.Name == "Processor");

        // Assert
        processorCategory.Should().NotBeNull();
        processorCategory!.Counters.Should().NotBeEmpty();
        processorCategory.Counters.Should().Contain("% Processor Time");
    }

    [Fact]
    public void GetCounterValue_ForProcessorTime_ReturnsValue()
    {
        // Act
        var result = _sut.GetCounterValue("Processor", "% Processor Time", "_Total");

        // Assert
        result.Should().NotBeNull();
        result!.CategoryName.Should().Be("Processor");
        result.CounterName.Should().Be("% Processor Time");
        result.Value.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void GetCounterValue_ForMemory_ReturnsValue()
    {
        // Act
        var result = _sut.GetCounterValue("Memory", "Available Bytes");

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetCounterValue_ForInvalidCategory_ReturnsNull()
    {
        // Act
        var result = _sut.GetCounterValue("NonExistentCategory", "SomeCounter");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetCategoryCounters_ForMemory_ReturnsMultipleCounters()
    {
        // Act
        var result = _sut.GetCategoryCounters("Memory").ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(c => c.CounterName == "Available Bytes");
    }

    [Fact]
    public void GetCategoryCounters_ForInvalidCategory_ReturnsEmpty()
    {
        // Act
        var result = _sut.GetCategoryCounters("NonExistentCategory").ToList();

        // Assert
        result.Should().BeEmpty();
    }
}
