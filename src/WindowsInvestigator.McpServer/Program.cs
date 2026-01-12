using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol;
using WindowsInvestigator.McpServer.Services;
using WindowsInvestigator.McpServer.Tools;

var builder = Host.CreateApplicationBuilder(args);

// Register services
builder.Services.AddSingleton<IEventLogService, WindowsEventLogService>();
builder.Services.AddSingleton<ISystemInfoService, WindowsSystemInfoService>();
builder.Services.AddSingleton<IServiceInfoService, WindowsServiceInfoService>();
builder.Services.AddSingleton<INetworkService, WindowsNetworkService>();
builder.Services.AddSingleton<IPrintService, WindowsPrintService>();
builder.Services.AddSingleton<IFileLogService, WindowsFileLogService>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<EventLogTools>()
    .WithTools<SystemInfoTools>()
    .WithTools<ServiceTools>()
    .WithTools<NetworkTools>()
    .WithTools<PrintTools>()
    .WithTools<FileLogTools>();

var app = builder.Build();
await app.RunAsync();
