using Microsoft.AspNetCore.Builder;

namespace TestWebApplication;

public class Program
{
    private static void Main(
        string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        Startup.SetupServices(builder.Services, builder.Configuration);
        var app = builder.Build();
        Startup.SetupPipeline(app, app.Environment);
        Startup.SetupEndpoints(app);
        app.Run();
    }
}
