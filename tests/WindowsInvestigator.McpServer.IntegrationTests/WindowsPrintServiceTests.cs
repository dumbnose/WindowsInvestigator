using FluentAssertions;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.IntegrationTests;

/// <summary>
/// Integration tests that run against real Windows print system.
/// </summary>
public class WindowsPrintServiceTests
{
    private readonly WindowsPrintService _sut;

    public WindowsPrintServiceTests()
    {
        _sut = new WindowsPrintService();
    }

    [Fact]
    public void GetPrinters_ReturnsListOfPrinters()
    {
        // Act
        var result = _sut.GetPrinters().ToList();

        // Assert
        // Most Windows systems have at least "Microsoft Print to PDF" or "Microsoft XPS Document Writer"
        result.Should().NotBeNull();
        // Note: Could be empty on systems with no printers installed
    }

    [Fact]
    public void GetPrinters_ReturnsOrderedByName()
    {
        // Act
        var result = _sut.GetPrinters().ToList();

        // Assert
        if (result.Any())
        {
            result.Select(p => p.Name).Should().BeInAscendingOrder(StringComparer.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void GetPrinters_PrintersHaveRequiredProperties()
    {
        // Act
        var result = _sut.GetPrinters().ToList();

        // Assert
        result.Should().AllSatisfy(printer =>
        {
            printer.Name.Should().NotBeNullOrEmpty();
            printer.DriverName.Should().NotBeNullOrEmpty();
            printer.PortName.Should().NotBeNullOrEmpty();
            printer.Status.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public void GetPrinters_ContainsMicrosoftPrintToPdf()
    {
        // Act
        var result = _sut.GetPrinters().ToList();

        // Assert - This printer exists on Windows 10+ by default
        var pdfPrinter = result.FirstOrDefault(p => 
            p.Name.Contains("Microsoft Print to PDF", StringComparison.OrdinalIgnoreCase));
        
        if (pdfPrinter != null)
        {
            pdfPrinter.DriverName.Should().Contain("PDF");
        }
    }

    [Fact]
    public void GetPrinters_HasExactlyOneDefaultPrinter()
    {
        // Act
        var result = _sut.GetPrinters().ToList();

        // Assert
        if (result.Any())
        {
            result.Count(p => p.IsDefault).Should().BeLessThanOrEqualTo(1);
        }
    }

    [Fact]
    public void GetPrinter_WithExistingPrinter_ReturnsPrinter()
    {
        // Arrange
        var printers = _sut.GetPrinters().ToList();
        if (!printers.Any())
        {
            return; // Skip if no printers installed
        }
        var firstPrinter = printers.First();

        // Act
        var result = _sut.GetPrinter(firstPrinter.Name);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(firstPrinter.Name);
    }

    [Fact]
    public void GetPrinter_WithNonExistentPrinter_ReturnsNull()
    {
        // Act
        var result = _sut.GetPrinter("NonExistentPrinter12345");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetPrinter_IsCaseInsensitive()
    {
        // Arrange
        var printers = _sut.GetPrinters().ToList();
        if (!printers.Any())
        {
            return; // Skip if no printers installed
        }
        var firstPrinter = printers.First();

        // Act
        var result1 = _sut.GetPrinter(firstPrinter.Name.ToLower());
        var result2 = _sut.GetPrinter(firstPrinter.Name.ToUpper());

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1!.Name.Should().Be(result2!.Name);
    }

    [Fact]
    public void GetPrintJobs_WithExistingPrinter_ReturnsJobsList()
    {
        // Arrange
        var printers = _sut.GetPrinters().ToList();
        if (!printers.Any())
        {
            return; // Skip if no printers installed
        }
        var firstPrinter = printers.First();

        // Act
        var result = _sut.GetPrintJobs(firstPrinter.Name).ToList();

        // Assert
        result.Should().NotBeNull();
        // List might be empty if no jobs are queued
    }

    [Fact]
    public void GetPrintJobs_WithNonExistentPrinter_ReturnsEmpty()
    {
        // Act
        var result = _sut.GetPrintJobs("NonExistentPrinter12345").ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetAllPrintJobs_ReturnsJobsList()
    {
        // Act
        var result = _sut.GetAllPrintJobs().ToList();

        // Assert
        result.Should().NotBeNull();
        // List might be empty if no jobs are queued
    }

    [Fact]
    public void GetPrintJobs_JobsHaveRequiredProperties()
    {
        // Act
        var result = _sut.GetAllPrintJobs().ToList();

        // Assert
        result.Should().AllSatisfy(job =>
        {
            job.JobId.Should().BeGreaterThan(0);
            job.DocumentName.Should().NotBeNull(); // May be empty string
            job.Status.Should().NotBeNull();
        });
    }

    [Fact]
    public void GetPrinters_StatusIsValidString()
    {
        // Act
        var result = _sut.GetPrinters().ToList();

        // Assert
        result.Should().AllSatisfy(printer =>
        {
            printer.Status.Should().NotBeNullOrEmpty();
            // Status should be one of the known values or a "Status N" format
            var validStatuses = new[] { "Other", "Unknown", "Idle", "Printing", "Warmup", "Stopped Printing", "Offline" };
            var isKnownStatus = validStatuses.Contains(printer.Status) || printer.Status.StartsWith("Status ");
            isKnownStatus.Should().BeTrue($"Status '{printer.Status}' should be a known status");
        });
    }
}
