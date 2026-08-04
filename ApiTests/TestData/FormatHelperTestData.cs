namespace ApiTests.TestData;

internal static class FormatHelperTestData
{
    public const string NullText = "null";
    public const string DoingWhat = "writing coverage";
    public const string Message = "something broke";
    public const string ParameterName = "UserId";
    public const string ParameterValue = "42";
    public const string CommandText = "SELECT 1";
    public const string HttpMethod = "GET";
    public const string HttpPath = "/api/test";
    public const string HttpQuery = "?page=1";
    public const string UserName = "test-user";
    public const int UserId = 123;

    public static readonly DateTime UtcDateTime = new(2026, 8, 4, 18, 39, 0, DateTimeKind.Utc);
}

