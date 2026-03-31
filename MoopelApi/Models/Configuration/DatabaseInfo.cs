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
        return $"SELECT {string.Join(", ", Fields.Select(f => f.SelectName))} FROM [{Name}]";
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
    public string SelectName => $"[{Name}]";

    public required SqlDbType Type { get; init; }
    public required bool Nullable { get; init; }
    public int? FieldSize { get; init; } = null;
    public int? Precision { get; init; } = null;

    public string? DefaultValue { get; init; } = null;

    public DatabaseField? Reference { get; init; } = null;

    public string FieldDataType
    {
        get
        {
            string? sizeOrMax = FieldSize.ToString();
            if (string.IsNullOrEmpty(sizeOrMax))
                sizeOrMax = "MAX";
            string typePart = Type switch
            {
                SqlDbType.Int => "INT",
                SqlDbType.BigInt => "BIGINT",
                SqlDbType.SmallInt => "SMALLINT",
                SqlDbType.TinyInt => "TINYINT",

                SqlDbType.Bit => "BIT",

                SqlDbType.NVarChar => $"NVARCHAR({sizeOrMax})",
                SqlDbType.VarChar => $"VARCHAR({sizeOrMax})",

                SqlDbType.Text => "TEXT",
                SqlDbType.NText => "NTEXT",

                SqlDbType.DateTime2 => "DATETIME2",
                SqlDbType.DateTimeOffset => "DATETIMEOFFSET",

                SqlDbType.Decimal => $"DECIMAL({FieldSize},{Precision})",
                SqlDbType.Money => "MONEY",
                SqlDbType.SmallMoney => "SMALLMONEY",
                SqlDbType.Float => "FLOAT",
                SqlDbType.Real => "REAL",

                SqlDbType.UniqueIdentifier => "UNIQUEIDENTIFIER",
                SqlDbType.Image => "IMAGE",

                SqlDbType.Xml => "XML",

                _ => throw new NotSupportedException($"Unsupported SQL data type: {Type}")
            };

            string nullPart = Nullable ? "NULL" : "NOT NULL";
            string defaultValue = DefaultValue != null ? $"DEFAULT {DefaultValue}" : string.Empty;

            return $"{typePart} {nullPart} {defaultValue}";
        }
    }

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
