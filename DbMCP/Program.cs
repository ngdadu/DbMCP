using DbMCP.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

var useHttpTransport = bool.TryParse(Environment.GetEnvironmentVariable("DBMCP_HTTP"), out var useHttp) && useHttp;
var sqlOptions = await SqlTools.BuildServerOptions();

if (useHttpTransport)
{
    var builder = WebApplication.CreateBuilder(args);
    ConfigureLogging(builder.Logging, LogLevel.Information);
    ConfigureMcpServer(builder.Services, sqlOptions).WithHttpTransport();

    var app = builder.Build();
    app.MapMcp("/mcp");
    app.Lifetime.ApplicationStarted.Register(() =>
        app.Logger.LogInformation("DbMCP HTTP endpoint listening at {McpEndpoints}",
            string.Join(", ", app.Urls.Select(address => $"{address.TrimEnd('/')}/mcp"))));
    await app.RunAsync();
}
else
{
    var builder = Host.CreateApplicationBuilder(args);
    ConfigureLogging(builder.Logging, LogLevel.Trace);
    ConfigureMcpServer(builder.Services, sqlOptions).WithStdioServerTransport();

    await builder.Build().RunAsync();
}

static void ConfigureLogging(ILoggingBuilder logging, LogLevel level)
{
    logging.ClearProviders();
    logging.AddConsole(options =>
    {
        options.LogToStandardErrorThreshold = level;
    });
    LogWriter.MaxLogLevel = level;
}

static IMcpServerBuilder ConfigureMcpServer(IServiceCollection services, McpServerOptions sqlOptions)
{
    return services
        .AddMcpServer(options =>
        {
            options.ServerInfo = sqlOptions.ServerInfo;
            options.ToolCollection ??= new McpServerPrimitiveCollection<McpServerTool>();
            if (sqlOptions.ToolCollection is not null)
            {
                foreach (var tool in sqlOptions.ToolCollection)
                {
                    options.ToolCollection.Add(tool);
                }
            }
        }
        )
        .WithToolsFromAssembly()
        .WithRequestFilters(filterBuilder =>
        {
            filterBuilder.AddListToolsFilter(next
                => async (context, request) =>
            {
                var result = await next(context, request);
                return result;
            });
        });
}

