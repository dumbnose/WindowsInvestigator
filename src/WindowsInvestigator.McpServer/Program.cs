using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using WindowsInvestigator.McpServer.Services;
using WindowsInvestigator.McpServer.Tools;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add file logging if a log path is specified
var logPath = Environment.GetEnvironmentVariable("WINDOWSINVESTIGATOR_LOGPATH");
if (!string.IsNullOrEmpty(logPath))
{
    builder.Logging.AddSimpleConsole(options =>
    {
        options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
    });
}

// Register services
builder.Services.AddSingleton<IEventLogService, WindowsEventLogService>();
builder.Services.AddSingleton<ISystemInfoService, WindowsSystemInfoService>();
builder.Services.AddSingleton<IServiceInfoService, WindowsServiceInfoService>();
builder.Services.AddSingleton<INetworkService, WindowsNetworkService>();
builder.Services.AddSingleton<IPrintService, WindowsPrintService>();
builder.Services.AddSingleton<IFileLogService, WindowsFileLogService>();
builder.Services.AddSingleton<IRegistryService, WindowsRegistryService>();
builder.Services.AddSingleton<IProcessService, WindowsProcessService>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<EventLogTools>()
    .WithTools<SystemInfoTools>()
    .WithTools<ServiceTools>()
    .WithTools<NetworkTools>()
    .WithTools<PrintTools>()
    .WithTools<FileLogTools>()
    .WithTools<RegistryTools>()
    .WithTools<ProcessTools>();

var app = builder.Build();
await app.RunAsync();
