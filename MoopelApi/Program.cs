using MoopelApi;
using Serilog;

Console.WriteLine("Starting...");
Startup startup = new(args);

await startup.CreateBuilder();
WebApplication app = await startup.ConfigureApp();

Console.WriteLine("Created app...");
try
{
    Console.WriteLine("Building Database...");
    startup.BuildDatabase();
}
catch (Exception ex)
{
    Log.Fatal(ex, "An error occurred while seeding the database");
    throw;
}


Console.WriteLine("Running App...");
app.Run();
