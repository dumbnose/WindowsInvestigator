# Copilot Instructions for WindowsInvestigator

## MCP Server Usage

When investigating Windows issues in this repository, **always use the WindowsInvestigator MCP server tools** instead of native PowerShell cmdlets for data collection.

### Use MCP Tools Instead Of:

| Instead of... | Use MCP Tool |
|---------------|--------------|
| `Get-WinEvent` | `QueryEventLog` or `ListEventLogs` |
| `Get-ComputerInfo`, `systeminfo` | `GetSystemInfo` |
| `Get-Service` | `ListServices`, `GetService`, `SearchServices` |
| `Test-NetConnection` | `TestConnection` |
| `Resolve-DnsName` | `ResolveDns` |
| `Get-NetAdapter` | `GetNetworkAdapters` |
| `Get-Printer`, `Get-WmiObject Win32_Printer` | `ListPrinters`, `GetPrinter` |
| `Get-PrintJob` | `GetPrintJobs`, `GetAllPrintJobs` |
| `Get-Content`, `Select-String` on log files | `DiscoverLogs`, `ReadLog` |
| `Get-ItemProperty` (Registry) | `GetRegistryKey`, `GetRegistryValue` |
| `Get-ChildItem` (Registry) | `SearchRegistryKeys`, `SearchRegistryValues` |
| `Get-Process` | `ListProcesses`, `GetProcess`, `SearchProcesses` |
| `ps`, `tasklist` | `GetProcessSummary` |
| `Get-Counter`, Performance Monitor | `GetPerformanceSnapshot`, `GetPerformanceCounter` |
| `Get-HotFix`, Windows Update history | `GetUpdateStatus`, `GetUpdateHistory` |
| Reliability Monitor | `GetReliabilityEvents`, `GetApplicationCrashes` |
| `Get-ScheduledTask`, Task Scheduler | `ListScheduledTasks`, `GetScheduledTask` |

### Available MCP Tools

#### Event Log Tools
1. **ListEventLogs** - Lists all available Windows Event Log names
2. **QueryEventLog** - Queries events from a Windows Event Log
   - Parameters:
     - `logName` (required): Name of the event log (e.g., System, Application, Security)
     - `level` (optional): Filter by level (Critical, Error, Warning, Information, Verbose)
     - `source` (optional): Filter by event source
     - `maxResults` (optional): Maximum number of events to return (default: 50)
     - `reverseChronological` (optional): Return most recent events first (default: true)
     - `startTime` (optional): Filter events after this time (ISO 8601 format)
     - `endTime` (optional): Filter events before this time (ISO 8601 format)

#### System Info Tools
3. **GetSystemInfo** - Gets comprehensive system information including OS, hardware, memory, disk, and network details

#### Service Tools
4. **ListServices** - Lists all Windows services with their status
5. **GetService** - Gets detailed information about a specific Windows service
   - Parameters:
     - `serviceName` (required): The service name (e.g., "Spooler", "wuauserv")
6. **SearchServices** - Searches for Windows services by name pattern
   - Parameters:
     - `searchPattern` (required): Pattern to search for in service names/display names

#### Network Tools
7. **TestConnection** - Tests network connectivity to a host (like ping/Test-NetConnection)
   - Parameters:
     - `host` (required): Hostname or IP address to test
     - `port` (optional): TCP port to test (if provided, tests TCP connectivity)
8. **ResolveDns** - Resolves a hostname to IP addresses
   - Parameters:
     - `hostname` (required): Hostname to resolve
9. **GetNetworkAdapters** - Gets information about all network adapters

#### Print Tools
10. **ListPrinters** - Lists all installed printers with their status
11. **GetPrinter** - Gets detailed information about a specific printer
    - Parameters:
      - `printerName` (required): Name of the printer
12. **GetPrintJobs** - Gets print jobs for a specific printer
    - Parameters:
      - `printerName` (required): Name of the printer
13. **GetAllPrintJobs** - Gets print jobs from all printers

#### File Log Tools
14. **DiscoverLogs** - Discovers log files in common Windows locations
    - Parameters:
      - `includeSystemLogs` (optional): Include system log locations (default: true)
      - `maxFiles` (optional): Maximum number of files to return (default: 100)
15. **ReadLog** - Reads content from a log file
    - Parameters:
      - `path` (required): Full path to the log file
      - `tailLines` (optional): Number of lines to read from end of file
      - `searchPattern` (optional): Regex pattern to filter lines
      - `maxLines` (optional): Maximum number of lines to return (default: 500)

#### Registry Tools
16. **GetRegistryKey** - Gets information about a registry key including subkeys and values
    - Parameters:
      - `path` (required): Registry path (e.g., HKLM\SOFTWARE\Microsoft)
17. **GetRegistryValue** - Gets a specific value from a registry key
    - Parameters:
      - `path` (required): Registry path
      - `valueName` (required): Name of the value (empty string for default)
18. **SearchRegistryKeys** - Searches for registry keys matching a pattern
    - Parameters:
      - `basePath` (required): Base registry path to search from
      - `pattern` (required): Regex pattern to match key names
      - `maxResults` (optional): Maximum number of results (default: 100)
19. **SearchRegistryValues** - Searches for registry values matching a pattern
    - Parameters:
      - `basePath` (required): Base registry path to search from
      - `pattern` (required): Regex pattern to match value names or data
      - `maxResults` (optional): Maximum number of results (default: 100)

#### Process Tools
20. **ListProcesses** - Lists all running processes on the system
21. **GetProcess** - Gets information about a specific process by ID
    - Parameters:
      - `processId` (required): The process ID
22. **SearchProcesses** - Searches for processes matching a pattern
    - Parameters:
      - `pattern` (required): Regex pattern to match process name or command line
23. **GetProcessSummary** - Gets a summary of system resource usage including top CPU and memory consumers

#### Performance Tools
24. **GetPerformanceSnapshot** - Gets a snapshot of current system performance (CPU, memory, disk, network)
25. **ListPerformanceCategories** - Lists available performance counter categories
26. **GetPerformanceCounter** - Gets the value of a specific performance counter
    - Parameters:
      - `categoryName` (required): The performance counter category (e.g., "Processor", "Memory")
      - `counterName` (required): The counter name (e.g., "% Processor Time")
      - `instanceName` (optional): Instance name (e.g., "_Total")
27. **GetCategoryCounters** - Gets all counter values for a performance category
    - Parameters:
      - `categoryName` (required): The category name
      - `instanceName` (optional): Instance name

#### Windows Update Tools
28. **GetUpdateStatus** - Gets current Windows Update status including pending updates and reboot requirements
29. **GetUpdateHistory** - Gets history of installed Windows updates
    - Parameters:
      - `maxResults` (optional): Maximum number of updates to return (default: 50)
30. **GetPendingUpdates** - Gets pending updates waiting to be installed
31. **GetUpdateFailures** - Gets recent Windows Update failures
    - Parameters:
      - `maxResults` (optional): Maximum number of failures to return (default: 20)

#### Reliability Tools
32. **GetReliabilityEvents** - Gets reliability events (crashes, hangs, failures) from Reliability Monitor
    - Parameters:
      - `startTime` (optional): Start time for query (default: 30 days ago)
      - `endTime` (optional): End time for query (default: now)
      - `maxResults` (optional): Maximum number of events (default: 50)
33. **GetApplicationCrashes** - Gets recent application crashes
    - Parameters:
      - `maxResults` (optional): Maximum number of crashes (default: 20)
34. **GetApplicationHangs** - Gets recent application hangs
    - Parameters:
      - `maxResults` (optional): Maximum number of hangs (default: 20)
35. **GetSystemFailures** - Gets system failures (BSODs, unexpected shutdowns)
    - Parameters:
      - `maxResults` (optional): Maximum number of failures (default: 20)
36. **GetReliabilityScores** - Gets daily reliability scores showing system stability over time
    - Parameters:
      - `days` (optional): Number of days of history (default: 30)

#### Scheduled Task Tools
37. **ListScheduledTasks** - Lists all scheduled tasks on the system
    - Parameters:
      - `includeHidden` (optional): Include hidden tasks (default: false)
38. **GetScheduledTask** - Gets information about a specific scheduled task
    - Parameters:
      - `taskPath` (required): Full path of the task (e.g., "\\Microsoft\\Windows\\WindowsUpdate\\Scheduled Start")
39. **SearchScheduledTasks** - Searches for scheduled tasks matching a pattern
    - Parameters:
      - `pattern` (required): Regex pattern to match task name, path, or description
40. **GetFailedTasks** - Gets scheduled tasks that have failed recently
41. **GetTaskHistory** - Gets the run history for a specific scheduled task
    - Parameters:
      - `taskPath` (required): Full path of the task
      - `maxResults` (optional): Maximum number of history entries (default: 20)

### Why Use MCP Tools?

1. **Dogfooding** - We're building these tools, so we should use them
2. **Testing** - Using the tools helps us find bugs and missing features
3. **Consistency** - Demonstrates the intended usage patterns
4. **Feature Discovery** - Helps identify gaps in functionality that need to be added

## Code Quality Requirements

### Test Coverage

**All code must have full test coverage before being checked in.** This includes:

1. **Unit Tests** (in `tests/WindowsInvestigator.McpServer.Tests/`)
   - Test each MCP tool class with mocked dependencies
   - Verify correct parameter passing to services
   - Test edge cases (null, empty, invalid inputs)
   - Use NSubstitute for mocking interfaces

2. **Integration Tests** (in `tests/WindowsInvestigator.McpServer.IntegrationTests/`)
   - Test service implementations against real Windows APIs
   - Verify actual Windows data is returned correctly
   - Test error handling with invalid inputs

### Test Patterns

- Use FluentAssertions for readable assertions
- Use NSubstitute for mocking
- Follow Arrange-Act-Assert pattern
- Name tests descriptively: `MethodName_Scenario_ExpectedBehavior`

### Before Committing

1. Ensure all new code has corresponding tests
2. Run `dotnet test` and verify all tests pass
3. Verify build succeeds with `dotnet build`

### Adding New Features

If you encounter a scenario where the MCP server doesn't have the needed functionality:
1. Note the gap
2. Suggest adding the feature to the implementation plan
3. Consider implementing it as part of the current session
