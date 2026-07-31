using MoopelApi;

using Serilog;

try
{
    Startup startup = new(args);

    await startup.CreateBuilder();

    WebApplication app = await startup.ConfigureApp();

    try
    {
        Log.Information("Building Database...");
        startup.BuildDatabase(app.Services);
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "An error occurred while seeding the database");
        throw;
    }

    await startup.CheckApp(app.Services.CreateScope());

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
    throw;
}

