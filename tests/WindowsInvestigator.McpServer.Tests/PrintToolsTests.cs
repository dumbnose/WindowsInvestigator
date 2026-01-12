using FluentAssertions;
using NSubstitute;
using WindowsInvestigator.McpServer.Services;
using WindowsInvestigator.McpServer.Tools;

namespace WindowsInvestigator.McpServer.Tests;

public class PrintToolsTests
{
    private readonly IPrintService _printService;
    private readonly PrintTools _sut;

    public PrintToolsTests()
    {
        _printService = Substitute.For<IPrintService>();
        _sut = new PrintTools(_printService);
    }

    [Fact]
    public void ListPrinters_ReturnsPrintersFromService()
    {
        // Arrange
        var expectedPrinters = new[]
        {
            new PrinterInfo
            {
                Name = "HP LaserJet",
                DriverName = "HP Universal Printing PCL 6",
                PortName = "USB001",
                Status = "Ready",
                IsDefault = true,
                IsShared = false
            },
            new PrinterInfo
            {
                Name = "Microsoft Print to PDF",
                DriverName = "Microsoft Print To PDF",
                PortName = "PORTPROMPT:",
                Status = "Ready",
                IsDefault = false,
                IsShared = false
            }
        };
        _printService.GetPrinters().Returns(expectedPrinters);

        // Act
        var result = _sut.ListPrinters();

        // Assert
        result.Should().BeEquivalentTo(expectedPrinters);
        _printService.Received(1).GetPrinters();
    }

    [Fact]
    public void ListPrinters_WhenNoPrinters_ReturnsEmpty()
    {
        // Arrange
        _printService.GetPrinters().Returns(Enumerable.Empty<PrinterInfo>());

        // Act
        var result = _sut.ListPrinters();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetPrinter_WithValidName_ReturnsPrinter()
    {
        // Arrange
        var expectedPrinter = new PrinterInfo
        {
            Name = "HP LaserJet",
            DriverName = "HP Universal Printing PCL 6",
            PortName = "USB001",
            Status = "Ready",
            IsDefault = true,
            IsShared = false,
            Location = "Office 101",
            Comment = "Main office printer"
        };
        _printService.GetPrinter("HP LaserJet").Returns(expectedPrinter);

        // Act
        var result = _sut.GetPrinter("HP LaserJet");

        // Assert
        result.Should().BeEquivalentTo(expectedPrinter);
        _printService.Received(1).GetPrinter("HP LaserJet");
    }

    [Fact]
    public void GetPrinter_WithNonExistentName_ReturnsNull()
    {
        // Arrange
        _printService.GetPrinter("NonExistentPrinter").Returns((PrinterInfo?)null);

        // Act
        var result = _sut.GetPrinter("NonExistentPrinter");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetPrinter_PassesPrinterNameToService()
    {
        // Arrange
        var printerName = "Canon Printer";
        _printService.GetPrinter(printerName).Returns(new PrinterInfo { Name = printerName });

        // Act
        _ = _sut.GetPrinter(printerName);

        // Assert
        _printService.Received(1).GetPrinter(printerName);
    }

    [Fact]
    public void GetPrintJobs_WithPrinterName_ReturnsJobs()
    {
        // Arrange
        var expectedJobs = new[]
        {
            new PrintJobInfo
            {
                JobId = 1,
                DocumentName = "Document1.docx",
                Status = "Printing",
                UserName = "User1",
                SizeBytes = 102400,
                Pages = 5,
                SubmittedTime = DateTime.Now.AddMinutes(-2)
            },
            new PrintJobInfo
            {
                JobId = 2,
                DocumentName = "Report.pdf",
                Status = "Spooling",
                UserName = "User2",
                SizeBytes = 204800,
                Pages = 10,
                SubmittedTime = DateTime.Now.AddMinutes(-1)
            }
        };
        _printService.GetPrintJobs("HP LaserJet").Returns(expectedJobs);

        // Act
        var result = _sut.GetPrintJobs("HP LaserJet");

        // Assert
        result.Should().BeEquivalentTo(expectedJobs);
        _printService.Received(1).GetPrintJobs("HP LaserJet");
    }

    [Fact]
    public void GetPrintJobs_WithNoJobs_ReturnsEmpty()
    {
        // Arrange
        _printService.GetPrintJobs("Idle Printer").Returns(Enumerable.Empty<PrintJobInfo>());

        // Act
        var result = _sut.GetPrintJobs("Idle Printer");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetPrintJobs_PassesPrinterNameToService()
    {
        // Arrange
        var printerName = "Network Printer";
        _printService.GetPrintJobs(printerName).Returns(Enumerable.Empty<PrintJobInfo>());

        // Act
        _ = _sut.GetPrintJobs(printerName);

        // Assert
        _printService.Received(1).GetPrintJobs(printerName);
    }

    [Fact]
    public void GetAllPrintJobs_ReturnsAllJobsFromService()
    {
        // Arrange
        var expectedJobs = new[]
        {
            new PrintJobInfo { JobId = 1, DocumentName = "Doc1.docx" },
            new PrintJobInfo { JobId = 2, DocumentName = "Doc2.pdf" },
            new PrintJobInfo { JobId = 3, DocumentName = "Doc3.xlsx" }
        };
        _printService.GetAllPrintJobs().Returns(expectedJobs);

        // Act
        var result = _sut.GetAllPrintJobs();

        // Assert
        result.Should().BeEquivalentTo(expectedJobs);
        _printService.Received(1).GetAllPrintJobs();
    }

    [Fact]
    public void GetAllPrintJobs_WithNoJobs_ReturnsEmpty()
    {
        // Arrange
        _printService.GetAllPrintJobs().Returns(Enumerable.Empty<PrintJobInfo>());

        // Act
        var result = _sut.GetAllPrintJobs();

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("HP LaserJet Pro")]
    [InlineData("Canon PIXMA")]
    [InlineData("Microsoft Print to PDF")]
    public void GetPrinter_WithDifferentPrinterNames_PassesCorrectName(string printerName)
    {
        // Arrange
        _printService.GetPrinter(printerName).Returns(new PrinterInfo { Name = printerName });

        // Act
        var result = _sut.GetPrinter(printerName);

        // Assert
        result!.Name.Should().Be(printerName);
    }
}
