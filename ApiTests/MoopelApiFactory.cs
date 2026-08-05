using System.Data.Common;
using System.Text.Json;

using ApiTests.TestData;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using MoopelApi;
using MoopelApi.Models.Configuration;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Enums;
using MoopelApi.Models.Requests;
using MoopelApi.Services.Interfaces;

using Respawn;

using Testcontainers.MsSql;

using static MoopelApi.ConstantValues.Database;


namespace ApiTests;

public class MoopelApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    internal static JsonSerializerOptions SharedJsonSerializerOptions { get; } =
        CreateJsonSerializerOptions();

    public JsonSerializerOptions JsonSerializerOptions => SharedJsonSerializerOptions;

    private static JsonSerializerOptions CreateJsonSerializerOptions()
    {
        JsonSerializerOptions options = new();
        ConstantValues.ConfigureJsonSerializerOptions(options);
        return options;
    }

    private readonly MsSqlContainer _dbContainer =
    new MsSqlBuilder(image: "mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("YourStrong!Passw0rd")
        .Build();

    private const int DatabaseTimeoutSeconds = 30;

    private Respawner _respawner = null!;
    private DbConnection _connection = null!;

    public string ConnectionString => _dbContainer.GetConnectionString();

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        GC.SuppressFinalize(this);

        Console.WriteLine($"Disposing of ApiFactory UTC:{DateTime.UtcNow}");
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    async ValueTask IAsyncLifetime.InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Force the app to build and run BuildDatabase so that all application
        // tables exist before Respawner takes its schema snapshot.
        CreateClient().Dispose();

        _connection = new SqlConnection(ConnectionString);
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = ["dbo"]
        });

        await SeedAdminUserAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.Sources.Clear();

            Dictionary<string, string?> overrideConfig = new()
            {
                ["DatabaseConnection:ConnectionString"] = ConnectionString
            };

            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
            config.AddInMemoryCollection(overrideConfig);
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DatabaseConnection>();
            services.AddSingleton(new DatabaseConnection
            {
                ConnectionString = ConnectionString,
                DatabaseName = "MoopelDB",
                Timeout = DatabaseTimeoutSeconds
            });
        });

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_connection);
        await SeedAdminUserAsync();
    }

    private async Task SeedAdminUserAsync()
    {
        using IServiceScope scope = Services.CreateScope();
        IAuthService authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        User? adminUser = await authService.Register(new RegistrationRequest
        {
            Username = AuthTestData.AdminUsername,
            Password = AuthTestData.AdminPassword
        });

        if (adminUser is null)
        {
            throw new InvalidOperationException("Failed to register the test admin user.");
        }

        using DbCommand command = _connection.CreateCommand();
        command.CommandText =
            $"""
             UPDATE {UsersTable.SelectName}
             SET {UserRoleField.SelectName} = @Role
             WHERE {UserIdField.SelectName} = @UserId;
             """;

        command.Parameters.Add(UserRoleField.GenerateParameter("@Role", UserRole.Admin.ToString()));
        command.Parameters.Add(UserIdField.GenerateParameter("@UserId", adminUser.UserId));

        int updatedRows = await command.ExecuteNonQueryAsync();
        if (updatedRows != 1)
        {
            throw new InvalidOperationException("Failed to promote the test user to administrator.");
        }
    }
}
