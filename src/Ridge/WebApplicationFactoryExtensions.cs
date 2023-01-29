using Microsoft.Extensions.DependencyInjection;
using Ridge;
using Ridge.DelegationHandlers;
using Ridge.ExceptionHandling;
using Ridge.HttpRequestFactoryMiddlewares;
using Ridge.HttpRequestFactoryMiddlewares.Internal;
using Ridge.LogWriter;
using Ridge.LogWriter.Internal;
using Ridge.Parameters.CustomParams;
using Ridge.Serialization;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Xunit.Abstractions;

// Namespace is correct
namespace Microsoft.AspNetCore.Mvc.Testing;

// TODO zdokumentovat WebApplicationFactoryExtensions
// TODO zdokumentovat ze ridge atribut nemusi generovat return
// TODO zdokumentovat ze delegation handler ma specialni extension metodu pro ziskani contextu

/// <summary>
///     Ridge extensions for WebApplicationFactory.
/// </summary>
public static class WebApplicationFactoryExtensions
{
    /// <summary>
    ///     Add ridge dependencies to application.
    /// </summary>
    public static WebApplicationFactory<TEntryPoint> AddExceptionCatching<TEntryPoint>(
        this WebApplicationFactory<TEntryPoint> webApplicationFactory)
        where TEntryPoint : class
    {
        return webApplicationFactory.WithWebHostBuilder(x =>
        {
            x.ConfigureServices(x =>
            {
                x.AddSingleton<ExceptionManager>();
            });

            x.UseSetting("Ridge:IsTestCall", "true");
        });
    }


    /// <summary>
    ///     Create client which can be used to call application using RidgeCallers.
    /// </summary>
    /// <returns><see cref="RidgeHttpClient" /> which can be used in RidgeCallers.</returns>
    public static RidgeHttpClient CreateRidgeClient<TEntryPoint>(
        this WebApplicationFactory<TEntryPoint> webApplicationFactory,
        WebApplicationFactoryClientOptions? clientOptions = null,
        params DelegatingHandler[] delegatingHandlers)
        where TEntryPoint : class
    {
        var delegationHandlersList = delegatingHandlers.ToList();
        if (clientOptions != null)
        {
            var createHandlersMethod = clientOptions.GetType()
               .GetMethod("CreateHandlers", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (createHandlersMethod == null)
            {
                throw new InvalidOperationException($"Internal method CreateHandlers on {clientOptions.GetType()} not found");
            }

            var delegationHandlersFromOptions = (DelegatingHandler[])createHandlersMethod.Invoke(clientOptions, Array.Empty<object>())!;
            delegationHandlersList.AddRange(delegationHandlersFromOptions);
        }

        var logWriter = webApplicationFactory.Services.GetService<ILogWriter>();
        if (logWriter != null)
        {
            delegationHandlersList.Add(new LogRequestDelegationHandler(logWriter));
        }

        var client = webApplicationFactory.CreateDefaultClient(
            clientOptions?.BaseAddress ?? new WebApplicationFactoryClientOptions().BaseAddress,
            delegationHandlersList.ToArray());
        var httpRequestFactoryMiddlewareBuilder = new HttpRequestFactoryMiddlewareBuilder();
        httpRequestFactoryMiddlewareBuilder.AddHttpRequestFactoryMiddlewares(webApplicationFactory.Services.GetServices<HttpRequestFactoryMiddleware>());
        httpRequestFactoryMiddlewareBuilder.AddHeaders(webApplicationFactory.Services.GetServices<HttpHeaderParameter>());

        return new RidgeHttpClient(client,
            webApplicationFactory.Services.GetService<IRequestResponseSerializer>(),
            webApplicationFactory.Services,
            httpRequestFactoryMiddlewareBuilder);
    }


    /// <summary>
    ///     Add one or more headers to the requests. This method actually adds <see cref="HttpRequestFactoryMiddleware" />
    ///     which then adds the header to requests.
    /// </summary>
    /// <param name="webApplicationFactory">WebApplication factory.</param>
    /// <param name="headers">Headers to add.</param>
    /// <returns>Returns this.</returns>
    public static WebApplicationFactory<TEntryPoint> AddHeader<TEntryPoint>(
        this WebApplicationFactory<TEntryPoint> webApplicationFactory,
        params HttpHeaderParameter[] headers)
        where TEntryPoint : class
    {
        return webApplicationFactory.WithWebHostBuilder(x =>
        {
            x.ConfigureServices(x =>
            {
                foreach (var header in headers)
                {
                    x.AddSingleton(header);
                }
            });
        });
    }

    /// <summary>
    ///     Add one or many <see cref="HttpRequestFactoryMiddleware" /> which will be later used in pipeline to create
    ///     HttpRequestMessage.
    /// </summary>
    /// <param name="webApplicationFactory">WebApplication factory.</param>
    /// <param name="httpRequestFactoryMiddlewares"><see cref="HttpRequestFactoryMiddleware" />  to add.</param>
    /// <returns>Returns this.</returns>
    public static WebApplicationFactory<TEntryPoint> AddHttpRequestFactoryMiddleware<TEntryPoint>(
        this WebApplicationFactory<TEntryPoint> webApplicationFactory,
        params HttpRequestFactoryMiddleware[] httpRequestFactoryMiddlewares)
        where TEntryPoint : class
    {
        return webApplicationFactory.WithWebHostBuilder(x =>
        {
            x.ConfigureServices(x =>
            {
                foreach (var httpRequestFactoryMiddleware in httpRequestFactoryMiddlewares)
                {
                    x.AddSingleton(httpRequestFactoryMiddleware);
                }
            });
        });
    }

    /// <summary>
    ///     Add Nunit logger which will be used to log requests and responses generated by ridge.
    ///     This logger uses TestContext.Out.WriteLine. See
    ///     https://docs.nunit.org/articles/nunit/writing-tests/TestContext.html#out.
    /// </summary>
    /// <returns>Returns this.</returns>
    public static WebApplicationFactory<TEntryPoint> AddNUnitLogger<TEntryPoint>(
        this WebApplicationFactory<TEntryPoint> webApplicationFactory)
        where TEntryPoint : class
    {
        return webApplicationFactory.WithWebHostBuilder(x =>
        {
            x.ConfigureServices(x =>
            {
                x.AddSingleton<ILogWriter>(new NunitLogWriter());
            });
        });
    }

    /// <summary>
    ///     Add XUnit logger which will be used to log requests and responses generated by ridge.
    ///     To read more about <see cref="ITestOutputHelper" /> see documentation https://xunit.net/docs/capturing-output.
    /// </summary>
    /// <param name="webApplicationFactory">WebApplication factory.</param>
    /// <param name="testOutputHelper">
    ///     To read more about <see cref="ITestOutputHelper" /> see documentation
    ///     https://xunit.net/docs/capturing-output.
    /// </param>
    /// <returns>Returns this.</returns>
    public static WebApplicationFactory<TEntryPoint> AddXUnitLogger<TEntryPoint>(
        this WebApplicationFactory<TEntryPoint> webApplicationFactory,
        ITestOutputHelper testOutputHelper)
        where TEntryPoint : class
    {
        return webApplicationFactory.WithWebHostBuilder(x =>
        {
            x.ConfigureServices(x =>
            {
                x.AddSingleton<ILogWriter>(new XunitLogWriter(testOutputHelper));
            });
        });
    }

    /// <summary>
    ///     Add custom logger implementation which will be used to log requests and responses generated by ridge.
    /// </summary>
    /// <param name="webApplicationFactory">WebApplication factory.</param>
    /// <param name="logWriter">Custom <see cref="ILogWriter" />.</param>
    /// <returns>Returns this.</returns>
    public static WebApplicationFactory<TEntryPoint> AddCustomLogger<TEntryPoint>(
        this WebApplicationFactory<TEntryPoint> webApplicationFactory,
        ILogWriter logWriter)
        where TEntryPoint : class
    {
        return webApplicationFactory.WithWebHostBuilder(x =>
        {
            x.ConfigureServices(x =>
            {
                x.AddSingleton(logWriter);
            });
        });
    }

    /// <summary>
    ///     Add custom request response serializer. If no serializer is provided then
    ///     ridge uses Json.Net or System.Text.Json based on your application settings.
    ///     If Ridge can not determine which serializer your application uses then
    ///     System.Text.Json is used by default.
    /// </summary>
    /// <param name="webApplicationFactory">WebApplication factory.</param>
    /// <param name="requestResponseSerializer">Serializer which will be used.</param>
    /// <returns>Returns this.</returns>
    public static WebApplicationFactory<TEntryPoint> AddCustomRequestResponseSerializer<TEntryPoint>(
        this WebApplicationFactory<TEntryPoint> webApplicationFactory,
        IRequestResponseSerializer requestResponseSerializer)
        where TEntryPoint : class
    {
        return webApplicationFactory.WithWebHostBuilder(x =>
        {
            x.ConfigureServices(x =>
            {
                x.AddSingleton(requestResponseSerializer);
            });
        });
    }
}
