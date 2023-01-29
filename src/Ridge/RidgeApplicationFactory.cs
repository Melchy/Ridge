using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Ridge.DelegationHandlers;
using Ridge.ExceptionHandling;
using Ridge.HttpRequestFactoryMiddlewares;
using Ridge.HttpRequestFactoryMiddlewares.Internal;
using Ridge.LogWriter;
using Ridge.LogWriter.Internal;
using Ridge.Parameters.CustomParams;
using Ridge.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Xunit.Abstractions;

namespace Ridge;

/// <summary>
///     Custom <see cref="WebApplicationFactory{TEntryPoint}" /> which is used to setup dependencies
///     needed by Ridge.
/// </summary>
/// <typeparam name="TEntryPoint">
///     A type in the entry point assembly of the application.
///     Typically the Startup or Program classes can be used.
/// </typeparam>
public class RidgeApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    private readonly WebApplicationFactory<TEntryPoint> _webApplicationFactory;
    private ILogWriter? _logWriter;
    private IRequestResponseSerializer? _ridgeSerializer;
    private readonly List<DelegatingHandler> _delegatingHandlers = new();
    private WebApplicationFactoryClientOptions? _webApplicationFactoryClientOptions;
    private HttpRequestFactoryMiddlewareBuilder HttpRequestFactoryMiddlewareBuilder { get; } = new();

    /// <summary>
    ///     Create ridge application factory.
    /// </summary>
    public RidgeApplicationFactory()
    {
        _webApplicationFactory = RegisterDependencies(this);
    }

    /// <summary>
    ///     Create ridge application factory using existing <see cref="WebApplicationFactory{TEntryPoint}" />
    /// </summary>
    /// <param name="webApplicationFactory">WebApplicationFactory used to call application.</param>
    public RidgeApplicationFactory(
        WebApplicationFactory<TEntryPoint> webApplicationFactory)
    {
        _webApplicationFactory = RegisterDependencies(webApplicationFactory);
    }

    /// <summary>
    ///     Add ridge dependencies to application.
    /// </summary>
    private static WebApplicationFactory<TEntryPoint> RegisterDependencies(
        WebApplicationFactory<TEntryPoint> webApplicationFactory)
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
    public RidgeHttpClient CreateRidgeClient()
    {
        var delegationHandlersList = _delegatingHandlers.ToList();
        if (_webApplicationFactoryClientOptions != null)
        {
            var createHandlersMethod = _webApplicationFactoryClientOptions.GetType()
               .GetMethod("CreateHandlers", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (createHandlersMethod == null)
            {
                throw new InvalidOperationException($"Internal method CreateHandlers on {_webApplicationFactoryClientOptions.GetType()} not found");
            }

            var delegationHandlersFromOptions = (DelegatingHandler[])createHandlersMethod.Invoke(_webApplicationFactoryClientOptions, Array.Empty<object>())!;
            delegationHandlersList.AddRange(delegationHandlersFromOptions);
        }

        if (_logWriter != null)
        {
            delegationHandlersList.Add(new LogRequestDelegationHandler(_logWriter));
        }

        var client = _webApplicationFactory.CreateDefaultClient(
            _webApplicationFactoryClientOptions?.BaseAddress ?? new WebApplicationFactoryClientOptions().BaseAddress,
            delegationHandlersList.ToArray());
        return new RidgeHttpClient(client, _ridgeSerializer, _webApplicationFactory.Services, HttpRequestFactoryMiddlewareBuilder.CreateNewBuilderByCopyingExisting());
    }

    /// <summary>
    ///     Customize WebApplicationFactoryHttpClient by WebApplicationFactoryClientOptions.
    /// </summary>
    /// <param name="webApplicationFactoryClientOptions">Options used to configure WebApplicationFactoryHttpClient.</param>
    /// <returns>Returns this.</returns>
    public RidgeApplicationFactory<TEntryPoint> SetHttpClientOptions(
        WebApplicationFactoryClientOptions webApplicationFactoryClientOptions)
    {
        _webApplicationFactoryClientOptions = webApplicationFactoryClientOptions;
        return this;
    }

    /// <summary>
    ///     Add <see cref="DelegatingHandler" /> to httpClient.
    /// </summary>
    /// <param name="delegatingHandler"><see cref="DelegatingHandler" /> to add.</param>
    /// <returns>Returns this.</returns>
    public RidgeApplicationFactory<TEntryPoint> AddDelegationHandler(
        params DelegatingHandler[] delegatingHandler)
    {
        _delegatingHandlers.AddRange(delegatingHandler);
        return this;
    }

    /// <summary>
    ///     Add one or more headers to the requests. This method actually adds <see cref="HttpRequestFactoryMiddleware" />
    ///     which then adds the header to requests.
    /// </summary>
    /// <param name="headers">Headers to add.</param>
    /// <returns>Returns this.</returns>
    public RidgeApplicationFactory<TEntryPoint> AddHeader(
        params HttpHeaderParameter[] headers)
    {
        HttpRequestFactoryMiddlewareBuilder.AddHeaders(headers);
        return this;
    }

    /// <summary>
    ///     Add one or many <see cref="HttpRequestFactoryMiddleware" /> which will be later used in pipeline to create
    ///     HttpRequestMessage.
    /// </summary>
    /// <param name="httpRequestFactoryMiddlewares"><see cref="HttpRequestFactoryMiddleware" />  to add.</param>
    /// <returns>Returns this.</returns>
    public RidgeApplicationFactory<TEntryPoint> AddHttpRequestFactoryMiddleware(
        params HttpRequestFactoryMiddleware[] httpRequestFactoryMiddlewares)
    {
        HttpRequestFactoryMiddlewareBuilder.AddHttpRequestFactoryMiddlewares(httpRequestFactoryMiddlewares);
        return this;
    }

    /// <summary>
    ///     Add Nunit logger which will be used to log requests and responses generated by ridge.
    ///     This logger uses TestContext.Out.WriteLine. See
    ///     https://docs.nunit.org/articles/nunit/writing-tests/TestContext.html#out.
    /// </summary>
    /// <returns>Returns this.</returns>
    public RidgeApplicationFactory<TEntryPoint> AddNUnitLogger()
    {
        _logWriter = new NunitLogWriter();
        return this;
    }

    /// <summary>
    ///     Add XUnit logger which will be used to log requests and responses generated by ridge.
    ///     To read more about <see cref="ITestOutputHelper" /> see documentation https://xunit.net/docs/capturing-output.
    /// </summary>
    /// <param name="testOutputHelper">
    ///     To read more about <see cref="ITestOutputHelper" /> see documentation
    ///     https://xunit.net/docs/capturing-output.
    /// </param>
    /// <returns>Returns this.</returns>
    public RidgeApplicationFactory<TEntryPoint> AddXUnitLogger(
        ITestOutputHelper testOutputHelper)
    {
        _logWriter = new XunitLogWriter(testOutputHelper);
        return this;
    }

    /// <summary>
    ///     Add custom logger implementation which will be used to log requests and responses generated by ridge.
    /// </summary>
    /// <param name="logWriter">Custom <see cref="ILogWriter" />.</param>
    /// <returns>Returns this.</returns>
    public RidgeApplicationFactory<TEntryPoint> AddCustomLogger(
        ILogWriter logWriter)
    {
        _logWriter = logWriter;
        return this;
    }

    /// <summary>
    ///     Add custom request response serializer. If no serializer is provided then
    ///     ridge uses Json.Net or System.Text.Json based on your application settings.
    ///     If Ridge can not determine which serializer your application uses then
    ///     System.Text.Json is used by default.
    /// </summary>
    /// <param name="requestResponseSerializer"></param>
    /// <returns>Returns this.</returns>
    public RidgeApplicationFactory<TEntryPoint> AddCustomRequestResponseSerializer(
        IRequestResponseSerializer requestResponseSerializer)
    {
        _ridgeSerializer = requestResponseSerializer;
        return this;
    }
}
