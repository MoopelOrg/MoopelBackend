using MoopelApi;
using MoopelApi.Helpers;
using MoopelApi.Models.Configuration;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder = await Startup.CreateBuilder(builder);

DeploymentInfo deploymentInfo = new()
{
    BuildDate = builder.Configuration["DEPLOYED_AT"] ?? "Date Not Found. Date Now: " + FormatHelper.FormatDate(DateTime.UtcNow),
    Environment = builder.Configuration["ASPNETCORE_ENVIRONMENT"] ?? "Unknown",
    Version = builder.Configuration["VERSION"] ?? "Unknown",
    CommitName = builder.Configuration["COMMIT_NAME"] ?? "Unknown",
    Tag = builder.Configuration["GIT_TAG"] ?? "Unknown"
};

WebApplication app = await Startup.ConfigureApp(builder, deploymentInfo);

DatabaseConnection databaseConnection = builder.Configuration
            .GetRequiredSection("DatabaseConnection")
            .Get<DatabaseConnection>()
            ?? throw new("Failed to retrieve DatabaseConnection configuration.");

try
{
    Startup.BuildDatabase(databaseConnection.ConnectionString);
}
catch (Exception ex)
{
    Log.Fatal(ex, "An error occurred while seeding the database");
    throw;
}

app.Run();
