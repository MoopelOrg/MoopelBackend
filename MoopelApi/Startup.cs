using System.Data;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using MoopelApi.Models.Configuration;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Sinks.MSSqlServer;

namespace MoopelApi;

public class Startup
{

    public static WebApplicationBuilder CreateBuilder(WebApplicationBuilder builder)
    {
        try
        {
            string dbConnection = builder.Configuration.GetConnectionString("MoopelDB") 
                ?? throw new("Failed to retrieve MoopelDB connection string.");

            builder = AddServices(builder);
            builder = AddLifetimeServices(builder);

            builder = AddLogging(builder, dbConnection);

            return builder;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred during startup: {ex.Message}");
            Log.Error(ex, "An error occurred during startup");
            throw;
        }
    }

    public static WebApplicationBuilder AddServices(WebApplicationBuilder builder)
    {
        //https://github.com/scalar/scalar/discussions/4468#discussioncomment-15371071
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            //https://learn.microsoft.com/en-us/azure/container-apps/dotnet-overview#define-x-forwarded-headers
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        JwtSettings jwt = builder.Configuration.GetRequiredSection("JwtSettings").Get<JwtSettings>()!;

        builder.Services.AddControllers();
        builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt.JwtKey)
                        ),
                    ClockSkew = TimeSpan.Zero
                };
            });

        /*
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(corsPolicyName, policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
        */

        return builder;
    }

    public static WebApplicationBuilder AddLifetimeServices(WebApplicationBuilder builder)
    {
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetRequiredSection("JwtSettings"));

        return builder;
    }

    public static WebApplicationBuilder AddLogging(WebApplicationBuilder builder, string dbConnection)
    {
        ColumnOptions logColumns = new();
        logColumns.Id.ColumnName = "LogId";
        logColumns.TimeStamp.ColumnName = "CreatedAtUtc";
        logColumns.Level.ColumnName = "Level";
        logColumns.Message.ColumnName = "Message";
        logColumns.Exception.ColumnName = "Exception";

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Warning()
            .Enrich.FromLogContext()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Error)
            .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Error)
            .WriteTo.MSSqlServer(
                connectionString: dbConnection,
                columnOptions: logColumns,
                sinkOptions: new MSSqlServerSinkOptions 
                { 
                    TableName = "LogEvents",
                    AutoCreateSqlTable = true,
                })
            .WriteTo.Console()
            .WriteTo.Seq()
            .CreateLogger();

        return builder;
    }

    public static async Task<WebApplication> ConfigureApp(WebApplicationBuilder builder, DeploymentInfo deploymentInfo)
    {
        WebApplication app = builder.Build();

        // https://github.com/scalar/scalar/discussions/4468#discussioncomment-15371071
        app.UseForwardedHeaders();

        app.UseStaticFiles();
        app.MapOpenApi();

        app.MapScalarApiReference(options =>
        {
            options.WithTitle($"[{deploymentInfo.Environment}] Moopel API")
                   .WithFavicon("/favicon.png")
                   .WithTheme(ScalarTheme.Kepler)
                   .AddPreferredSecuritySchemes(JwtBearerDefaults.AuthenticationScheme);
        });

        app.UseHttpsRedirection();
        //app.UseCors(corsPolicyName);

        app.UseAuthorization();

        app.MapGet("/", () => Results.Ok(new
        {
            status = "Healthy",
            time = DateTime.UtcNow,
            deploymentInfo
        }));

        app.MapControllers();

        return app;
    }


    #region Database
    public static bool BuildDatabase(string connectionString)
    {
        try
        {
            using SqlConnection conn = new(connectionString);
            conn.Open();

            CreateTables(conn);
            CheckTables(conn);
            CreateTriggers(conn);

            return true;
        }
        catch (Exception ex)
        {
            throw new($"Failed to Seed Database. Error: {ex.Message}", ex);
        }
    }

    private static void CreateTables(SqlConnection conn)
    {
        using SqlCommand command = conn.CreateCommand();

        try
        {
        }
        catch (Exception ex)
        {
            throw new($"Failed to create tables. Command Text: {command.CommandText}. Error: {ex.Message}", ex);
        }
    }

    private static void CheckTables(SqlConnection conn)
    {
        SqlCommand command = conn.CreateCommand();
        foreach (DatabaseTable table in DatabaseTables)
        {
            command.CommandText = table.GenerateSelect();
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new($"Failed to query {table.Name}. Error: {ex.Message}", ex);
            }
        }
    }

    private static void CreateTriggers(SqlConnection conn)
    {
        SqlCommand command = conn.CreateCommand();

        try
        {
        }
        catch (Exception ex)
        {
            throw new($"Failed to create triggers. Command Text: {command.CommandText}. Error: {ex.Message}", ex);
        }
    }

    #endregion
}
