namespace ApiTests.TestData;

internal static class AuthServiceTestData
{
    public const int AdminUserId = 1;
    public const int ThemeOwnerUserId = 2;
    public const int ThemeEditorUserId = 3;
    public const int OtherUserId = 4;
    public const int RequestUserId = 5;
    public const int PrimaryThemeId = 6;
    public const int SecondaryThemeId = 7;

    public const string AdminUserName = "authservice-admin";
    public const string ThemeOwnerUserName = "authservice-theme-owner";
    public const string ThemeEditorUserName = "authservice-theme-editor";
    public const string OtherUserName = "authservice-other-user";

    public const string RegisterUsername = "authservice-user";
    public const string RegisterPassword = "AuthService1!";
    public const string DuplicateUsername = "authservice-duplicate";
    public const string GuestPrefix = "Guest_";
    public const string GuestUsername = "Guest_authservice";
    public const string InvalidPasswordSuffix = "wrong";

    public const int MissingUserId = 999997;
    public const int MissingGroupId = 999996;

    public const string GroupName = "AuthService Group";
    public const string GroupDescription = "AuthService Group Description";

    public const string ThemeName = "AuthService Theme";
    public const string ThemeData = "{}";
    public const string ThemeDescription = "AuthService Theme Description";

    public const string RequestIpAddress = "127.0.0.1";
    public const string RequestUserAgent = "AuthServiceTests";
    public const string RequestPath = "/auth-service-tests";
    public const string RequestMethod = "POST";
    public const string RequestTraceId = "trace-id";

    public const string JwtAudience = "authservice-audience";
    public const string JwtIssuer = "authservice-issuer";
    public const string JwtKey = "0123456789abcdef0123456789abcdef";
    public const int JwtExpiryMinutes = 30;
    public const int InvalidJwtExpiryMinutes = 0;

    public static readonly DateTime RequestedAtUtc = new(2026, 8, 4, 22, 39, 0, DateTimeKind.Utc);
}
