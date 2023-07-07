using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ridge.AspNetCore.Setup;

namespace TestWebApplication;

public static class Startup
{
    public static void SetupServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers()
           .ConfigureApiBehaviorOptions(options => options.SuppressInferBindingSourcesForParameters = true)
           .AddControllersAsServices()
           .AddNewtonsoftJson();
        services.AddTransient<object>();
        services.AddSwaggerGen();
    }

    public static void SetupPipeline(
        WebApplication app,
        IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        });

        if (app.WasApplicationCalledFromTestClient())
        {
            app.ThrowExceptionInsteadOfReturning500();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();
    }

    public static void SetupEndpoints(
        IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllers();
        endpoints.MapControllerRoute(
            name: "complexExample",
            "{controller}/ComplexExample/{fromRoute}/");
        endpoints.MapControllerRoute(
            name: "foo",
            "{controller}/{action}/");
    }
}
