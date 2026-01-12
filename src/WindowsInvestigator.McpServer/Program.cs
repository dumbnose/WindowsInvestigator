using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol;
using WindowsInvestigator.McpServer.Services;
using WindowsInvestigator.McpServer.Tools;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IEventLogService, WindowsEventLogService>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<EventLogTools>();

var app = builder.Build();
await app.RunAsync();
