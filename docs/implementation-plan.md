# WindowsInvestigator Implementation Plan

## Vision
Create MCP (Model Context Protocol) servers that expose Windows diagnostic data sources to AI agents (like GitHub Copilot), enabling natural language investigation of Windows PC issues.

## Architecture Overview
```
┌─────────────────────────────────────────────────────────────┐
│                     AI Agent (e.g., Copilot)                │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ MCP Protocol
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    MCP Server(s)                            │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐           │
│  │ Event Logs  │ │ File Logs   │ │ System Info │  ...      │
│  └─────────────┘ └─────────────┘ └─────────────┘           │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Windows APIs / Data Sources              │
│  • Event Log API    • File System    • WMI/CIM             │
│  • Registry         • Performance    • Services            │
└─────────────────────────────────────────────────────────────┘
```

## Data Sources to Expose

### Phase 1: Core Sources
- [x] **Windows Event Logs** - System, Application, Security, and custom logs
- [ ] **File-based Logs** - Common log locations (e.g., `%TEMP%`, `%ProgramData%`, app-specific logs)
- [x] **System Information** - OS version, hardware, installed software

### Phase 2: Extended Sources
- [ ] **Registry** - Key configuration areas relevant to troubleshooting
- [x] **Services** - Status, startup type, dependencies
- [ ] **Processes** - Running processes, resource usage, command lines
- [x] **Network** - Connections, firewall rules, DNS cache
- [x] **Printing** - Printers, print jobs, status

### Phase 3: Advanced Sources
- [ ] **Performance Counters** - CPU, memory, disk, network metrics
- [ ] **Windows Update** - Update history, pending updates, failures
- [ ] **Reliability Monitor** - Application crashes, system failures
- [ ] **Scheduled Tasks** - Task status, history, failures

## Technology Decisions

### Language Choice
**Recommendation: C# (.NET 8+)**
- Excellent Windows API integration
- Strong MCP SDK support via `mcpdotnet` or `ModelContextProtocol`
- Easy access to WMI/CIM, Event Logs, Registry
- Cross-compilation not needed (Windows-only tool)

### MCP Server Structure Options
1. **Single Server** - One MCP server with multiple tools/resources
2. **Multiple Servers** - Separate servers per data source (more modular)

**Recommendation:** Start with a single server, split later if needed.

## Implementation Phases

### Phase 1: Foundation (Current)
- [x] Repository setup
- [x] Documentation structure
- [x] Project scaffolding (.NET solution)
- [x] Basic MCP server setup
- [x] First tool: Event Log query
- [x] Unit testing framework (xUnit, NSubstitute, FluentAssertions)
- [x] Integration testing framework
- [x] Testable architecture (dependency injection)

### Phase 2: Core Functionality
- [x] Event Log tools (query, filter by level/source, reverse chronological)
- [x] Event Log time range filtering
- [x] File log discovery and reading
- [x] System information resource
- [x] Error handling and logging

### Phase 3: Extended Capabilities
- [ ] Registry exploration tools
- [x] Service status tools (ListServices, GetService, SearchServices)
- [ ] Process inspection tools
- [x] Network diagnostics tools (TestConnection, ResolveDns, GetNetworkAdapters)
- [x] Print diagnostics tools (ListPrinters, GetPrinter, GetPrintJobs, GetAllPrintJobs)

### Phase 4: Polish & Distribution
- [ ] Installation script / MSI
- [ ] Configuration options
- [ ] Documentation for end users
- [ ] Performance optimization

## MCP Tools Design (Initial)

### Event Log Tools
```
eventlog_query
  - logName: string (System, Application, Security, etc.)
  - level: string? (Error, Warning, Information, etc.)
  - source: string? (filter by source)
  - startTime: datetime?
  - endTime: datetime?
  - maxResults: int (default 50)

eventlog_list_logs
  - Returns available log names
```

### File Log Tools
```
filelogs_discover
  - Scans common locations for log files
  - Returns list of discovered logs with metadata

filelogs_read
  - path: string
  - tailLines: int? (read last N lines)
  - search: string? (filter lines containing text)
```

### System Info Resource
```
resource://system/info
  - OS version, build, edition
  - Computer name, domain
  - Hardware summary
  - Uptime
```

## Next Steps
1. Create .NET solution structure
2. Add MCP server NuGet package
3. Implement first tool (eventlog_list_logs)
4. Test with GitHub Copilot or Claude Desktop

## Resources
- [MCP Specification](https://modelcontextprotocol.io/)
- [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [Windows Event Log API](https://learn.microsoft.com/en-us/windows/win32/wes/windows-event-log)
