namespace MoopelApi;

public static class FormatHelper
{
    public static string FormatNullable(string? value) => value ?? "null";
    public static string FormatDate(DateTime? value) =>
        value.HasValue ? $"{value.Value:yyyy-MM-dd HH:mm:ss} UTC" : "null";
}