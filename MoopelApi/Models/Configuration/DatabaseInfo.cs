namespace MoopelApi.Models.Configuration;

using System.Data;
using Microsoft.Data.SqlClient;

public class DatabaseTable
{
    public required string Name { get; init; }

    public required List<DatabaseField> Fields { get; init; }

    public required string Alias { get; init; }

    public string GenerateSelect()
    {
        return $"SELECT {string.Join(", ", Fields.Select(f => f.SelectName))} FROM `{Name}`";
    }
    public string GetFields(string alias)
    {
        return $"{string.Join(", ", Fields.Select(f => $"{alias}.{f.SelectName} as {alias}_{f.Name}"))}";
    }
    public string GetFields()
    {
        return $"{string.Join(", ", Fields.Select(field => field.SelectName))}";
    }
}

public class DatabaseField
{
    public required string Name { get; init; }
    public string SelectName => $"`{Name}`";

    public required SqlDbType Type { get; init; }
    public required bool Nullable { get; init; }
    public int? FieldSize { get; init; } = null;

    public DatabaseField? Reference { get; init; } = null; 

    public SqlParameter GenerateParameter(string paramName, object? value)
    {
        if (FieldSize is not null)
        {
            return new SqlParameter()
            {
                ParameterName = paramName,
                SqlDbType = Type,
                Value = value ?? DBNull.Value,
                Size = FieldSize.Value,
            };
        }
        return new SqlParameter()
        {
            ParameterName = paramName,
            SqlDbType = Type,
            Value = value ?? DBNull.Value
        };
    }
}
