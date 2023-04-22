using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace ApplicationWithDefaultSerialization;

public static class Startup
{
    public static void SetupServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo {Title = "ApplicationWithDefaultSerialization", Version = "v1"});
        });
    }

    public static void SetupPipeline(
        WebApplication app,
        IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApplicationWithDefaultSerialization v1"));
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();
    }

    public static void SetupEndpoints(
        IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllers();
    }
}
