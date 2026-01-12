namespace WindowsInvestigator.McpServer.Services;

/// <summary>
/// Printer information.
/// </summary>
public class PrinterInfo
{
    public string Name { get; set; } = "";
    public string DriverName { get; set; } = "";
    public string PortName { get; set; } = "";
    public string Status { get; set; } = "";
    public bool IsDefault { get; set; }
    public bool IsShared { get; set; }
    public string? Location { get; set; }
    public string? Comment { get; set; }
}

/// <summary>
/// Print job information.
/// </summary>
public class PrintJobInfo
{
    public int JobId { get; set; }
    public string DocumentName { get; set; } = "";
    public string Status { get; set; } = "";
    public string UserName { get; set; } = "";
    public int SizeBytes { get; set; }
    public int Pages { get; set; }
    public DateTime SubmittedTime { get; set; }
}

/// <summary>
/// Abstraction for print service access.
/// </summary>
public interface IPrintService
{
    /// <summary>
    /// Gets all installed printers.
    /// </summary>
    IEnumerable<PrinterInfo> GetPrinters();

    /// <summary>
    /// Gets a specific printer by name.
    /// </summary>
    PrinterInfo? GetPrinter(string printerName);

    /// <summary>
    /// Gets print jobs for a specific printer.
    /// </summary>
    IEnumerable<PrintJobInfo> GetPrintJobs(string printerName);

    /// <summary>
    /// Gets all print jobs across all printers.
    /// </summary>
    IEnumerable<PrintJobInfo> GetAllPrintJobs();
}
