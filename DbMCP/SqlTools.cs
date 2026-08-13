using Microsoft.Data.SqlClient;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace DbMCP.Tools;

public static class SqlTools
{
    public static async Task<McpServerOptions> BuildServerOptions()
    {
        var asm = Assembly.GetExecutingAssembly();
        var sqlService = BuildSqlService(asm);

        List<Icon>? iconsList = await BuildIconsList(asm.Location, sqlService.InstanceName);
        var title = await sqlService.GetDatabaseTitle();
        var description = await sqlService.GetDatabaseDescription();
        var options = new McpServerOptions
        {
            ServerInfo = new Implementation
            {
                Name = sqlService.ServerAppName,
                Version = sqlService.AssemblyVersion,
                Title = title,
                Description = description,
                Icons = iconsList
            },
            ToolCollection = await RetrieveMcpToolsInDb(sqlService)
        };
        return options;
    }

    private static async Task<List<Icon>?> BuildIconsList(string asmLocation, string instanceName)
    {
        var asmPath = Path.GetDirectoryName(asmLocation) ?? Environment.CurrentDirectory;
        var appName = Path.GetFileNameWithoutExtension(asmLocation);
        await LogWriter.WriteTraceAsync($"searching for icons: {asmPath}\\{appName}-{instanceName}.*.ico");

        var icoFiles = string.IsNullOrEmpty(instanceName)
            ? new List<string>()
            : Directory.GetFiles(asmPath, $"{appName}-{instanceName}.*.ico")
                .Union(Directory.GetFiles(asmPath, $"{appName}-{instanceName}.ico"))
                .ToList();
        if (icoFiles.Count == 0)
        {
            await LogWriter.WriteTraceAsync($"searching for icons: {asmPath}\\{appName}.*.ico");
            icoFiles = Directory.GetFiles(asmPath, $"{appName}.*.ico")
                .Union(Directory.GetFiles(asmPath, $"{appName}.ico"))
                .ToList();
        }
        var iconsList = icoFiles.Count == 0 ? null
            : icoFiles.Distinct()
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .Select(f => new Icon
            {
                Source = new Uri(f, UriKind.Absolute).AbsoluteUri,
                MimeType = "image/x-icon"
            }).ToList();
        await LogWriter.WriteDebugAsync($"{iconsList?.Count ?? 0} icons found: {string.Join(", ", iconsList?.Select(f => f.Source) ?? Array.Empty<string>())}");
        return iconsList;
    }

    public static SqlService BuildSqlService(Assembly asm)
    {
        var asmName = asm.GetName();
        var connectionString = Environment.GetEnvironmentVariable("DBMCP_CONNECTION_STR");
        var instanceName = Environment.GetEnvironmentVariable("DBMCP_INSTANCE_NAME");
        if (string.IsNullOrWhiteSpace(connectionString)) throw new InvalidProgramException("DBMCP_CONNECTION_STR environment variable is not set.");

        var sqlService = new SqlService(connectionString, instanceName)
        {
            AssemblyName = asmName.Name ?? "DbMCP",
            AssemblyVersion = asmName.Version?.ToString() ?? "1.0.0",
            Schema = Environment.GetEnvironmentVariable("DBMCP_SCHEMA") ?? "mcp"
        };
        return sqlService;
    }

    private static async Task<McpServerPrimitiveCollection<McpServerTool>> RetrieveMcpToolsInDb(SqlService sqlService)
    {
        var whereTips = string.Join(", ", SqlService.SQL_KEYWORDS_AFTER_WHERE
            .Select(k => $"`{k}`"));
        var tools = new McpServerPrimitiveCollection<McpServerTool>();

        foreach (var view in await sqlService.GetViews())
        {
            var whereParameter = new List<SqlDataProperty>
            {
                new()
                {
                     Name = "Top",
                     DataType = "int",                      
                     Description = "SQL TOP clause to limit the number of results"
                },
                new() 
                {
                     Name = "Where",
                     DataType = "nvarchar",
                     MaxLength = -1,
                     Description = $"""
                        SQL WHERE clause to filter the results
                            - The view will be aliased as `{SqlService.VIEW_ALIAS}` in the SQL query. Using the view columns in the WHERE clause should be prefixed with `{SqlService.VIEW_ALIAS}.` (e.g. `{SqlService.VIEW_ALIAS}.[ColumnName] = 123`).
                            - If the clause starts with a SQL keyword (e.g. {whereTips}), 
                              the keyword will be used as-is without prepending a `WHERE` keyword.
                            - If the clause does not start with a SQL keyword, a `WHERE` keyword will be prepended to the clause automatically.
                        """
                }
            };
            var viewTool = McpServerTool.Create(
                async (AIFunctionArguments arguments) => await sqlService.ExecuteView(view, ToArgs(arguments)),
                new McpServerToolCreateOptions
                {
                    Name = BuildToolName("view", view.Name),
                    UseStructuredContent = true,
                    OutputSchema = view.BuildOutputSchema(),
                    Description = BuildMarkdownDescription(
                        view.Description ?? $"Execute view [{sqlService.Schema}].[{view.Name}]",
                        whereParameter,
                        view.DataOutput)
                });
            viewTool.ProtocolTool.InputSchema = SqlDataObject.BuildMcpSchema(whereParameter) ?? new System.Text.Json.JsonElement();
            tools.Add(viewTool);
        }

        foreach (var proc in await sqlService.GetProcedures())
        {
            var output = proc.DataOutput ?? new List<SqlDataProperty>();
            if (output.Count == 0)
            {
                output.Add(new SqlDataProperty
                {
                    Name = "",
                    DataType = "int",
                    NotNullable = true,
                    Description = "Return value of `ExecuteNonQuery` procedure"
                });
            }
            var procTool = McpServerTool.Create(
                async (AIFunctionArguments arguments) => await sqlService.ExecuteProcedure(proc, ToArgs(arguments)),
                new McpServerToolCreateOptions
                {
                    Name = BuildToolName("proc", proc.Name),
                    UseStructuredContent = (proc.DataOutput?.Count ?? 0) != 0,
                    OutputSchema = proc.BuildOutputSchema(),
                    Description = BuildMarkdownDescription(
                        proc.Description ?? $"Execute procedure [{sqlService.Schema}].[{proc.Name}]",
                        proc.Parameters,
                        output)
                });

            var procInputSchema = proc.BuildInputSchema();
            if (procInputSchema is not null)
            {
                procTool.ProtocolTool.InputSchema = procInputSchema.Value;
            }

            tools.Add(procTool);
        }

        foreach (var func in await sqlService.GetFunctions())
        {
            var output = func.DataOutput ?? new List<SqlDataProperty>();
            if (output.Count == 0)
            {
                output.Add(new SqlDataProperty
                {
                    Name = "",
                    DataType = "object",
                    Description = "Return value of `SELECT` function"
                });
            }
            var funcTool = McpServerTool.Create(
                async (AIFunctionArguments arguments) => await sqlService.ExecuteFunction(func, ToArgs(arguments)),
                new McpServerToolCreateOptions
                {
                    Name = BuildToolName("func", func.Name),
                    UseStructuredContent = (func.DataOutput?.Count ?? 0) != 0,
                    OutputSchema = func.BuildOutputSchema(),
                    Description = BuildMarkdownDescription(
                        func.Description ?? $"Execute function [{sqlService.Schema}].[{func.Name}]",
                        func.Parameters,
                        output)
                });

            var funcInputSchema = func.BuildInputSchema();
            if (funcInputSchema is not null)
            {
                funcTool.ProtocolTool.InputSchema = funcInputSchema.Value;
            }

            tools.Add(funcTool);
        }

        return tools;
    }

    private static string BuildMarkdownDescription(
        string description,
        IEnumerable<SqlDataProperty>? parameters,
        IEnumerable<SqlDataProperty>? output)
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(description))
        {
            sb.AppendLine(description);
            sb.AppendLine();
        }

        sb.AppendLine("### Parameters");

        var items = parameters?.ToList() ?? new List<SqlDataProperty>();
        if (items.Count > 0)
        {
            BuildDescriptionItems(sb, items, "Supported SQL parameters:", "required", "optional");
        }

        sb.AppendLine();
        sb.AppendLine("### Output");

        var outputItems = output?.ToList() ?? new List<SqlDataProperty>();
        if (outputItems.Count > 0)
        {
            BuildDescriptionItems(sb, outputItems, "Result columns:", "not null", "nullable");
        }
        else
        {
            sb.AppendLine("- `int` as result of `EXEC` sql.");
        }

        return sb.ToString().Trim();
    }

    private static void BuildDescriptionItems(StringBuilder sb, List<SqlDataProperty> items, string title, string notNullText, string nullText)
    {
        sb.AppendLine(title);
        foreach (var p in items)
        {
            var nullable = p.NotNullable ? notNullText : nullText;
            var descr = string.IsNullOrWhiteSpace(p.Description) ? "" : $": {p.Description}";
            var maxLength = p.MaxLength switch
            {
                > 0 => $"({p.MaxLength})",
                -1 when p.DataType.EndsWith("char", StringComparison.OrdinalIgnoreCase) => "(MAX)",
                -1 when p.DataType.EndsWith("binary", StringComparison.OrdinalIgnoreCase) => "(MAX)",
                _ => ""
            };
            sb.AppendLine($"  - `{p.Name.Trim().TrimStart('@')}` (`{p.DataType}{maxLength}`, {nullable}){descr}");
        }
    }

    // Copies all root request fields into a plain dictionary the SQL layer understands.
    private static Dictionary<string, object?> ToArgs(AIFunctionArguments? arguments)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        if (arguments is null)
        {
            return result;
        }

        foreach (var kvp in arguments)
        {
            result[kvp.Key] = ConvertJsonElement(kvp.Value);
        }

        return result;
    }

    private static object? ConvertJsonElement(object? value)
    {
        if (value is not JsonElement element)
        {
            return value;
        }

        return element.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.Undefined => null,
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number when element.TryGetInt64(out var l) => l,
            JsonValueKind.Number when element.TryGetDecimal(out var d) => d,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => element.ToString()
        };
    }

    private static string BuildToolName(string prefix, string dbObjectName)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(prefix))
        {
            if (!prefix.EndsWith('_')) prefix = $"{prefix}_";
            if (dbObjectName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) prefix = string.Empty;
        }

        foreach (var ch in dbObjectName)
        {
            sb.Append(char.IsLetterOrDigit(ch) ? char.ToLowerInvariant(ch) : '_');
        }

        var normalized = sb.ToString().Trim('_');
        return string.IsNullOrWhiteSpace(normalized) ? prefix : $"{prefix}{normalized}";
    }
}
