using System.Text.Json;

namespace DbMCP.Tools;

public class SqlDataObject
{
    public string Name { get; set; } = string.Empty;
    public CodeType Type { get; set; }
    public string? Description { get; set; }
    public List<SqlDataProperty> DataOutput { get; set; } = new List<SqlDataProperty>();

    public JsonElement? BuildOutputSchema() => BuildMcpSchema(DataOutput);
    public static JsonElement? BuildMcpSchema(List<SqlDataProperty> sqlProperties)
    {
        if (sqlProperties is null || sqlProperties.Count == 0) return null;
        var schema = new McpJsonSchema202012(sqlProperties);
        return JsonSerializer.SerializeToElement(schema);
    }
}
public class SqlCodeObject : SqlDataObject
{
    public List<SqlDataProperty> Parameters { get; set; } = new List<SqlDataProperty>();
    public JsonElement? BuildInputSchema() => BuildMcpSchema(Parameters);
}
public enum CodeType
{
    View,
    Procedure,
    Function
}
public class SqlDataProperty
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = "varchar";
    public int MaxLength { get; set; } = -1;
    public bool NotNullable { get; set; }
    public string? Description { get; set; }
    public override string ToString()
    {
        var descr = string.IsNullOrWhiteSpace(Description) ? "" : $" ({Description})";
        return $"{Name.Trim().TrimStart('@')} as {(NotNullable ? " not nullable" : "")} {DataType}{(MaxLength > 0 ? $" with max length {MaxLength}" : "")}{descr}";
    }
}