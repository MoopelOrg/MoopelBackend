using Microsoft.Extensions.Configuration;

namespace ApiTests;

internal class IntegrationTestConfig
{
    private const string DefaultSqlServerTestContainerPassword = "YourStrong!Passw0rd";
    private static readonly IConfigurationRoot _config = Build();

    private static IConfigurationRoot Build()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables(prefix: "Moopel_")
            .Build();
    }

    public static string? DatabaseConnectionString =>
        Nullable(_config.GetConnectionString("") ?? _config["Moopel_Database"]);

    public static string RequireDatabase() =>
        DatabaseConnectionString
        ?? IgnoreForMissing("Database");

    public static string SqlServerTestContainerPassword =>
        Nullable(_config["SqlServerTestContainerPassword"])
        ?? DefaultSqlServerTestContainerPassword;

    private static string IgnoreForMissing(string name)
    {
        Assert.Skip($"No {name} configured. Set the field in configuration.");
        return string.Empty;
    }

    private static string? Nullable(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
