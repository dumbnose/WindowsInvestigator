<#
.SYNOPSIS
    Installs WindowsInvestigator MCP Server for use with GitHub Copilot CLI.

.DESCRIPTION
    This script builds the WindowsInvestigator MCP Server and registers it with GitHub Copilot CLI
    by updating the MCP configuration file at ~/.github-copilot/mcp.json.

.PARAMETER BuildConfiguration
    The build configuration to use (Debug or Release). Default is Release.

.PARAMETER InstallPath
    The installation path for the built executable. Default is the Release output directory.

.PARAMETER SkipBuild
    Skip the build step and use an existing executable.

.PARAMETER Uninstall
    Remove the WindowsInvestigator registration from MCP configuration.

.EXAMPLE
    .\Install-WindowsInvestigator.ps1
    Builds and installs WindowsInvestigator with default settings.

.EXAMPLE
    .\Install-WindowsInvestigator.ps1 -BuildConfiguration Debug
    Builds and installs using Debug configuration.

.EXAMPLE
    .\Install-WindowsInvestigator.ps1 -Uninstall
    Removes WindowsInvestigator from MCP configuration.
#>

[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$BuildConfiguration = "Release",
    
    [string]$InstallPath,
    
    [switch]$SkipBuild,
    
    [switch]$Uninstall
)

$ErrorActionPreference = "Stop"

# Determine paths
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir
$ProjectDir = Join-Path $RepoRoot "src\WindowsInvestigator.McpServer"
$ProjectFile = Join-Path $ProjectDir "WindowsInvestigator.McpServer.csproj"
$McpConfigPath = Join-Path $env:USERPROFILE ".copilot\mcp-config.json"
$McpConfigDir = Split-Path -Parent $McpConfigPath

# Default install path is the build output
if (-not $InstallPath) {
    $InstallPath = Join-Path $ProjectDir "bin\$BuildConfiguration\net8.0-windows"
}

$ExecutablePath = Join-Path $InstallPath "WindowsInvestigator.McpServer.exe"

function Write-Step {
    param([string]$Message)
    Write-Host "`n>> $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "   $Message" -ForegroundColor Green
}

function Write-WarningMessage {
    param([string]$Message)
    Write-Host "   $Message" -ForegroundColor Yellow
}

function Build-Project {
    Write-Step "Building WindowsInvestigator MCP Server..."
    
    if (-not (Test-Path $ProjectFile)) {
        throw "Project file not found: $ProjectFile"
    }
    
    # Restore and build
    $buildResult = & dotnet build $ProjectFile -c $BuildConfiguration --nologo -v q
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed with exit code $LASTEXITCODE"
    }
    
    if (-not (Test-Path $ExecutablePath)) {
        throw "Build succeeded but executable not found: $ExecutablePath"
    }
    
    Write-Success "Build completed successfully"
    Write-Success "Executable: $ExecutablePath"
}

function Get-McpConfig {
    if (Test-Path $McpConfigPath) {
        $content = Get-Content $McpConfigPath -Raw
        return $content | ConvertFrom-Json -AsHashtable
    }
    return @{ mcpServers = @{} }
}

function Save-McpConfig {
    param([hashtable]$Config)
    
    # Ensure directory exists
    if (-not (Test-Path $McpConfigDir)) {
        New-Item -ItemType Directory -Path $McpConfigDir -Force | Out-Null
    }
    
    $json = $Config | ConvertTo-Json -Depth 10
    Set-Content -Path $McpConfigPath -Value $json -Encoding UTF8
}

function Register-McpServer {
    Write-Step "Registering WindowsInvestigator with GitHub Copilot CLI..."
    
    $config = Get-McpConfig
    
    # Add or update WindowsInvestigator entry
    $config.mcpServers["WindowsInvestigator"] = @{
        command = $ExecutablePath
        args = @()
    }
    
    Save-McpConfig $config
    
    Write-Success "Registered WindowsInvestigator in: $McpConfigPath"
    Write-Success "Command: $ExecutablePath"
}

function Unregister-McpServer {
    Write-Step "Unregistering WindowsInvestigator from GitHub Copilot CLI..."
    
    if (-not (Test-Path $McpConfigPath)) {
        Write-WarningMessage "MCP configuration file not found: $McpConfigPath"
        return
    }
    
    $config = Get-McpConfig
    
    if ($config.mcpServers.ContainsKey("WindowsInvestigator")) {
        $config.mcpServers.Remove("WindowsInvestigator")
        Save-McpConfig $config
        Write-Success "WindowsInvestigator has been unregistered"
    }
    else {
        Write-WarningMessage "WindowsInvestigator was not registered"
    }
}

function Show-Summary {
    Write-Step "Installation Complete!"
    
    Write-Host @"

WindowsInvestigator MCP Server is now installed and registered.

To use it with GitHub Copilot CLI:
1. Open a terminal
2. Run: gh copilot
3. Ask questions like:
   - "What errors are in the System event log?"
   - "What services are stopped?"
   - "Show me the system information"
   - "What scheduled tasks failed recently?"

Configuration file: $McpConfigPath

To uninstall:
  .\Install-WindowsInvestigator.ps1 -Uninstall

"@ -ForegroundColor White
}

# Main execution
try {
    Write-Host "`nWindowsInvestigator MCP Server Installer" -ForegroundColor Magenta
    Write-Host "========================================" -ForegroundColor Magenta
    
    if ($Uninstall) {
        Unregister-McpServer
        Write-Host "`nUninstall complete." -ForegroundColor Green
        exit 0
    }
    
    if (-not $SkipBuild) {
        Build-Project
    }
    else {
        Write-Step "Skipping build (using existing executable)"
        if (-not (Test-Path $ExecutablePath)) {
            throw "Executable not found: $ExecutablePath. Run without -SkipBuild to build first."
        }
        Write-Success "Found executable: $ExecutablePath"
    }
    
    Register-McpServer
    Show-Summary
}
catch {
    Write-Host "`nError: $_" -ForegroundColor Red
    exit 1
}
