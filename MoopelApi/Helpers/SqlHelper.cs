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
}
