using ModelContextProtocol.Server;
using System.ComponentModel;
using WindowsInvestigator.McpServer.Services;

namespace WindowsInvestigator.McpServer.Tools;

[McpServerToolType]
public sealed class PrintTools
{
    private readonly IPrintService _printService;

    public PrintTools(IPrintService printService)
    {
        _printService = printService;
    }

    [McpServerTool, Description("Lists all installed printers on the system")]
    public IEnumerable<PrinterInfo> ListPrinters()
    {
        return _printService.GetPrinters();
    }

    [McpServerTool, Description("Gets detailed information about a specific printer")]
    public PrinterInfo? GetPrinter(
        [Description("The name of the printer")] string printerName)
    {
        return _printService.GetPrinter(printerName);
    }

    [McpServerTool, Description("Gets print jobs for a specific printer")]
    public IEnumerable<PrintJobInfo> GetPrintJobs(
        [Description("The name of the printer")] string printerName)
    {
        return _printService.GetPrintJobs(printerName);
    }

    [McpServerTool, Description("Gets all print jobs across all printers")]
    public IEnumerable<PrintJobInfo> GetAllPrintJobs()
    {
        return _printService.GetAllPrintJobs();
    }
}
