using System.Text.Json.Serialization;

namespace DbMCP.Tools
{
    public class McpJsonSchema202012
    {
        public McpJsonSchema202012(List<SqlDataProperty> sqlProperties)
        {
            foreach (var sqlProperty in sqlProperties)
            {
                var name = sqlProperty.Name.Trim().TrimStart('@');
                if (string.IsNullOrWhiteSpace(name) || Properties.ContainsKey(name))
                {
                    continue;
                }

                Properties[name] = McpJsonSchemaProperty.FromSqlProperty(sqlProperty);

                if (sqlProperty.NotNullable)
                {
                    Required.Add(name);
                }
            }
        }

        [JsonPropertyName("$schema")]
        public string Schema { get; } = "https://json-schema.org/draft/2020-12/schema";

        [JsonPropertyName("type")]
        public string Type { get; } = "object";

        [JsonPropertyName("properties")]
        public Dictionary<string, McpJsonSchemaProperty> Properties { get; } = new(StringComparer.OrdinalIgnoreCase);

        [JsonPropertyName("required")]
        public List<string> Required { get; } = new();

        [JsonPropertyName("additionalProperties")]
        public bool AdditionalProperties { get; } = false;
    }

    public class McpJsonSchemaProperty
    {
        [JsonPropertyName("type")]
        public required string Type { get; init; }

        [JsonPropertyName("format")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Format { get; init; }

        [JsonPropertyName("maxLength")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxLength { get; init; }

        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; init; }

        public static McpJsonSchemaProperty FromSqlProperty(SqlDataProperty sqlProperty)
        {
            var (type, format) = MapType(sqlProperty.DataType);

            return new McpJsonSchemaProperty
            {
                Type = type,
                Format = format,
                MaxLength = type == "string" && sqlProperty.MaxLength > 0 ? sqlProperty.MaxLength : null,
                Description = string.IsNullOrWhiteSpace(sqlProperty.Description) ? null : sqlProperty.Description
            };
        }

        private static (string Type, string? Format) MapType(string sqlType) => sqlType.ToLowerInvariant() switch
        {
            "bit" => ("boolean", null),
            "tinyint" or "smallint" or "int" or "bigint" => ("integer", null),
            "decimal" or "numeric" or "money" or "smallmoney" or "float" or "real" => ("number", null),
            "date" => ("string", "date"),
            "time" => ("string", "time"),
            "datetime" or "datetime2" or "smalldatetime" or "datetimeoffset" => ("string", "date-time"),
            "uniqueidentifier" => ("string", "uuid"),
            "binary" or "varbinary" or "image" or "timestamp" or "rowversion" => ("string", "byte"),
            _ => ("string", null)
        };
    }
}