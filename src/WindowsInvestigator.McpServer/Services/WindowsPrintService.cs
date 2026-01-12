using System.Management;

namespace WindowsInvestigator.McpServer.Services;

/// <summary>
/// Windows print service implementation.
/// </summary>
public class WindowsPrintService : IPrintService
{
    public IEnumerable<PrinterInfo> GetPrinters()
    {
        var printers = new List<PrinterInfo>();

        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT Name, DriverName, PortName, PrinterStatus, Default, Shared, Location, Comment FROM Win32_Printer");

            foreach (ManagementObject printer in searcher.Get())
            {
                printers.Add(new PrinterInfo
                {
                    Name = printer["Name"]?.ToString() ?? "",
                    DriverName = printer["DriverName"]?.ToString() ?? "",
                    PortName = printer["PortName"]?.ToString() ?? "",
                    Status = GetPrinterStatusString(Convert.ToInt32(printer["PrinterStatus"] ?? 0)),
                    IsDefault = Convert.ToBoolean(printer["Default"] ?? false),
                    IsShared = Convert.ToBoolean(printer["Shared"] ?? false),
                    Location = printer["Location"]?.ToString(),
                    Comment = printer["Comment"]?.ToString()
                });
            }
        }
        catch
        {
            // WMI might fail
        }

        return printers.OrderBy(p => p.Name).ToList();
    }

    public PrinterInfo? GetPrinter(string printerName)
    {
        return GetPrinters().FirstOrDefault(p => 
            p.Name.Equals(printerName, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<PrintJobInfo> GetPrintJobs(string printerName)
    {
        var jobs = new List<PrintJobInfo>();

        try
        {
            var escapedName = printerName.Replace("\\", "\\\\").Replace("'", "\\'");
            using var searcher = new ManagementObjectSearcher(
                $"SELECT JobId, Document, JobStatus, Owner, Size, TotalPages, TimeSubmitted FROM Win32_PrintJob WHERE Name LIKE '{escapedName}%'");

            foreach (ManagementObject job in searcher.Get())
            {
                jobs.Add(MapPrintJob(job));
            }
        }
        catch
        {
            // WMI might fail
        }

        return jobs.OrderByDescending(j => j.SubmittedTime).ToList();
    }

    public IEnumerable<PrintJobInfo> GetAllPrintJobs()
    {
        var jobs = new List<PrintJobInfo>();

        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT JobId, Document, JobStatus, Owner, Size, TotalPages, TimeSubmitted FROM Win32_PrintJob");

            foreach (ManagementObject job in searcher.Get())
            {
                jobs.Add(MapPrintJob(job));
            }
        }
        catch
        {
            // WMI might fail
        }

        return jobs.OrderByDescending(j => j.SubmittedTime).ToList();
    }

    private static PrintJobInfo MapPrintJob(ManagementObject job)
    {
        var info = new PrintJobInfo
        {
            JobId = Convert.ToInt32(job["JobId"] ?? 0),
            DocumentName = job["Document"]?.ToString() ?? "",
            Status = job["JobStatus"]?.ToString() ?? "Unknown",
            UserName = job["Owner"]?.ToString() ?? "",
            SizeBytes = Convert.ToInt32(job["Size"] ?? 0),
            Pages = Convert.ToInt32(job["TotalPages"] ?? 0)
        };

        var timeStr = job["TimeSubmitted"]?.ToString();
        if (!string.IsNullOrEmpty(timeStr))
        {
            info.SubmittedTime = ManagementDateTimeConverter.ToDateTime(timeStr);
        }

        return info;
    }

    private static string GetPrinterStatusString(int status)
    {
        return status switch
        {
            1 => "Other",
            2 => "Unknown",
            3 => "Idle",
            4 => "Printing",
            5 => "Warmup",
            6 => "Stopped Printing",
            7 => "Offline",
            _ => $"Status {status}"
        };
    }
}
