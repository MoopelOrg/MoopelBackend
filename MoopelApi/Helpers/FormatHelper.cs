using System.Text;
using Microsoft.Data.SqlClient;
using Serilog.Sinks.MSSqlServer;

namespace MoopelApi.Helpers;

public static class FormatHelper
{
    public static string FormatNullable(string? value) => value ?? "null";
    public static string FormatDate(DateTime? value) =>
        value.HasValue ? $"{value.Value:yyyy-MM-dd HH:mm:ss} UTC" : "null";

    public static string FormatErrorLogMessage(
        string doingWhat,
        string message,
        List<(string Name, object? Value)>? parameters = null, 
        SqlCommand? command = null)
    {
        StringBuilder sb = new();

        sb.AppendLine($"Error {doingWhat}");
        sb.AppendLine(message);

        if (parameters is not null)
        {
            sb.AppendLine();
            foreach ((string Name, object? Value) in parameters)
            {
                string value;
                if (Value is null)
                    value = "NULL";
                else
                    value = Value.ToString()!;
                sb.Append($"{Name}: {value}" );
            }
        }

        if (command is not null)
        {
            sb.AppendLine();
            sb.AppendLine($"CommandText: {command.CommandText}");
        }

        return sb.ToString();
    }
}