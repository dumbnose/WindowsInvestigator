using System.Diagnostics;

namespace WindowsInvestigator.McpServer.Services;

/// <summary>
/// Windows implementation of performance counter access.
/// </summary>
public class WindowsPerformanceService : IPerformanceService
{
    public PerformanceSnapshot GetPerformanceSnapshot()
    {
        var snapshot = new PerformanceSnapshot
        {
            Timestamp = DateTime.Now
        };

        try
        {
            // CPU usage
            using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
            cpuCounter.NextValue(); // First call always returns 0
            System.Threading.Thread.Sleep(100);
            snapshot.CpuUsagePercent = cpuCounter.NextValue();
        }
        catch { }

        try
        {
            // Memory
            using var availMemCounter = new PerformanceCounter("Memory", "Available Bytes", true);
            snapshot.AvailableMemoryBytes = (long)availMemCounter.NextValue();

            // Get total memory from system info
            var gcMemInfo = GC.GetGCMemoryInfo();
            snapshot.TotalMemoryBytes = gcMemInfo.TotalAvailableMemoryBytes;
            
            if (snapshot.TotalMemoryBytes > 0)
            {
                snapshot.MemoryUsagePercent = (float)((snapshot.TotalMemoryBytes - snapshot.AvailableMemoryBytes) * 100.0 / snapshot.TotalMemoryBytes);
            }
        }
        catch { }

        try
        {
            // Disk I/O
            using var diskReadCounter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", "_Total", true);
            using var diskWriteCounter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", "_Total", true);
            snapshot.DiskReadBytesPerSec = diskReadCounter.NextValue();
            snapshot.DiskWriteBytesPerSec = diskWriteCounter.NextValue();
        }
        catch { }

        try
        {
            // Network I/O
            using var networkSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", GetFirstNetworkInstance(), true);
            using var networkRecvCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", GetFirstNetworkInstance(), true);
            snapshot.NetworkSentBytesPerSec = networkSentCounter.NextValue();
            snapshot.NetworkReceivedBytesPerSec = networkRecvCounter.NextValue();
        }
        catch { }

        try
        {
            // Process/Thread/Handle counts
            using var procCounter = new PerformanceCounter("System", "Processes", true);
            using var threadCounter = new PerformanceCounter("System", "Threads", true);
            using var handleCounter = new PerformanceCounter("Process", "Handle Count", "_Total", true);
            snapshot.ProcessCount = (int)procCounter.NextValue();
            snapshot.ThreadCount = (int)threadCounter.NextValue();
            snapshot.HandleCount = (int)handleCounter.NextValue();
        }
        catch { }

        return snapshot;
    }

    public IEnumerable<PerformanceCategory> GetCategories()
    {
        var categories = PerformanceCounterCategory.GetCategories();
        
        foreach (var cat in categories.OrderBy(c => c.CategoryName))
        {
            var perfCat = new PerformanceCategory
            {
                Name = cat.CategoryName,
                Description = cat.CategoryHelp
            };

            try
            {
                // Get instances
                var instances = cat.GetInstanceNames();
                perfCat.Instances = instances.OrderBy(i => i).Take(10).ToList();

                // Get counter names (from first instance or default)
                var instanceName = instances.Length > 0 ? instances[0] : null;
                var counters = instanceName != null 
                    ? cat.GetCounters(instanceName) 
                    : cat.GetCounters();
                perfCat.Counters = counters.Select(c => c.CounterName).OrderBy(n => n).ToList();
            }
            catch
            {
                // Some categories may not be accessible
            }

            yield return perfCat;
        }
    }

    public PerformanceCounterValue? GetCounterValue(string categoryName, string counterName, string? instanceName = null)
    {
        try
        {
            using var counter = instanceName != null
                ? new PerformanceCounter(categoryName, counterName, instanceName, true)
                : new PerformanceCounter(categoryName, counterName, true);

            return new PerformanceCounterValue
            {
                CategoryName = categoryName,
                CounterName = counterName,
                InstanceName = instanceName ?? "",
                Value = counter.NextValue()
            };
        }
        catch
        {
            return null;
        }
    }

    public IEnumerable<PerformanceCounterValue> GetCategoryCounters(string categoryName, string? instanceName = null)
    {
        var results = new List<PerformanceCounterValue>();
        
        try
        {
            var category = new PerformanceCounterCategory(categoryName);
            var counters = instanceName != null
                ? category.GetCounters(instanceName)
                : category.GetCounters();

            foreach (var counter in counters)
            {
                try
                {
                    results.Add(new PerformanceCounterValue
                    {
                        CategoryName = categoryName,
                        CounterName = counter.CounterName,
                        InstanceName = instanceName ?? "",
                        Value = counter.NextValue()
                    });
                }
                catch
                {
                    // Skip counters that fail
                }
                finally
                {
                    counter.Dispose();
                }
            }
        }
        catch
        {
            // Category not found or inaccessible
        }

        return results;
    }

    private string GetFirstNetworkInstance()
    {
        try
        {
            var category = new PerformanceCounterCategory("Network Interface");
            var instances = category.GetInstanceNames();
            return instances.FirstOrDefault() ?? "";
        }
        catch
        {
            return "";
        }
    }
}
