using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Reflection;

namespace DbMCP.Tools;

public static class SqlHelper
{
    public static async Task<SqlDataReader> QueryData(this SqlConnection connection, string commandText, params SqlParameter[] parameters)
    {
        SqlCommand command = new SqlCommand(commandText, connection);
        command.CommandTimeout = 0;
        if (parameters != null)
        {
            command.Parameters.AddRange(parameters);
        }
        await Console.Error.WriteLineAsync($"Executing SQL Query: `{commandText}` with {parameters?.Length ?? 0} parameters: {string.Join(", ", parameters?.Select(p => $"{p.ParameterName}={p.Value}") ?? new List<string>())}");
        return await command.ExecuteReaderAsync();
    }

    public static async Task<int> ExecuteNonQuery(this SqlConnection connection, string commandText, params SqlParameter[] parameters)
    {
        SqlCommand command = new SqlCommand(commandText, connection);
        command.CommandTimeout = 0;
        if (parameters != null)
        {
            command.Parameters.AddRange(parameters);
        }
        await Console.Error.WriteLineAsync($"Executing SQL NonQuery: `{commandText}` with {parameters?.Length ?? 0} parameters: {string.Join(", ", parameters?.Select(p => $"{p.ParameterName}={p.Value}") ?? new List<string>())}");
        return await command.ExecuteNonQueryAsync();
    }

    public static async Task<object?> ExecuteScalar(this SqlConnection connection, string commandText, params SqlParameter[] parameters)
    {
        SqlCommand command = new SqlCommand(commandText, connection);
        command.CommandTimeout = 0;
        if (parameters != null)
        {
            command.Parameters.AddRange(parameters);
        }
        await Console.Error.WriteLineAsync($"Executing SQL Scalar: `{commandText}` with {parameters?.Length ?? 0} parameters: {string.Join(", ", parameters?.Select(p => $"{p.ParameterName}={p.Value}") ?? new List<string>())}");
        return await command.ExecuteScalarAsync();
    }

    public static string ToJson(this SqlDataReader rdr, bool noObject = false, Func<string, string>? onMapColumn = null)
    {
        StringBuilder sb = new StringBuilder();
        StringWriter sw = new StringWriter(sb);

        using (JsonWriter jsonWriter = new JsonTextWriter(sw))
        {
            jsonWriter.WriteStartArray();

            string[]? fieldNames = null;
            while (rdr.Read())
            {
                if (fieldNames == null)
                {
                    if (rdr.FieldCount == 0)
                    {
                        return "null";
                    }
                    if (noObject && rdr.FieldCount > 1) noObject = false;
                    fieldNames = new string[rdr.FieldCount];
                    for (int i = 0; i < rdr.FieldCount; i++)
                    {
                        fieldNames[i] = onMapColumn?.Invoke(rdr.GetName(i)) ?? rdr.GetName(i);
                    }
                }
                if (!noObject) jsonWriter.WriteStartObject();

                for (int i = 0; i < fieldNames.Length; i++)
                {
                    if (rdr[i] == null || DBNull.Value.Equals(rdr[i]))
                    {
                        continue;
                    }
                    if (!noObject) jsonWriter.WritePropertyName(fieldNames[i]);
                    jsonWriter.WriteValue(rdr[i]);
                }
                if (!noObject) jsonWriter.WriteEndObject();
            }

            jsonWriter.WriteEndArray();
            sw.Flush();
            return sb.ToString();
        }
    }

    public static IEnumerable<T> ToObject<T>(this SqlDataReader rdr, Action<T>? onDataCompleted = null) where T : class, new()
    {
        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty).ToList();
        string[]? fieldNames = null;
        while (rdr.Read())
        {
            if (fieldNames == null)
            {
                if (rdr.FieldCount == 0)
                {
                    yield break;
                }
                fieldNames = new string[rdr.FieldCount];
                for (int i = 0; i < rdr.FieldCount; i++)
                {
                    fieldNames[i] = rdr.GetName(i);
                }
                properties = properties.Where(p => fieldNames.Contains(p.Name, StringComparer.OrdinalIgnoreCase)).ToList();
            }
            var data = new T();
            for (int i = 0; i < fieldNames.Length; i++)
            {
                var prop = properties.FirstOrDefault(p => string.Equals(p.Name, fieldNames[i], StringComparison.OrdinalIgnoreCase));
                if (prop == null || rdr[i] == null || DBNull.Value.Equals(rdr[i]))
                {
                    continue;
                }
                prop.SetValue(data, Convert.ChangeType(rdr[i], prop.PropertyType));
            }
            onDataCompleted?.Invoke(data);
            yield return data;
        }
    }
}