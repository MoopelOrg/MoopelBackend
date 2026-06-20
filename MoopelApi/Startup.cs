using System.Data;
using System.Text;
using System.Threading.RateLimiting;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

using MoopelApi.Helpers;
using MoopelApi.Models;
using MoopelApi.Models.Configuration;
using MoopelApi.Models.Dto;
using MoopelApi.Models.Enums;
using MoopelApi.Services;
using MoopelApi.Services.Interfaces;

using Scalar.AspNetCore;

using Serilog;
using Serilog.Sinks.MSSqlServer;

using static MoopelApi.ConstantValues.Database;

namespace MoopelApi;

public class Startup
{
    private WebApplicationBuilder _builder;
    private readonly DeploymentInfo _deploymentInfo;
    private readonly DatabaseConnection _databaseConnection;

    public Startup(string[] args)
    {
        _builder = WebApplication.CreateBuilder(args);

        _deploymentInfo = new()
        {
            BuildDate = _builder.Configuration["DEPLOYED_AT"] ?? "Date Not Found. Date Now: " + FormatHelper.FormatDate(DateTime.UtcNow),
            Environment = _builder.Configuration["ASPNETCORE_ENVIRONMENT"] ?? "Unknown",
            Version = _builder.Configuration["VERSION"] ?? "Unknown",
            CommitName = _builder.Configuration["COMMIT_NAME"] ?? "Unknown",
            Tag = _builder.Configuration["GIT_TAG"] ?? "Unknown"
        };

        _databaseConnection = _builder.Configuration
            .GetRequiredSection("DatabaseConnection")
            .Get<DatabaseConnection>()
            ?? throw new("Failed to retrieve DatabaseConnection configuration.");
    }

    public async Task<WebApplicationBuilder> CreateBuilder()
    {
        try
        {
            if (_databaseConnection.ConnectionString.Contains("localhost"))
            {
                Console.WriteLine("Running in development environment. Waiting for database to be ready...");
                await Task.Delay(TimeSpan.FromSeconds(20));
            }

            _builder.Services.AddSingleton(_databaseConnection);

            _builder = AddServices();
            _builder = AddLifetimeServices();

            _builder = AddLogging();

            return _builder;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred during startup: {ex.Message}");
            Log.Error(ex, "An error occurred during startup");
            throw;
        }
    }

    public WebApplicationBuilder AddServices()
    {
        //https://github.com/scalar/scalar/discussions/4468#discussioncomment-15371071
        _builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            //https://learn.microsoft.com/en-us/azure/container-apps/dotnet-overview#define-x-forwarded-headers
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        JwtSettings jwt = _builder.Configuration.GetRequiredSection("JwtSettings").Get<JwtSettings>()!;

        _builder.Services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                ConstantValues.ConfigureJsonSerializerOptions(options.JsonSerializerOptions);
            });

        _builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });

        _builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

        _builder.Services.AddAuthorizationBuilder()
            .AddPolicy(Policies.NotGuest, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(Roles.Standard, Roles.Admin);
            })
            .AddPolicy(Policies.AdminOnly, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(Roles.Admin);
            });


        _builder.Services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 10,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    }));
        });

        /*
        _builder.Services.AddCors(options =>
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

        return _builder;
    }

    public WebApplicationBuilder AddLifetimeServices()
    {
        _builder.Services.Configure<JwtSettings>(_builder.Configuration.GetRequiredSection("JwtSettings"));

        _builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        _builder.Services.Configure<PasswordHasherOptions>(options =>
        {
            options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
            options.IterationCount = 100_000;
        });


        _builder.Services.AddScoped<IAuthService, AuthService>();
        _builder.Services.AddScoped<IUserService, UserService>();
        _builder.Services.AddScoped<IThemeService, ThemeService>();

        _builder.Services.AddScoped<IContextService, ContextService>();
        _builder.Services.AddScoped<INotificationService, NotificationService>();
        _builder.Services.AddScoped<ISessionService, SessionService>();

        _builder.Services.AddScoped<IImageService, ImageService>();

        _builder.Services.AddScoped<IBankService, BankService>();
        _builder.Services.AddScoped<IFridgeService, FridgeService>();
        _builder.Services.AddScoped<IStorageService, StorageService>();
        _builder.Services.AddScoped<INoteService, NoteService>();

        _builder.Services.AddScoped<ITagService, TagService>();

        _builder.Services.AddScoped<IGroupService, GroupService>();
        _builder.Services.AddScoped<IWorkItemService, WorkItemService>();

        return _builder;
    }

    public WebApplicationBuilder AddLogging()
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
                connectionString: _databaseConnection.ConnectionString,
                columnOptions: logColumns,
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = "LogEvents",
                    AutoCreateSqlTable = true,
                })
            .WriteTo.Console()
            //.WriteTo.Seq()
            .CreateLogger();

        return _builder;
    }

    public async Task<WebApplication> ConfigureApp()
    {
        WebApplication app = _builder.Build();

        // https://github.com/scalar/scalar/discussions/4468#discussioncomment-15371071
        app.UseForwardedHeaders();

        app.UseMiddleware<GlobalExceptionMiddleware>();

        app.UseStaticFiles();
        app.MapOpenApi();

        app.MapScalarApiReference(options =>
        {
            options.WithTitle($"[{_deploymentInfo.Environment}] Moopel API")
                   .WithFavicon("/favicon.png")
                   .WithTheme(ScalarTheme.Kepler)
                   .AddPreferredSecuritySchemes(JwtBearerDefaults.AuthenticationScheme);
        });

        app.UseRateLimiter();

        app.UseHttpsRedirection();
        //app.UseCors(corsPolicyName);

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/", () => Results.Ok(new
        {
            status = "Healthy",
            time = DateTime.UtcNow,
            _deploymentInfo
        }));

        app.MapControllers();

        return app;
    }


    #region Database
    public bool BuildDatabase()
    {
        try
        {
            using SqlConnection conn = new(_databaseConnection.ConnectionString);
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
            foreach (DatabaseTable table in DatabaseTables)
            {
                command.CommandText = table.GenerateCreateTable();
                command.ExecuteNonQuery();
                command.CommandText = string.Empty;
            }
        }
        catch (Exception ex)
        {
            throw new($"Failed to create tables. Command Text: {command.CommandText}. Error: {ex.Message}", ex);
        }
    }

    private static void CheckTables(SqlConnection conn)
    {
        SqlCommand command = conn.CreateCommand();
        List<string> aliases = [];
        foreach (DatabaseTable table in DatabaseTables)
        {
            if (aliases.Contains(table.Alias))
            {
                throw new($"Duplicate table alias detected: {table.Alias} for table {table.Name}");
            }
            else
            {
                aliases.Add(table.Alias);
            }

            command.CommandText =
                $"""
                 SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE, COLUMN_DEFAULT, NUMERIC_PRECISION, NUMERIC_SCALE 
                                    FROM INFORMATION_SCHEMA.COLUMNS
                                    WHERE TABLE_NAME = '{table.Name}'
                 """;
            try
            {
                using SqlDataReader reader = command.ExecuteReader();
                List<DatabaseField> fields = [];
                while (reader.Read())
                {
                    string? columnName = SqlReaderHelper.GetNullableString(reader, "COLUMN_NAME");
                    string? dataType = SqlReaderHelper.GetNullableString(reader, "DATA_TYPE");
                    string? maxLength = SqlReaderHelper.GetNullableString(reader, "CHARACTER_MAXIMUM_LENGTH");
                    string? isNullable = SqlReaderHelper.GetNullableString(reader, "IS_NULLABLE");
                    string? columnDefault = SqlReaderHelper.GetNullableString(reader, "COLUMN_DEFAULT");
                    int? fieldSize = null;
                    int? precision = null;

                    // Get FieldSize
                    if (!string.IsNullOrWhiteSpace(maxLength))
                    {
                        fieldSize = int.Parse(maxLength);
                    }
                    // -1 is max which I represent with null
                    if (fieldSize == -1)
                    {
                        fieldSize = null;
                    }

                    if (columnName is null || dataType is null || isNullable is null)
                    {
                        throw new($"Missing required column information for a field in table {table.Name}");
                    }
                    if (fields.Any(fields => fields.Name == columnName))
                    {
                        throw new($"Duplicate column name detected: {columnName} in table {table.Name}");
                    }

                    SqlDbType sqlType = SqlHelper.GetSqlDbType(dataType);

                    if (sqlType == SqlDbType.Decimal)
                    {
                        string? numericPrecision = SqlReaderHelper.GetNullableString(reader, "NUMERIC_PRECISION");
                        string? numericScale = SqlReaderHelper.GetNullableString(reader, "NUMERIC_SCALE");
                        if (numericPrecision is null || numericScale is null)
                        {
                            throw new($"Missing precision or scale information for decimal field {columnName} in table {table.Name}");
                        }
                        fieldSize = int.Parse(numericPrecision);
                        precision = int.Parse(numericScale);
                    }

                    fields.Add(new DatabaseField
                    {
                        Name = columnName,
                        Type = sqlType,

                        FieldSize = fieldSize,
                        Nullable = isNullable == "YES"
                    });
                }
                string? error = CompareFields(table.Fields, fields);
                if (error is not null)
                    throw new($"Issue with {table.Name} Table: {error}");
            }
            catch (Exception ex)
            {
                throw new($"Failed to query {table.Name}. Error: {ex.Message}", ex);
            }
        }
    }

    private static string? CompareFields(List<DatabaseField> fieldSet1, List<DatabaseField> fieldSet2)
    {
        try
        {
            if (fieldSet1.Count != fieldSet2.Count)
            {
                return $"Field count mismatch: expected {fieldSet1.Count}, found {fieldSet2.Count}";
            }
            foreach (DatabaseField field1 in fieldSet1)
            {
                DatabaseField? matchingField = fieldSet2.FirstOrDefault(f => f.Name == field1.Name);
                if (matchingField == null)
                {
                    return $"Missing field {field1.Name}";
                }
                if (matchingField.Type != field1.Type)
                {
                    return $"Data type mismatch for field {field1.Name}: expected {field1.Type}, found {matchingField.Type}";
                }
                if (matchingField.Nullable != field1.Nullable)
                {
                    return $"Nullability mismatch for field {field1.Name}: expected {(field1.Nullable ? "NULL" : "NOT NULL")}, found {(matchingField.Nullable ? "NULL" : "NOT NULL")}";
                }
                if (matchingField.FieldSize != field1.FieldSize)
                {
                    return $"Field size mismatch for field {field1.Name}: expected {field1.FieldSize}, found {matchingField.FieldSize}";
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            throw new($"Error comparing fields. Error: {ex.Message}", ex);
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
