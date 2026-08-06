using System.ComponentModel;
using System.Reflection;
using ModelContextProtocol.Server;

namespace DbMCP.Tools;

[McpServerToolType]
public static class BasicTools
{
    [McpServerTool, Description("Returns a short server health status text.")]
    public static async Task<string> Health()
    { 
        var sql = SqlTools.BuildSqlService(Assembly.GetExecutingAssembly());
        try
        {
            var title = await sql.GetDatabaseTitle();
            return $"{sql.ServerAppName} v{sql.AssemblyVersion} server [{title}] is running.";
        }
        catch (Exception ex)
        {
            return $"{sql.ServerAppName} v{sql.AssemblyVersion} server has encountered an error: {ex.Message}";
        }
    }

    [McpServerTool, Description("Echoes the input text.")]
    public static string Echo(string text)
        => $"Hello: {text}";
}
