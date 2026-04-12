using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Respawn;
using Testcontainers.MsSql;


namespace ApiTests;

public class MoopelApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer =
    new MsSqlBuilder(image: "mcr.microsoft.com/mssql/server:2022-latest")
        .WithEnvironment("ACCEPT_EULA", "Y")
        .WithPassword("YourStrong!Passw0rd")
        .WithPortBinding(1433, assignRandomHostPort: true)
        .Build();

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

        _connection = new SqlConnection(ConnectionString);
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = ["dbo"]
        });
    }


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });

        builder.ConfigureAppConfiguration((context, config) =>
        {
            Dictionary<string, string?> overrideConfig = new()
            {
                ["ConnectionStrings:MoopelDB"] = ConnectionString
            };

            config.AddInMemoryCollection(overrideConfig);
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_connection);
    }
}
