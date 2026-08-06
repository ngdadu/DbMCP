using DbMCP.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    // stdout is reserved for the MCP stdio protocol, so log to stderr
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

var sqlOptions = await SqlTools.BuildServerOptions();
builder.Services
    //.AddMcpServer()
    .AddMcpServer(options =>
    {
        options.ServerInfo = sqlOptions.ServerInfo;
        // this line make only tools from Db available: options.ToolCollection = sqlOptions.ToolCollection;
        // add all tools from assembly and then add the Db tools to the collection
        options.ToolCollection ??= new ModelContextProtocol.Server.McpServerPrimitiveCollection<ModelContextProtocol.Server.McpServerTool>();
        if (sqlOptions.ToolCollection is not null)
        {
            foreach (var tool in sqlOptions.ToolCollection)
            {
                options.ToolCollection.Add(tool);
            }
        }
    })
    .WithStdioServerTransport()
    .WithToolsFromAssembly()
    .WithRequestFilters(filterBuilder =>
    {
        // https://www.youtube.com/watch?v=qRUjI42zmaM
        filterBuilder.AddListToolsFilter(next
            => async (context, request) =>
        {
            // Custom logic before listing tools
            var result = await next(context, request);
            // Custom logic after listing tools - may be filter out not allowed tools in result.Tools
            return result;
        });
    });

await builder.Build().RunAsync();

