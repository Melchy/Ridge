using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ridge;
using Ridge.AspNetCore;
using Ridge.AspNetCore.ExceptionHandling;
using Ridge.AspNetCore.Options;
using Ridge.ExceptionHandling;
using Ridge.HttpRequestFactoryMiddlewares.Internal;
using Ridge.LogWriter.Internal;
using Ridge.Response;
using Ridge.Serialization;
using Ridge.Setup;
using System;

// Namespace is correct
namespace Microsoft.AspNetCore.Mvc.Testing;

/// <summary>
///     Ridge extensions for WebApplicationFactory.
/// </summary>
public static class WebApplicationFactoryExtensions
{
    /// <summary>
    ///     Add ridge dependencies and settings.
    /// </summary>
    /// <param name="webApplicationFactory">WebApplicationFactory which will be edited.</param>
    /// <param name="setupAction"></param>
    /// <typeparam name="TEntryPoint"></typeparam>
    /// <returns></returns>
    public static WebApplicationFactory<TEntryPoint> WithRidge<TEntryPoint>(
        this WebApplicationFactory<TEntryPoint> webApplicationFactory,
        Action<RidgeOptions>? setupAction = null)
        where TEntryPoint : class
    {
        setupAction ??= _ => { };
        
        var webApplicationFactoryEdited = webApplicationFactory.WithWebHostBuilder(x =>
        {
            x.ConfigureServices(x =>
            {
                x.Configure(setupAction);
                x.Configure<RidgeAspNetCoreOptions>(ridgeAspNetCoreOptions =>
                {
                    var ridgeOptions = new RidgeOptions();
                    setupAction(ridgeOptions);
                    ridgeAspNetCoreOptions.ExceptionRethrowFilter = ridgeOptions.ExceptionRethrowFilter;
                });

                x.AddSingleton<ExceptionManager>();
                x.AddSingleton<SerializerProvider>();
                x.AddSingleton<HttpRequestFactoryMiddlewareBuilder>();
                x.AddSingleton<HttpResponseCallFactory>();
                x.AddSingleton<RidgeLogger>();
                x.AddSingleton<IApplicationClientFactory, ApplicationClientFactory>();
            });

            x.UseSetting("Ridge:IsTestCall", "true");
        });

        return webApplicationFactoryEdited;
    }
}
