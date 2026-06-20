namespace MoopelApi.Models.Configuration;

using System.Data;

using Microsoft.Data.SqlClient;

public class DatabaseTable
{
    public required string Name { get; init; }

    public string SelectName => $"[{Name}]";

    public required List<DatabaseField> Fields { get; init; }

    public required List<DatabaseConstraint> Constraints { get; init; }

    public required string Alias { get; init; }

    public string GenerateSelect()
    {
        return $"SELECT {string.Join(", ", Fields.Select(f => f.SelectName))} FROM {SelectName}";
    }
    public string GetFields(string alias)
    {
        return $"{string.Join(", ", Fields.Select(f => $"{alias}.{f.SelectName} as {alias}_{f.Name}"))}";
    }
    public string GetFields()
    {
        return $"{string.Join(", ", Fields.Select(field => field.SelectName))}";
    }
    public string GenerateCreateTable()
    {
        try
        {
            List<string> tableParts = [];

            tableParts.AddRange(Fields.Select(field =>
                $"    {field.SelectName} {field.FieldDataType}"));

            tableParts.AddRange(Constraints
                .Where(c => c.FilterExpression is null)
                .Select(constraint => $"    {constraint.GenerateSql()}"));

            List<DatabaseConstraint> filteredConstraints = Constraints
                .Where(c => c.FilterExpression is not null)
                .ToList();

            string filteredIndexesSql = filteredConstraints.Count > 0
                ? "\n                                " + string.Join("\n                                ",
                    filteredConstraints.Select(c => c.GenerateFilteredIndexSql(Name) + ";"))
                : string.Empty;

            return $"""

                                IF OBJECT_ID('dbo.{Name}', 'U') IS NULL
                                BEGIN
                                CREATE TABLE {SelectName} (
                                {string.Join(",\n", tableParts)}
                                );{filteredIndexesSql}
                                END
                    """;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error generating CREATE TABLE SQL for table '{Name}': {ex.Message}", ex);
        }
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

    public bool IsPrimaryKey { get; init; } = false;
    public bool IsIdentity { get; init; } = false;

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
                SqlDbType.VarBinary => $"VARBINARY({sizeOrMax})",

                SqlDbType.Text => "TEXT",
                SqlDbType.NText => "NTEXT",

                SqlDbType.DateTime => "DATETIME",
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

            string identityPart = IsIdentity ? "IDENTITY(1,1)" : string.Empty;
            string pkPart = IsPrimaryKey ? "PRIMARY KEY" : string.Empty;

            return $"{typePart} {identityPart} {nullPart} {defaultValue} {pkPart}".Trim();
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

public class DatabaseConstraint
{
    public required string Name { get; init; }
    public required DatabaseConstraintType Type { get; init; }
    public required List<DatabaseField> Fields { get; init; }

    public DatabaseTable? ReferenceTable { get; init; } = null;
    public List<DatabaseField>? ReferenceFields { get; init; } = null;

    public string? CheckSql { get; init; } = null;
    public string OnDelete { get; init; } = "NO ACTION";
    public string? FilterExpression { get; init; } = null;

    public string GenerateSql()
    {
        string fields = string.Join(", ", Fields.Select(f => f.SelectName));

        return Type switch
        {
            DatabaseConstraintType.Unique =>
                $"CONSTRAINT [{Name}] UNIQUE ({fields})",

            DatabaseConstraintType.CompositePrimaryKey =>
                $"CONSTRAINT [{Name}] PRIMARY KEY ({fields})",

            DatabaseConstraintType.Check =>
                $"CONSTRAINT [{Name}] CHECK ({CheckSql})",

            DatabaseConstraintType.ForeignKey =>
                GenerateForeignKeySql(fields),

            _ => throw new NotSupportedException($"Unsupported constraint type: {Type}")
        };
    }

    public string GenerateFilteredIndexSql(string tableName)
    {
        if (Type != DatabaseConstraintType.Unique)
            throw new InvalidOperationException($"FilterExpression is only supported on Unique constraints (constraint '{Name}').");

        if (FilterExpression is null)
            throw new InvalidOperationException($"Constraint '{Name}' has no FilterExpression.");

        string fields = string.Join(", ", Fields.Select(f => f.SelectName));
        return $"CREATE UNIQUE INDEX [{Name}] ON [{tableName}]({fields}) WHERE {FilterExpression}";
    }

    private string GenerateForeignKeySql(string fields)
    {
        if (ReferenceTable is null)
            throw new InvalidOperationException($"Foreign key {Name} is missing ReferenceTable.");

        if (ReferenceFields is null || ReferenceFields.Count == 0)
            throw new InvalidOperationException($"Foreign key {Name} is missing ReferenceFields.");

        string referenceFields = string.Join(", ", ReferenceFields.Select(f => f.SelectName));

        return
            $"CONSTRAINT [{Name}] FOREIGN KEY ({fields}) " +
            $"REFERENCES {ReferenceTable.SelectName}({referenceFields}) " +
            $"ON DELETE {OnDelete}";
    }
}

public enum DatabaseConstraintType
{
    Unique,
    CompositePrimaryKey,
    ForeignKey,
    Check
}