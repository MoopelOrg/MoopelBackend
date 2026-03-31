using System.Data;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using MoopelApi.Helpers;
using MoopelApi.Models.Configuration;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Sinks.MSSqlServer;

using static MoopelApi.ConstantValues;

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
            //.WriteTo.Seq()
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
            // Groups Table
            command.CommandText =
            $@"
            IF OBJECT_ID('dbo.{GroupsTable.Name}', 'U') IS NULL
            BEGIN

            CREATE TABLE [{GroupsTable.Name}] (
                {GroupIdField.SelectName} {GroupIdField.FieldDataType} PRIMARY KEY IDENTITY(1,1),
                {GroupNameField.SelectName} {GroupNameField.FieldDataType},
                {GroupDescriptionField.SelectName} {GroupDescriptionField.FieldDataType},
                {GroupCreatedAtUtcField.SelectName} {GroupCreatedAtUtcField.FieldDataType},

                CONSTRAINT UQ_{GroupsTable.Name}_{GroupNameField.Name} UNIQUE ({GroupNameField.SelectName})
            );

            END
            ";
            command.ExecuteNonQuery();

            // Users Table
            command.CommandText =
            @$"
            IF OBJECT_ID('dbo.{UsersTable.Name}', 'U') IS NULL
            BEGIN

            CREATE TABLE [{UsersTable.Name}] (
                {UserIdField.SelectName} {UserIdField.FieldDataType} PRIMARY KEY IDENTITY(1,1),

                {UserNameField.SelectName} {UserNameField.FieldDataType},
                {UserEmailField.SelectName} {UserEmailField.FieldDataType},
                {UserPasswordHashField.SelectName} {UserPasswordHashField.FieldDataType},
                {UserRoleField.SelectName} {UserRoleField.FieldDataType},
                {UserCreatedAtUtc.SelectName} {UserCreatedAtUtc.FieldDataType},
                {UserDeactivated.SelectName} {UserDeactivated.FieldDataType},
                
                CONSTRAINT UQ_{UsersTable.Name}_{UserNameField.Name} UNIQUE ({UserNameField.SelectName}),
                CONSTRAINT UQ_{UsersTable.Name}_{UserEmailField.Name} UNIQUE ({UserEmailField.SelectName}),
                CONSTRAINT CHK_{UsersTable.Name}_{UserRoleField.Name} CHECK ({UserRoleField.SelectName} IN ('User', 'Admin'))
            );

            END";
            command.ExecuteNonQuery();

            // GroupUsers Table
            command.CommandText =
            $@"
            IF OBJECT_ID('dbo.{GroupUsersTable.Name}', 'U') IS NULL
            BEGIN

            CREATE TABLE [{GroupUsersTable.Name}] (
                {GroupUsersGroupIdField.SelectName} {GroupUsersGroupIdField.FieldDataType},
                {GroupUsersUserIdField.SelectName} {GroupUsersUserIdField.FieldDataType},
                
                {GroupUsersCanReadWorkItemsField.SelectName} {GroupUsersCanReadWorkItemsField.FieldDataType},
                {GroupUsersCanWriteWorkItemsField.SelectName} {GroupUsersCanWriteWorkItemsField.FieldDataType},                

                CONSTRAINT PK_{GroupUsersTable.Name} PRIMARY KEY ({GroupUsersGroupIdField.SelectName}, {GroupUsersUserIdField.SelectName}),
                CONSTRAINT FK_{GroupUsersTable.Name}_{GroupsTable.Name} FOREIGN KEY ({GroupUsersGroupIdField.SelectName}) REFERENCES {GroupsTable.Name}({GroupIdField.SelectName}) ON DELETE CASCADE,
                CONSTRAINT FK_{GroupUsersTable.Name}_{UsersTable.Name} FOREIGN KEY ({GroupUsersUserIdField.SelectName}) REFERENCES {UsersTable.Name}({UserIdField.SelectName}) ON DELETE CASCADE
            );

            END
            ";
            command.ExecuteNonQuery();

            // UserSettings Table
            command.CommandText =
            $@"
            IF OBJECT_ID('dbo.{UserSettingsTable.Name}', 'U') IS NULL
            BEGIN

            CREATE TABLE [{UserSettingsTable.Name}] (
                {UserSettingsIdField.SelectName} {UserSettingsIdField.FieldDataType} PRIMARY KEY IDENTITY(1,1),
                {UserSettingsUserIdField.SelectName} {UserSettingsUserIdField.FieldDataType},
                
                CONSTRAINT FK_{UserSettingsTable.Name}_{UsersTable.Name} FOREIGN KEY ({UserSettingsUserIdField.SelectName}) REFERENCES {UsersTable.Name}({UserIdField.SelectName}) ON DELETE CASCADE
            );

            END
            ";
            command.ExecuteNonQuery();

            // Themes Table
            command.CommandText =
            $@"
            IF OBJECT_ID('dbo.{ThemesTable.Name}', 'U') IS NULL
            BEGIN

            CREATE TABLE [{ThemesTable.Name}] (
                {ThemeIdField.SelectName} {ThemeIdField.FieldDataType} PRIMARY KEY IDENTITY(1,1),
                {ThemeUserIdField.SelectName} {ThemeUserIdField.FieldDataType},
                {ThemeNameField.SelectName} {ThemeNameField.FieldDataType},
                {ThemeCreatedAtUtcField.SelectName} {ThemeCreatedAtUtcField.FieldDataType},

                CONSTRAINT FK_{ThemesTable.Name}_{UsersTable.Name} FOREIGN KEY ({ThemeUserIdField.SelectName}) REFERENCES {UsersTable.Name}({UserIdField.SelectName}) ON DELETE CASCADE,
                CONSTRAINT UQ_{ThemesTable.Name}_{ThemeUserIdField.Name}_{ThemeNameField.Name} UNIQUE ({ThemeUserIdField.SelectName}, {ThemeNameField.SelectName})
            );

            END
            ";
            command.ExecuteNonQuery();

            // LoginAttempts Table
            command.CommandText =
            $@"
            IF OBJECT_ID('dbo.{LoginsTable.Name}', 'U') IS NULL
            BEGIN

            CREATE TABLE [{LoginsTable.Name}] (
                {LoginIdField.SelectName} {LoginIdField.FieldDataType} PRIMARY KEY IDENTITY(1,1),
                {LoginUserNameField.SelectName} {LoginUserNameField.FieldDataType},
                {LoginAttemptTimeField.SelectName} {LoginAttemptTimeField.FieldDataType},
                {LoginStatusField.SelectName} {LoginStatusField.FieldDataType}
                
                CONSTRAINT CHK_{LoginsTable.Name}_{LoginStatusField.Name} CHECK ({LoginStatusField.SelectName} IN ('Success', 'Failure'))
            );

            END
            ";
            command.ExecuteNonQuery();

            // Sessions Table
            command.CommandText =
            $@"
            IF OBJECT_ID('dbo.{SessionsTable.Name}', 'U') IS NULL
            BEGIN

            CREATE TABLE [{SessionsTable.Name}] (
                {SessionIdField.SelectName} {SessionIdField.FieldDataType} PRIMARY KEY IDENTITY(1,1),
                {SessionUserIdField.SelectName} {SessionUserIdField.FieldDataType},
                {SessionLoginIdField.SelectName} {SessionLoginIdField.FieldDataType},
                
                {SessionTokenField.SelectName} {SessionTokenField.FieldDataType},
                {SessionIpIssuedToField.SelectName} {SessionIpIssuedToField.FieldDataType},
                {SessionIssuedAtUtcField.SelectName} {SessionIssuedAtUtcField.FieldDataType},
                {SessionExpiresAtUtcField.SelectName} {SessionExpiresAtUtcField.FieldDataType},
                {SessionRevokedField.SelectName} {SessionRevokedField.FieldDataType},

                CONSTRAINT FK_{SessionsTable.Name}_{UsersTable.Name} FOREIGN KEY ({SessionUserIdField.SelectName}) REFERENCES {UsersTable.Name}({UserIdField.SelectName}) ON DELETE CASCADE,
                CONSTRAINT FK_{SessionsTable.Name}_{LoginsTable.Name} FOREIGN KEY ({SessionLoginIdField.SelectName}) REFERENCES {LoginsTable.Name}({LoginIdField.SelectName}) ON DELETE CASCADE,
            );

            END
            ";
            command.ExecuteNonQuery();

            // WorkItems Table
            command.CommandText =
            $@"
            IF OBJECT_ID('dbo.{WorkItemsTable.Name}', 'U') IS NULL
            BEGIN

            CREATE TABLE [{WorkItemsTable.Name}] (
                {WorkItemIdField.SelectName} {WorkItemIdField.FieldDataType} PRIMARY KEY IDENTITY(1,1),
                {WorkItemUserIdField.SelectName} {WorkItemUserIdField.FieldDataType},
                
                {WorkItemNameField.SelectName} {WorkItemNameField.FieldDataType},
                {WorkItemDescriptionField.SelectName} {WorkItemDescriptionField.FieldDataType},
                {WorkItemPointsField.SelectName} {WorkItemPointsField.FieldDataType},
                {WorkItemTypeField.SelectName} {WorkItemTypeField.FieldDataType},

                CONSTRAINT FK_{WorkItemsTable.Name}_{UsersTable.Name} FOREIGN KEY ({WorkItemUserIdField.SelectName}) REFERENCES {UsersTable.Name}({UserIdField.SelectName}) ON DELETE CASCADE,
                CONSTRAINT CHK_{WorkItemsTable.Name}_{WorkItemTypeField.Name} CHECK ({WorkItemTypeField.SelectName} IN ('Task', 'Bug', 'Feature', 'Epic', 'Project')),
                CONSTRAINT UQ_{WorkItemsTable.Name}_{WorkItemUserIdField.Name}_{WorkItemNameField.Name} UNIQUE ({WorkItemUserIdField.SelectName}, {WorkItemNameField.SelectName}),
                CONSTRAINT CHK_{WorkItemsTable.Name}_{WorkItemPointsField.Name} CHECK ({WorkItemPointsField.SelectName} >= 0)
            );

            END
            ";
            command.ExecuteNonQuery();

            // WorkItemsParents Table
            command.CommandText =
            $@"
            IF OBJECT_ID('dbo.{WorkItemParentsTable.Name}', 'U') IS NULL
            BEGIN

            CREATE TABLE [{WorkItemParentsTable.Name}] (
                {WorkItemParentIdField.SelectName} {WorkItemParentIdField.FieldDataType},
                {WorkItemChildIdField.SelectName} {WorkItemChildIdField.FieldDataType},

                CONSTRAINT PK_{WorkItemParentsTable.Name} PRIMARY KEY ({WorkItemParentIdField.SelectName}, {WorkItemChildIdField.SelectName}),
                CONSTRAINT FK_{WorkItemParentsTable.Name}_{WorkItemsTable.Name}_Parent FOREIGN KEY ({WorkItemParentIdField.SelectName}) REFERENCES {WorkItemsTable.Name}({WorkItemIdField.SelectName}) ON DELETE CASCADE,
                CONSTRAINT FK_{WorkItemParentsTable.Name}_{WorkItemsTable.Name}_Child FOREIGN KEY ({WorkItemChildIdField.SelectName}) REFERENCES {WorkItemsTable.Name}({WorkItemIdField.SelectName}) ON DELETE NO ACTION
            );

            END
            ";
            command.ExecuteNonQuery();

            // WorkItemTags Table
            command.CommandText =
            $@"
            IF OBJECT_ID('dbo.{WorkItemTagsTable.Name}', 'U') IS NULL
            BEGIN

            CREATE TABLE [{WorkItemTagsTable.Name}] (
                {WorkItemTagIdField.SelectName} {WorkItemTagIdField.FieldDataType} PRIMARY KEY IDENTITY(1,1),
                {WorkItemGroupIdField.SelectName} {WorkItemGroupIdField.FieldDataType},

                {WorkItemTagNameField.SelectName} {WorkItemTagNameField.FieldDataType},
                {WorkItemTagColorField.SelectName} {WorkItemTagColorField.FieldDataType},

                CONSTRAINT FK_{WorkItemTagsTable.Name}_{GroupsTable.Name} FOREIGN KEY ({WorkItemGroupIdField.SelectName}) REFERENCES {GroupsTable.Name}({GroupIdField.SelectName}) ON DELETE CASCADE,
                CONSTRAINT UQ_{WorkItemTagsTable.Name}_{WorkItemGroupIdField.Name}_{WorkItemTagNameField.Name} UNIQUE ({WorkItemGroupIdField.SelectName}, {WorkItemTagNameField.SelectName}),
                CONSTRAINT CHK_{WorkItemTagsTable.Name}_{WorkItemTagColorField.Name} CHECK ({WorkItemTagColorField.SelectName} LIKE '#[0-9A-Fa-f]{{6}}')
            );

            END
            ";
            command.ExecuteNonQuery();

            // WorkItemTagAssignments Table
            command.CommandText =
            $@"
            IF OBJECT_ID('dbo.{WorkItemTagAssignmentsTable.Name}', 'U') IS NULL
            BEGIN

            CREATE TABLE [{WorkItemTagAssignmentsTable.Name}] (
                {WorkItemTagAssignmentTagItemIdField.SelectName} {WorkItemTagAssignmentTagItemIdField.FieldDataType},
                {WorkItemTagAssignmentTagIdField.SelectName} {WorkItemTagAssignmentTagIdField.FieldDataType},

                CONSTRAINT PK_{WorkItemTagAssignmentsTable.Name} PRIMARY KEY ({WorkItemTagAssignmentTagItemIdField.SelectName}, {WorkItemTagAssignmentTagIdField.SelectName}),
                CONSTRAINT FK_{WorkItemTagAssignmentsTable.Name}_{WorkItemsTable.Name} FOREIGN KEY ({WorkItemTagAssignmentTagItemIdField.SelectName}) REFERENCES {WorkItemsTable.Name}({WorkItemIdField.SelectName}) ON DELETE CASCADE,
                CONSTRAINT FK_{WorkItemTagAssignmentsTable.Name}_{WorkItemTagsTable.Name} FOREIGN KEY ({WorkItemTagAssignmentTagIdField.SelectName}) REFERENCES {WorkItemTagsTable.Name}({WorkItemTagIdField.SelectName}) ON DELETE CASCADE
            );

            END
            ";
            command.ExecuteNonQuery();
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
                @$"SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE, COLUMN_DEFAULT, NUMERIC_PRECISION, NUMERIC_SCALE 
                   FROM INFORMATION_SCHEMA.COLUMNS
                   WHERE TABLE_NAME = '{table.Name}'";
            try
            {
                using SqlDataReader reader = command.ExecuteReader();
                List<DatabaseField> fields = [];
                while (reader.Read())
                {
                    string? columnName = SqlHelper.GetNullableString(reader, "COLUMN_NAME");
                    string? dataType = SqlHelper.GetNullableString(reader, "DATA_TYPE");
                    string? maxLength = SqlHelper.GetNullableString(reader, "CHARACTER_MAXIMUM_LENGTH");
                    string? isNullable = SqlHelper.GetNullableString(reader, "IS_NULLABLE");
                    string? columnDefault = SqlHelper.GetNullableString(reader, "COLUMN_DEFAULT");
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

                    SqlDbType sqlType = SqlHelper.GetSqlDbType(dataType);

                    if (sqlType == SqlDbType.Decimal)
                    {
                        string? numericPrecision = SqlHelper.GetNullableString(reader, "NUMERIC_PRECISION");
                        string? numericScale = SqlHelper.GetNullableString(reader, "NUMERIC_SCALE");
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
                        Nullable = isNullable == "YES",
                        Reference = null // TODO: Add this
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
