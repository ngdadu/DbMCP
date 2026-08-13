using Microsoft.Data.SqlClient;
using ModelContextProtocol.Protocol;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Linq;

namespace DbMCP.Tools;

public class SqlService
{
    public string ConnectionString { get; set; }
    public string InstanceName { get; set; }
    public string Schema { get; set; } = "mcp";
    public string DbPropertyDescription { get; set; } = "Mcp_Description";
    public string DbPropertyTitle { get; set; } = "Mcp_Title";
    public string AssemblyName { get; internal set; } = "";
    public string AssemblyVersion { get; internal set; } = "1.0.0";
    public string ServerAppName => string.IsNullOrEmpty(InstanceName) ? AssemblyName : $"{AssemblyName}.{InstanceName}";

    public SqlService(string connectionString, string? instanceName)
    {
        ConnectionString = connectionString;
        if (string.IsNullOrEmpty(instanceName))
        {
            var cb = new SqlConnectionStringBuilder(ConnectionString);
            InstanceName = cb.InitialCatalog;
        }
        else
        {
            InstanceName = instanceName;
        }
    }
    public async Task<SqlConnection> OpenConnection()
    {
        var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();
        await LogWriter.WriteTraceAsync($"Opened connection to database [{conn.Database}] on server [{conn.DataSource}] for [{InstanceName}]");
        return conn;
    }

    public async Task<string> GetDatabaseDescription()
    {
        var asm = Assembly.GetExecutingAssembly().GetName();
        using (var connection = await OpenConnection())
        {
            string commandText = $"""
                SELECT CAST(value AS NVARCHAR(MAX)) AS Value                
                FROM sys.extended_properties
                WHERE class = 0 AND name LIKE '{DbPropertyDescription}';
            """;
            var result = (await connection.ExecuteScalar(commandText) as string ?? $"Datenbank %DBNAME% auf dem Server %SERVER% (%INSTANCE%): Funkionalitäten aus dem Schema [%SCHEMA%] für %APPNAME% Version %APPVERSION%")
                .Replace("%DBNAME%", connection.Database)
                .Replace("%SERVER%", connection.DataSource)
                .Replace("%APPNAME%", asm.Name)
                .Replace("%APPVERSION%", asm.Version?.ToString() ?? "1.0.0")
                .Replace("%INSTANCE%", InstanceName)
                .Replace("%SCHEMA%", Schema).Trim();
            await LogWriter.WriteTraceAsync($"Database description: {result}");
            return result;
        }
    }
    public async Task<string> GetDatabaseTitle()
    {
        var asm = Assembly.GetExecutingAssembly().GetName();
        using (var connection = await OpenConnection())
        {
            string commandText = $"""
                SELECT CAST(value AS NVARCHAR(MAX)) AS Value                
                FROM sys.extended_properties
                WHERE class = 0 AND name LIKE '{DbPropertyTitle}';
            """;
            var result = (await connection.ExecuteScalar(commandText) as string ?? $"%APPNAME% %INSTANCE%")
                .Replace("%DBNAME%", connection.Database)
                .Replace("%SERVER%", connection.DataSource)
                .Replace("%APPNAME%", asm.Name)
                .Replace("%APPVERSION%", asm.Version?.ToString() ?? "1.0.0")
                .Replace("%INSTANCE%", InstanceName)
                .Replace("%SCHEMA%", Schema).Trim();
            await LogWriter.WriteInfoAsync($"Database title: {result}");
            return result;
        }
    }
    public async Task<IList<SqlDataObject>> GetViews()
    {
        var result = new List<SqlDataObject>();
        using (var connection = await OpenConnection())
        {
            string commandTextViews = """
                    SELECT
                    --SCHEMA_NAME(v.schema_id) AS SchemaName,
                    v.name AS Name,
                    CAST(ep.value AS NVARCHAR(MAX)) AS Description
                    --v.create_date AS CreatedDate,
                    --v.modify_date AS ModifiedDate,
                    --OBJECTPROPERTYEX(v.object_id, 'IsIndexed') AS IsIndexed,
                    --OBJECTPROPERTYEX(v.object_id, 'IsSchemaBound') AS IsSchemaBound
                FROM sys.views v
                LEFT JOIN sys.extended_properties ep 
                    ON ep.major_id = v.object_id 
                    AND ep.minor_id = 0 
                    AND ep.name LIKE 'MS_Description'
                WHERE SCHEMA_NAME(v.schema_id) LIKE @schema
                  AND v.is_ms_shipped = 0
                ORDER BY v.name
            """;
            string commandTextViewColumns = """
                SELECT 
                    c.name AS Name,
                    t.name AS DataType,
                    CASE WHEN t.name LIKE 'n%char' 
                        THEN c.max_length / 2
                        ELSE c.max_length     
                    END          AS MaxLength,
                    c.is_nullable AS IsNullable,
                    CAST(ep.value AS NVARCHAR(MAX)) AS Description
                FROM sys.columns c
                INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                LEFT JOIN sys.extended_properties ep 
                    ON ep.major_id = c.object_id 
                    AND ep.minor_id = c.column_id 
                    AND ep.name LIKE 'MS_Description'
                WHERE c.object_id = OBJECT_ID(@viewName)
                ORDER BY c.column_id
            """;
            using (var reader = await connection.QueryData(commandTextViews, new SqlParameter("@schema", Schema)))
            {
                result = reader.ToObject<SqlDataObject>().ToList();
            }
            foreach (var view in result)
            {
                using (var creader = await connection.QueryData(commandTextViewColumns,
                    new SqlParameter("@viewName", $"[{Schema}].[{view.Name}]")))
                {
                    view.DataOutput = creader.ToObject<SqlDataProperty>().ToList();
                }
            }
        }
        await LogWriter.WriteInfoAsync($"Found {result.Count} views in schema [{Schema}]");
        return result;
    }

    public async Task<IEnumerable<SqlCodeObject>> GetProcedures()
    {
        var result = new List<SqlCodeObject>();
        using (var connection = await OpenConnection())
        {
            string commandTextProcedures = """
                SELECT 
                    --SCHEMA_NAME(v.schema_id) AS SchemaName,
                    v.name AS Name,
                    CAST(ep.value AS NVARCHAR(MAX)) AS Description
                    --v.create_date AS CreatedDate,
                    --v.modify_date AS ModifiedDate,
                    --OBJECTPROPERTYEX(v.object_id, 'IsIndexed') AS IsIndexed,
                    --OBJECTPROPERTYEX(v.object_id, 'IsSchemaBound') AS IsSchemaBound
                FROM sys.procedures v
                LEFT JOIN sys.extended_properties ep 
                    ON ep.major_id = v.object_id 
                    AND ep.minor_id = 0 
                    AND ep.name LIKE 'MS_Description'
                WHERE SCHEMA_NAME(v.schema_id) LIKE @schema
                  AND v.is_ms_shipped = 0
                ORDER BY v.name
            """;
            string commandTextProcedureParameters = """
                SELECT 
                    p.name AS Name,
                    t.name AS DataType,
                    CASE WHEN t.name LIKE 'n%char' 
                        THEN p.max_length / 2
                        ELSE p.max_length     
                    END          AS MaxLength,
                    p.is_output AS IsOutput,
                    CAST(ep.value AS NVARCHAR(MAX)) AS Description
                FROM sys.parameters p
                INNER JOIN sys.types t ON p.user_type_id = t.user_type_id
                LEFT JOIN sys.extended_properties ep 
                    ON ep.major_id = p.object_id 
                    AND ep.minor_id = p.parameter_id 
                    AND ep.name = 'MS_Description'
                WHERE p.object_id = OBJECT_ID(@procedureName)
                ORDER BY p.parameter_id
            """;
            string commandTextProcedureColumns = """
                SELECT 
                    c.name AS Name,
                    t.name AS DataType,
                    CASE WHEN t.name LIKE 'n%char' 
                        THEN c.max_length / 2
                        ELSE c.max_length     
                    END          AS MaxLength,
                    c.is_nullable AS IsNullable,
                    CAST(ep.value AS NVARCHAR(MAX)) AS Description
                FROM sys.columns c
                INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                LEFT JOIN sys.extended_properties ep 
                    ON ep.major_id = c.object_id 
                    AND ep.minor_id = c.column_id 
                    AND ep.name = 'MS_Description'
                WHERE c.object_id = OBJECT_ID(@procedureName)
                ORDER BY c.column_id
            """;
            using (var reader = await connection.QueryData(commandTextProcedures, new SqlParameter("@schema", Schema)))
            {
                result = reader.ToObject<SqlCodeObject>().ToList();
            }
            foreach (var procedure in result)
            {
                using (var creader = await connection.QueryData(commandTextProcedureParameters,
                    new SqlParameter("@procedureName", $"[{Schema}].[{procedure.Name}]")))
                {
                    procedure.Parameters = creader.ToObject<SqlDataProperty>().ToList();
                }
                using (var creader = await connection.QueryData(commandTextProcedureColumns,
                    new SqlParameter("@procedureName", $"[{Schema}].[{procedure.Name}]")))
                {
                    procedure.DataOutput = creader.ToObject<SqlDataProperty>().ToList();
                }
            }
        }
        await LogWriter.WriteInfoAsync($"Found {result.Count} procedures in schema [{Schema}]");
        return result;
    }

    public async Task<IList<SqlCodeObject>> GetFunctions()
    {
        var result = new List<SqlCodeObject>();
        using (var connection = await OpenConnection())
        {
            string commandTextFunctions = """
                SELECT
                    o.name AS Name,
                    CAST(ep.value AS NVARCHAR(MAX)) AS Description
                FROM sys.objects o
                LEFT JOIN sys.extended_properties ep
                    ON ep.major_id = o.object_id
                    AND ep.minor_id = 0
                    AND ep.name LIKE 'MS_Description'
                WHERE SCHEMA_NAME(o.schema_id) LIKE @schema
                  AND o.is_ms_shipped = 0
                  AND o.type IN ('FN', 'IF', 'TF', 'FS', 'FT')
                ORDER BY o.name
            """;
            string commandTextFunctionParameters = """
                SELECT 
                    p.name AS Name,
                    t.name AS DataType,
                    CASE WHEN t.name LIKE 'n%char' 
                        THEN p.max_length / 2
                        ELSE p.max_length     
                    END          AS MaxLength,
                    p.is_output AS IsOutput,
                    CAST(ep.value AS NVARCHAR(MAX)) AS Description
                FROM sys.parameters p
                INNER JOIN sys.types t ON p.user_type_id = t.user_type_id
                LEFT JOIN sys.extended_properties ep 
                    ON ep.major_id = p.object_id 
                    AND ep.minor_id = p.parameter_id 
                    AND ep.name = 'MS_Description'
                WHERE p.object_id = OBJECT_ID(@functionName)
                ORDER BY p.parameter_id
            """;
            string commandTextFunctionColumns = """
                SELECT 
                    c.name AS Name,
                    t.name AS DataType,
                    CASE WHEN t.name LIKE 'n%char' 
                        THEN c.max_length / 2
                        ELSE c.max_length     
                    END          AS MaxLength,
                    c.is_nullable AS IsNullable,
                    CAST(ep.value AS NVARCHAR(MAX)) AS Description
                FROM sys.columns c
                INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                LEFT JOIN sys.extended_properties ep 
                    ON ep.major_id = c.object_id 
                    AND ep.minor_id = c.column_id 
                    AND ep.name = 'MS_Description'
                WHERE c.object_id = OBJECT_ID(@functionName)
                ORDER BY c.column_id
            """;
            using (var reader = await connection.QueryData(commandTextFunctions, new SqlParameter("@schema", Schema)))
            {
                result = reader.ToObject<SqlCodeObject>().ToList();
            }
            foreach (var function in result)
            {
                using (var creader = await connection.QueryData(commandTextFunctionParameters,
                    new SqlParameter("@functionName", $"[{Schema}].[{function.Name}]")))
                {
                    function.Parameters = creader.ToObject<SqlDataProperty>().ToList();
                }
                using (var creader = await connection.QueryData(commandTextFunctionColumns,
                    new SqlParameter("@functionName", $"[{Schema}].[{function.Name}]")))
                {
                    function.DataOutput = creader.ToObject<SqlDataProperty>().ToList();
                }
            }
        }
        await LogWriter.WriteInfoAsync($"Found {result.Count} functions in schema [{Schema}]");
        return result;
    }

    public async Task<string> ExecuteView(SqlDataObject view, string? parametersJson)
    {
        var condition = ParseParameters(parametersJson);
        return await ExecuteView(view, condition);
    }

    public const string VIEW_ALIAS = "vw";
    public static readonly List<string> SQL_KEYWORDS_AFTER_WHERE = new List<string>
    {
        "WHERE", "GROUP", "ORDER", "INNER", "LEFT", "RIGHT", "FULL", "NOT", "EXISTS", 
        "APPLY", "UNION", "INTERSECT", "EXCEPT", "CROSS", "OUTER", "JOIN"
    };
    public async Task<string> ExecuteView(SqlDataObject view, Dictionary<string, object?>? arguments)
    {
        var topLimit = 0;
        if (arguments is not null && arguments.TryGetValue("Top", out var topValue) && topValue is not null)
        {
            topLimit = Convert.ToInt32(topValue);
        }
        var whereClause = arguments is not null && arguments.TryGetValue("Where", out var whereValue) ? $"{whereValue}" : null;

        using var connection = await OpenConnection();
        var columns = view.DataOutput.Select(p => $"{VIEW_ALIAS}.[{p.Name}]").ToList();
        var topClause = topLimit > 0 ? $"TOP ({topLimit})" : string.Empty;
        var sql = $"SELECT {topClause} {string.Join(", ", columns)} FROM [{Schema}].[{view.Name}] AS {VIEW_ALIAS}";
        if (!string.IsNullOrWhiteSpace(whereClause))
        {
            var prefix = SQL_KEYWORDS_AFTER_WHERE.Any(keyword => whereClause.TrimStart().StartsWith(keyword, StringComparison.OrdinalIgnoreCase))
                ? "" : "WHERE ";
            sql += $" {prefix}{whereClause}";
        }
        using var rdr = await connection.QueryData(sql);
        return rdr.ToJson();
    }

    public async Task<string> ExecuteProcedure(SqlCodeObject procedure, string? parametersJson)
    {
        var named = ParseParameters(parametersJson);
        return await ExecuteProcedure(procedure, named);
    }

    public async Task<string> ExecuteProcedure(SqlCodeObject procedure, Dictionary<string, object?>? arguments)
    {
        var named = NormalizeArguments(arguments);
        var sqlParams = BuildSqlParameters(procedure.Parameters, named);
        var assignments = sqlParams.Length == 0
            ? string.Empty
            : " " + string.Join(", ", sqlParams.Select(p => $"{p.ParameterName} = {p.ParameterName}"));
        var sql = $"EXEC [{Schema}].[{procedure.Name}]{assignments}";

        using var connection = await OpenConnection();
        if (procedure.DataOutput.Count != 0)
        {
            using var rdr = await connection.QueryData(sql, sqlParams);
            return rdr.ToJson();
        }
        else
        {
            var result = await connection.ExecuteNonQuery(sql, sqlParams);
            return result.ToString();
        }
    }

    public async Task<string> ExecuteFunction(SqlCodeObject function, string? parametersJson)
    {
        var named = ParseParameters(parametersJson);
        return await ExecuteFunction(function, named);
    }

    public async Task<string> ExecuteFunction(SqlCodeObject function, Dictionary<string, object?>? arguments)
    {
        var named = NormalizeArguments(arguments);
        var sqlParams = BuildSqlParameters(function.Parameters, named);
        var args = string.Join(", ", sqlParams.Select(p => p.ParameterName));

        var sql = function.DataOutput.Count > 0
            ? $"SELECT * FROM [{Schema}].[{function.Name}]({args})"
            : $"SELECT [{Schema}].[{function.Name}]({args}) AS Result";

        using var connection = await OpenConnection();
        using var rdr = await connection.QueryData(sql, sqlParams);
        return rdr.ToJson();
    }

    private static Dictionary<string, object?> ParseParameters(string? parametersJson)
    {
        Console.Error.WriteLine($"Parsing parameter: {parametersJson}");
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(parametersJson))
        {
            return result;
        }

        var obj = JObject.Parse(parametersJson);
        foreach (var p in obj.Properties())
        {
            result[p.Name.TrimStart('@')] = p.Value.Type == JTokenType.Null
                ? null
                : ((JValue)p.Value).Value;
        }

        return result;
    }

    private static Dictionary<string, object?> NormalizeArguments(Dictionary<string, object?>? arguments)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        if (arguments is null)
        {
            return result;
        }

        foreach (var (key, value) in arguments)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            result[key.TrimStart('@')] = ConvertJsonElement(value);
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
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number when element.TryGetInt64(out var l) => l,
            JsonValueKind.Number when element.TryGetDecimal(out var d) => d,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => element.ToString()
        };
    }

    private static SqlParameter[] BuildSqlParameters(
        IEnumerable<SqlDataProperty> definedParameters,
        Dictionary<string, object?> paramsValues)
    {
        var list = new List<SqlParameter>();
        foreach (var (p, key) in from p in definedParameters
                                 let key = p.Name.TrimStart('@')
                                 select (p, key))
        {
            if (!paramsValues.TryGetValue(key, out var val))
            {
                continue;
            }

            list.Add(new SqlParameter(
                p.Name.StartsWith('@') ? p.Name : $"@{p.Name}",
                val ?? DBNull.Value));
        }

        // allow extra user-provided params (not present in metadata)
        foreach (var kvp in paramsValues)
        {
            if (list.Any(x => string.Equals(x.ParameterName.TrimStart('@'), kvp.Key, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            list.Add(new SqlParameter($"@{kvp.Key}", kvp.Value ?? DBNull.Value));
        }

        return list.ToArray();
    }
}
