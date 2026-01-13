# WindowsInvestigator

An MCP (Model Context Protocol) server that exposes Windows diagnostic data sources to AI agents like GitHub Copilot CLI, enabling natural language investigation of Windows PC issues.

## Features

WindowsInvestigator provides 41+ diagnostic tools across 12 categories:

| Category | Tools | Description |
|----------|-------|-------------|
| **Event Logs** | ListEventLogs, QueryEventLog | Query Windows Event Logs (System, Application, Security, etc.) |
| **System Info** | GetSystemInfo | OS version, hardware, memory, disk, and network details |
| **Services** | ListServices, GetService, SearchServices | Windows service status and details |
| **Processes** | ListProcesses, GetProcess, SearchProcesses | Running processes with resource usage |
| **Network** | TestConnection, ResolveDns, GetNetworkAdapters | Network diagnostics and connectivity tests |
| **Printing** | ListPrinters, GetPrinter, GetPrintJobs, GetAllPrintJobs | Printer status and print job management |
| **File Logs** | DiscoverLogs, ReadLog | Discover and read log files from common locations |
| **Registry** | GetRegistryKey, GetRegistryValue, SearchRegistry | Windows Registry exploration |
| **Performance** | GetCpuUsage, GetMemoryUsage, GetDiskUsage, GetPerformanceCategories | System performance metrics |
| **Windows Update** | GetUpdateHistory, GetPendingUpdates, GetUpdateSettings, GetRebootStatus | Windows Update status and history |
| **Reliability** | GetReliabilityEvents, GetSystemCrashes, GetAppHangs, GetAppCrashes, GetSystemReboots | Reliability Monitor data |
| **Scheduled Tasks** | ListScheduledTasks, GetScheduledTask, GetTaskHistory, SearchScheduledTasks, GetFailedTasks | Task Scheduler information |

## Installation

### Prerequisites

- Windows 10/11 or Windows Server 2016+
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [GitHub Copilot CLI](https://docs.github.com/en/copilot/using-github-copilot/using-github-copilot-in-the-command-line)

### Quick Install

Run the installation script from the repository root:

```powershell
.\scripts\Install-WindowsInvestigator.ps1
```

This will:
1. Build the MCP server in Release configuration
2. Register it with GitHub Copilot CLI at `~/.github-copilot/mcp.json`

### Manual Installation

1. Clone the repository:
   ```powershell
   git clone https://github.com/dumbnose/WindowsInvestigator.git
   cd WindowsInvestigator
   ```

2. Build the solution:
   ```powershell
   dotnet build -c Release
   ```

3. Register with GitHub Copilot CLI by adding to `~/.github-copilot/mcp.json`:
   ```json
   {
     "mcpServers": {
       "WindowsInvestigator": {
         "command": "C:\\path\\to\\WindowsInvestigator.McpServer.exe",
         "args": []
       }
     }
   }
   ```

### Uninstall

```powershell
.\scripts\Install-WindowsInvestigator.ps1 -Uninstall
```

## Usage

Once installed, use WindowsInvestigator through GitHub Copilot CLI with natural language queries:

```powershell
gh copilot
```

### Example Queries

**Event Log Investigation:**
- "What errors are in the System event log from the last hour?"
- "Show me application crashes in the event log"
- "Are there any security audit failures?"

**System Diagnostics:**
- "What's my system information?"
- "How much memory is being used?"
- "What's the CPU usage?"

**Service Troubleshooting:**
- "Is the Print Spooler service running?"
- "What services are stopped?"
- "Search for services related to Windows Update"

**Network Issues:**
- "Can I reach google.com on port 443?"
- "What's the IP address of microsoft.com?"
- "Show me all network adapters"

**Reliability Problems:**
- "What applications have crashed recently?"
- "Show me unexpected system reboots"
- "What failed scheduled tasks are there?"

**Windows Update:**
- "What Windows updates were installed recently?"
- "Are there pending updates?"
- "Does Windows need a reboot?"

## Configuration

Configuration options are in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "WindowsInvestigator": "Information"
    }
  },
  "WindowsInvestigator": {
    "EventLog": {
      "DefaultMaxResults": 50
    },
    "FileLog": {
      "DefaultMaxFiles": 100,
      "DefaultMaxLines": 500
    }
  }
}
```

### Environment Variables

- `WINDOWSINVESTIGATOR_LOGPATH` - Enable console logging (set to any path)

## Development

### Building

```powershell
dotnet build
```

### Testing

```powershell
# Run all tests
dotnet test

# Run unit tests only
dotnet test tests/WindowsInvestigator.McpServer.Tests

# Run integration tests only
dotnet test tests/WindowsInvestigator.McpServer.IntegrationTests
```

### Project Structure

```
WindowsInvestigator/
├── src/
│   └── WindowsInvestigator.McpServer/
│       ├── Services/          # Windows API wrappers (injectable)
│       ├── Tools/             # MCP tool definitions
│       ├── Exceptions/        # Custom exceptions
│       └── Program.cs         # Entry point and DI configuration
├── tests/
│   ├── WindowsInvestigator.McpServer.Tests/           # Unit tests
│   └── WindowsInvestigator.McpServer.IntegrationTests/ # Integration tests
├── docs/
│   └── implementation-plan.md
└── scripts/
    └── Install-WindowsInvestigator.ps1
```

### Architecture

The project follows a testable architecture pattern:

1. **Interfaces** (`IEventLogService`, `ISystemInfoService`, etc.) - Define contracts
2. **Services** (`WindowsEventLogService`, etc.) - Implement Windows API calls
3. **Tools** (`EventLogTools`, etc.) - Expose services via MCP protocol

This allows unit testing with mocked services while integration tests verify actual Windows API behavior.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality (full coverage required)
4. Ensure all tests pass: `dotnet test`
5. Submit a pull request

## License

MIT License - see [LICENSE](LICENSE) for details.

## Resources

- [MCP Specification](https://modelcontextprotocol.io/)
- [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [GitHub Copilot CLI](https://docs.github.com/en/copilot/using-github-copilot/using-github-copilot-in-the-command-line)
