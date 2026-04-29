using System.Data;
using Microsoft.Data.SqlClient;

namespace MoopelApi.Helpers;

public static class SqlHelper
{
    public static async Task CheckConnection(string connection, string connectionName)
    {
        try
        {
            await using SqlConnection connectionToCheck = new(connection);
            await connectionToCheck.OpenAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"[{connectionName}] failed to open. " +
                                $"Check connection works, if testing run {ConstantValues.DB_DOCKER_RUN} and use that connection " +
                                $"Error: {ex}");
        }
    }

    public static string? GetNullableString(SqlDataReader reader, string columnName)
    {
        try
        {
            int ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal))
                return null;
            return reader[columnName].ToString();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting nullable string for column '{columnName}': {ex}");
        }
    }

    public static SqlDbType GetSqlDbType(string type)
    {
        return type.ToLower() switch
        {
            // Integers
            "int" => SqlDbType.Int,
            "bigint" => SqlDbType.BigInt,
            "smallint" => SqlDbType.SmallInt,
            "tinyint" => SqlDbType.TinyInt,

            "bit" => SqlDbType.Bit,

            // Strings
            "nvarchar" => SqlDbType.NVarChar,
            "varchar" => SqlDbType.VarChar,
            "text" => SqlDbType.Text,
            "ntext" => SqlDbType.NText,
            "varbinary" => SqlDbType.VarBinary,

            "datetime" => SqlDbType.DateTime,
            "datetime2" => SqlDbType.DateTime2,
            "datetimeoffset" => SqlDbType.DateTimeOffset,

            // Numbers / money
            "decimal" => SqlDbType.Decimal,
            "numeric" => SqlDbType.Decimal,
            "money" => SqlDbType.Money,
            "float" => SqlDbType.Float,
            "real" => SqlDbType.Real,

            // Other
            "uniqueidentifier" => SqlDbType.UniqueIdentifier,
            "xml" => SqlDbType.Xml,

            _ => throw new Exception($"Could not identify SqlDbType of '{type}'")
        };
    }
}
