# Copilot Instructions for WindowsInvestigator

## MCP Server Usage

When investigating Windows issues in this repository, **always use the WindowsInvestigator MCP server tools** instead of native PowerShell cmdlets for data collection.

### Use MCP Tools Instead Of:

| Instead of... | Use MCP Tool |
|---------------|--------------|
| `Get-WinEvent` | `QueryEventLog` or `ListEventLogs` |
| Direct Event Log API calls | `QueryEventLog` |

### Available MCP Tools

1. **ListEventLogs** - Lists all available Windows Event Log names
2. **QueryEventLog** - Queries events from a Windows Event Log
   - Parameters:
     - `logName` (required): Name of the event log (e.g., System, Application, Security)
     - `level` (optional): Filter by level (Critical, Error, Warning, Information, Verbose)
     - `source` (optional): Filter by event source
     - `maxResults` (optional): Maximum number of events to return (default: 50)

### Why Use MCP Tools?

1. **Dogfooding** - We're building these tools, so we should use them
2. **Testing** - Using the tools helps us find bugs and missing features
3. **Consistency** - Demonstrates the intended usage patterns
4. **Feature Discovery** - Helps identify gaps in functionality that need to be added

### Adding New Features

If you encounter a scenario where the MCP server doesn't have the needed functionality:
1. Note the gap
2. Suggest adding the feature to the implementation plan
3. Consider implementing it as part of the current session
