using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Ridge.DelegationHandlers;
using Ridge.ExceptionHandling;
using Ridge.HttpRequestFactoryMiddlewares;
using Ridge.LogWriter;
using Ridge.Response;
using Ridge.Serialization;
using Ridge.WebApplicationFactoryTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Ridge;

/// <summary>
///     Helpers used to simplify generated code.
/// </summary>
public class ApplicationCaller<TEntryPoint>
    where TEntryPoint : class
{
    private readonly ILogWriter? _logWriter;
    private readonly IRequestResponseSerializer? _ridgeSerializer;
    private WebAppFactoryClientOptions? _webApplicationFactoryClientOptions;
    private WebApplicationFactory<TEntryPoint> _webApplicationFactory { get; set; }
    private IServiceProvider _serviceProvider => _webApplicationFactory.Services;
    private HttpRequestFactoryMiddlewareBuilder HttpRequestFactoryMiddlewareBuilder { get; } = new();

    private readonly List<DelegatingHandler> _delegatingHandlers = new();

    /// <summary>
    ///     Http client used to call Application. When this property is called for the first time the client is
    ///     generated using <see cref="WebApplicationFactory{TEntryPoint}" />.
    /// </summary>
    public Lazy<HttpClient> HttpClient => new(CreateClient);

    /// <summary>
    ///     Create <see cref="ApplicationCaller{TEntryPoint}" />.
    /// </summary>
    /// <param name="webApplicationFactory">
    ///     Pass WebApplicationFactory{TEntryPoint} to this parameter. This parameter is object because generator can not
    ///     ensure that containing assembly
    ///     references correct nuget package
    /// </param>
    /// <param name="logWriter">
    ///     Used to log requests and responses from server.
    ///     Use <see cref="XunitLogWriter" /> or <see cref="NunitLogWriter" /> or implement custom <see cref="ILogWriter" />
    /// </param>
    /// <param name="ridgeSerializer">
    ///     Serializer used to serialize and deserialize requests.
    ///     Serializer is by default chosen based on asp.net settings. If you need custom serializer implement
    ///     <see cref="IRequestResponseSerializer" />.
    /// </param>
    public ApplicationCaller(
        WebApplicationFactory<TEntryPoint> webApplicationFactory,
        ILogWriter? logWriter = null,
        IRequestResponseSerializer? ridgeSerializer = null)
    {
        _logWriter = logWriter;
        _ridgeSerializer = ridgeSerializer;
        _webApplicationFactory = webApplicationFactory;
        RegisterDependencies();
    }

    /// <summary>
    ///     Add ridge dependencies to application.
    /// </summary>
    private void RegisterDependencies()
    {
        _webApplicationFactory = _webApplicationFactory.WithWebHostBuilder(x =>
        {
            x.ConfigureServices(x =>
            {
                x.AddSingleton<ExceptionManager>();
            });

            x.UseSetting("Ridge:IsTestCall", "true");
        });
    }

    private HttpClient CreateClient()
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

        return _webApplicationFactory.CreateDefaultClient(
            _webApplicationFactoryClientOptions?.BaseAddress ?? new WebApplicationFactoryClientOptions().BaseAddress,
            delegationHandlersList.ToArray());
    }

    /// <summary>
    ///     Creates http request for the given method and calls the server.
    /// </summary>
    /// <param name="arguments">Arguments of controller method.</param>
    /// <param name="methodName">Controller method for which the request will be generated.</param>
    /// <param name="callParameters">Argument types of controller method.</param>
    /// <param name="customParameters">Custom parameters passed to caller.</param>
    /// <typeparam name="TController">Controller to be called</typeparam>
    /// <typeparam name="TReturn">Return type</typeparam>
    /// <returns>Returns the response from server.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public async Task<HttpCallResponse<TReturn>> CallAction<TReturn, TController>(
        IEnumerable<object?> arguments,
        string methodName,
        Type[] callParameters,
        object?[] customParameters)
    {
        var callId = Guid.NewGuid();
        var result = await CallActionCore<TController>(arguments,
            methodName,
            callId,
            callParameters,
            customParameters);

        var serializer = SerializerProvider.GetSerializer(_serviceProvider, _ridgeSerializer);
        var httpResponseCallFactory = new HttpResponseCallFactory(serializer);
        return await httpResponseCallFactory.CreateControllerCallResult<TReturn>(result, callId.ToString());
    }

    /// <summary>
    ///     Creates http request for the given method and calls the server.
    /// </summary>
    /// <param name="arguments">Arguments of controller method.</param>
    /// <param name="methodName">Controller method for which the request will be generated.</param>
    /// <param name="callParameters">Argument types of controller method.</param>
    /// <param name="customParameters">Custom parameters passed to caller.</param>
    /// <typeparam name="TController">Controller to be called</typeparam>
    /// <returns>Returns the response from server.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public async Task<HttpCallResponse> CallAction<TController>(
        IEnumerable<object?> arguments,
        string methodName,
        Type[] callParameters,
        object?[] customParameters)
    {
        var callId = Guid.NewGuid();
        var result = await CallActionCore<TController>(arguments,
            methodName,
            callId,
            callParameters,
            customParameters);
        var serializer = SerializerProvider.GetSerializer(_serviceProvider, _ridgeSerializer);
        var httpResponseCallFactory = new HttpResponseCallFactory(serializer);
        return await httpResponseCallFactory.CreateControllerCallResult(result, callId.ToString());
    }


    /// <summary>
    ///     Creates http request for the given method and calls the server.
    /// </summary>
    /// <param name="arguments">Arguments of controller method.</param>
    /// <param name="methodName">Controller method for which the request will be generated.</param>
    /// <param name="callParameters">Argument types of controller method.</param>
    /// <param name="customParameters">Custom parameters passed to caller.</param>
    /// <typeparam name="TController">Controller to be called</typeparam>
    /// <returns>Returns the response from server.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public async Task<HttpResponseMessage> CallActionWithHttpResponseMessageResult<TController>(
        IEnumerable<object?> arguments,
        string methodName,
        Type[] callParameters,
        object?[] customParameters)
    {
        var callId = Guid.NewGuid();
        var result = await CallActionCore<TController>(arguments,
            methodName,
            callId,
            callParameters,
            customParameters);
        return result;
    }

    private async Task<HttpResponseMessage> CallActionCore<TController>(
        IEnumerable<object?> arguments,
        string methodName,
        Guid callId,
        Type[] callParameters,
        object?[] customParameters)
    {
        var httpRequestFactoryMiddlewareBuilder = HttpRequestFactoryMiddlewareBuilder.CreateNewBuilderByCopyingExisting();
        var controllerType = typeof(TController);
        var methodInfo = controllerType.GetMethod(methodName, callParameters);
        if (methodInfo == null)
        {
            throw new InvalidOperationException($"Method with name {methodName} not found in class {controllerType.FullName}.");
        }

        var serializer = SerializerProvider.GetSerializer(_serviceProvider, _ridgeSerializer);
        var customParametersProvider = new CustomParameterProvider(customParameters);

        var requestFactoryMiddleware = httpRequestFactoryMiddlewareBuilder.BuildRequestFactoryMiddleware(
            _serviceProvider.GetRequiredService<IActionDescriptorCollectionProvider>(),
            _serviceProvider.GetRequiredService<LinkGenerator>(),
            serializer
        );

        var requestFactoryContext = new RequestFactoryContext(customParametersProvider,
            methodInfo,
            arguments,
            callId);
        using var request = await requestFactoryMiddleware.CreateHttpRequest(requestFactoryContext);

        var response = await HttpClient.Value.SendAsync(request);

        var exceptionManager = _serviceProvider.GetRequiredService<ExceptionManager>();
        exceptionManager.CheckIfExceptionOccuredAndThrowIfItDid(callId.ToString());
        return response;
    }

    /// <summary>
    ///     Customize WebApplicationFactoryHttpClient by WebApplicationFactoryClientOptions.
    /// </summary>
    /// <param name="webApplicationFactoryClientOptions">Options used to configure WebApplicationFactoryHttpClient.</param>
    public void SetHttpClientOptions(
        WebAppFactoryClientOptions webApplicationFactoryClientOptions)
    {
        _webApplicationFactoryClientOptions = webApplicationFactoryClientOptions;
    }

    /// <summary>
    ///     Add <see cref="DelegatingHandler" /> to httpClient.
    /// </summary>
    /// <param name="delegatingHandler"><see cref="DelegatingHandler" /> to add.</param>
    public void AddDelegationHandler(
        params DelegatingHandler[] delegatingHandler)
    {
        _delegatingHandlers.AddRange(delegatingHandler);
    }

    /// <summary>
    ///     Add one or more headers to the requests. This method actually adds <see cref="HttpRequestFactoryMiddleware" />
    ///     which then adds the header to requests.
    /// </summary>
    /// <param name="headers">Headers to add.</param>
    public void AddHeader(
        params HttpHeader[] headers)
    {
        HttpRequestFactoryMiddlewareBuilder.AddHeaders(headers);
    }

    /// <summary>
    ///     Add one or many <see cref="HttpRequestFactoryMiddleware" /> which will be later used in pipeline to create
    ///     HttpRequestMessage.
    /// </summary>
    /// <param name="httpRequestFactoryMiddlewares"><see cref="HttpRequestFactoryMiddleware" />  to add.</param>
    public void AddHttpRequestFactoryMiddleware(
        params HttpRequestFactoryMiddleware[] httpRequestFactoryMiddlewares)
    {
        HttpRequestFactoryMiddlewareBuilder.AddHttpRequestFactoryMiddlewares(httpRequestFactoryMiddlewares);
    }
}
