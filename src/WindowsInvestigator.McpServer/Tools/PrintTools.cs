using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using WindowsInvestigator.McpServer.Exceptions;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.Tools;

[McpServerToolType]
public sealed class PrintTools
{
    private readonly IPrintService _printService;
    private readonly ILogger<PrintTools> _logger;

    public PrintTools(IPrintService printService, ILogger<PrintTools> logger)
    {
        _printService = printService;
        _logger = logger;
    }

    [McpServerTool, Description("Lists all installed printers on the system")]
    public IEnumerable<PrinterInfo> ListPrinters()
    {
        try
        {
            _logger.LogDebug("Listing all printers");
            var printers = _printService.GetPrinters().ToList();
            _logger.LogInformation("Found {Count} printers", printers.Count);
            return printers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list printers");
            throw new WindowsApiException("WMI Win32_Printer", ex);
        }
    }

    [McpServerTool, Description("Gets detailed information about a specific printer")]
    public PrinterInfo? GetPrinter(
        [Description("The name of the printer")] string printerName)
    {
        if (string.IsNullOrWhiteSpace(printerName))
        {
            throw new InvalidParameterException("printerName", "Printer name cannot be empty");
        }

        try
        {
            _logger.LogDebug("Getting printer info for {PrinterName}", printerName);
            var printer = _printService.GetPrinter(printerName);
            
            if (printer == null)
            {
                _logger.LogWarning("Printer {PrinterName} not found", printerName);
            }
            else
            {
                _logger.LogInformation("Retrieved printer {PrinterName} (Status: {Status})", printerName, printer.Status);
            }
            
            return printer;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to get printer {PrinterName}", printerName);
            throw new WindowsApiException("WMI Win32_Printer", ex);
        }
    }

    [McpServerTool, Description("Gets print jobs for a specific printer")]
    public IEnumerable<PrintJobInfo> GetPrintJobs(
        [Description("The name of the printer")] string printerName)
    {
        if (string.IsNullOrWhiteSpace(printerName))
        {
            throw new InvalidParameterException("printerName", "Printer name cannot be empty");
        }

        try
        {
            _logger.LogDebug("Getting print jobs for {PrinterName}", printerName);
            var jobs = _printService.GetPrintJobs(printerName).ToList();
            _logger.LogInformation("Found {Count} print jobs for {PrinterName}", jobs.Count, printerName);
            return jobs;
        }
        catch (Exception ex) when (ex is not WindowsInvestigatorException)
        {
            _logger.LogError(ex, "Failed to get print jobs for {PrinterName}", printerName);
            throw new WindowsApiException("WMI Win32_PrintJob", ex);
        }
    }

    [McpServerTool, Description("Gets all print jobs across all printers")]
    public IEnumerable<PrintJobInfo> GetAllPrintJobs()
    {
        try
        {
            _logger.LogDebug("Getting all print jobs");
            var jobs = _printService.GetAllPrintJobs().ToList();
            _logger.LogInformation("Found {Count} total print jobs", jobs.Count);
            return jobs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all print jobs");
            throw new WindowsApiException("WMI Win32_PrintJob", ex);
        }
    }
}
