using MoopelApi;

using Serilog;

Console.WriteLine("Starting...");

try
{
    Startup startup = new(args);

    await startup.CreateBuilder();

    WebApplication app = await startup.ConfigureApp();

    Console.WriteLine("Created app...");

    try
    {
        Console.WriteLine("Building Database...");
        startup.BuildDatabase(app.Services);
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "An error occurred while seeding the database");
        throw;
    }

    await startup.CheckApp(app.Services.CreateScope());

    Console.WriteLine("Running App...");

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
    throw;
}

