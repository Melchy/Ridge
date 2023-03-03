using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ridge.ExceptionHandling;
using Ridge.HttpRequestFactoryMiddlewares.Internal;
using Ridge.LogWriter.Internal;
using Ridge.Parameters;
using Ridge.Parameters.AdditionalParams;
using Ridge.Response;
using Ridge.Setup;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge;

/// <summary>
///     Helpers used to simplify generated code.
/// </summary>
public class ApplicationClient
{
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly HttpRequestFactoryMiddlewareBuilder _httpRequestFactoryMiddlewareBuilder;
    private readonly HttpResponseCallFactory _httpResponseCallFactory;
    private readonly RidgeLogger _ridgeLogger;

    /// <summary>
    /// Create application client.
    /// </summary>
    /// <param name="httpClient">Http client used to call server.</param>
    /// <param name="serviceProvider">Application service provider.</param>
    public ApplicationClient(
        HttpClient httpClient,
        IServiceProvider serviceProvider)
    {
        _httpClient = httpClient;
        _serviceProvider = serviceProvider;

        var options = _serviceProvider.GetService<IOptions<RidgeOptions>>()?.Value;

        if (options == null)
        {
            throw new InvalidOperationException($"Ridge options not found. Did you forget to call {nameof(WebApplicationFactory<object>)}.{nameof(WebApplicationFactoryExtensions.WithRidge)}?" +
                                                $"To {nameof(ApplicationClient)} it is necessary to call {nameof(WebApplicationFactory<object>)}.{nameof(WebApplicationFactoryExtensions.WithRidge)} first.");
        }

        _httpRequestFactoryMiddlewareBuilder = _serviceProvider.GetRequiredService<HttpRequestFactoryMiddlewareBuilder>();
        _ridgeLogger = _serviceProvider.GetRequiredService<RidgeLogger>();
        _httpResponseCallFactory = _serviceProvider.GetRequiredService<HttpResponseCallFactory>();
    }
    
    /// <summary>
    ///     Creates http request for the given method and calls the server.
    /// </summary>
    /// <param name="methodName">Controller method for which the request will be generated.</param>
    /// <param name="callParameters">Argument types of controller method.</param>
    /// <param name="additionalParameters">Additional parameters passed to client.</param>
    /// <param name="parameterAndTransformationInfo">Information about parameters and transformations of parameters.</param>
    /// <typeparam name="TController">Controller to be called</typeparam>
    /// <typeparam name="TReturn">Return type</typeparam>
    /// <returns>Returns the response from server.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public async Task<HttpCallResponse<TReturn>> CallAction<TReturn, TController>(
        string methodName,
        Type[] callParameters,
        AdditionalParameter[] additionalParameters,
        IEnumerable<RawParameterAndTransformationInfo> parameterAndTransformationInfo)
    {
        var callId = Guid.NewGuid();
        var result = await CallActionCore<TController>(
            methodName,
            callId,
            callParameters,
            additionalParameters,
            parameterAndTransformationInfo);

        return await _httpResponseCallFactory.CreateControllerCallResult<TReturn>(result);
    }

    /// <summary>
    ///     Creates http request for the given method and calls the server.
    /// </summary>
    /// <param name="methodName">Controller method for which the request will be generated.</param>
    /// <param name="callParameters">Argument types of controller method.</param>
    /// <param name="additionalParameters">Additional parameters passed to client.</param>
    /// <param name="parameterAndTransformationInfo">Information about parameters and transformations of parameters.</param>
    /// <typeparam name="TController">Controller to be called</typeparam>
    /// <returns>Returns the response from server.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public async Task<HttpCallResponse> CallAction<TController>(
        string methodName,
        Type[] callParameters,
        AdditionalParameter[] additionalParameters,
        IEnumerable<RawParameterAndTransformationInfo> parameterAndTransformationInfo)
    {
        var callId = Guid.NewGuid();
        var result = await CallActionCore<TController>(
            methodName,
            callId,
            callParameters,
            additionalParameters,
            parameterAndTransformationInfo);

        return await _httpResponseCallFactory.CreateControllerCallResult(result);
    }


    /// <summary>
    ///     Creates http request for the given method and calls the server.
    /// </summary>
    /// <param name="methodName">Controller method for which the request will be generated.</param>
    /// <param name="callParameters">Argument types of controller method.</param>
    /// <param name="additionalParameters">Additional parameters passed to client.</param>
    /// <param name="parameterAndTransformationInfo">Information about parameters and transformations of parameters.</param>
    /// <typeparam name="TController">Controller to be called</typeparam>
    /// <returns>Returns the response from server.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public async Task<HttpResponseMessage> CallActionWithHttpResponseMessageResult<TController>(
        string methodName,
        Type[] callParameters,
        AdditionalParameter[] additionalParameters,
        IEnumerable<RawParameterAndTransformationInfo> parameterAndTransformationInfo)
    {
        var callId = Guid.NewGuid();
        var result = await CallActionCore<TController>(
            methodName,
            callId,
            callParameters,
            additionalParameters,
            parameterAndTransformationInfo);
        return result;
    }

    private async Task<HttpResponseMessage> CallActionCore<TController>(
        string methodName,
        Guid callId,
        Type[] actionParameters,
        AdditionalParameter[] additionalParameters,
        IEnumerable<RawParameterAndTransformationInfo> parameterAndTransformationInfo)
    {
        var controllerType = typeof(TController);
        var methodInfo = controllerType.GetMethod(methodName, actionParameters);
        if (methodInfo == null)
        {
            throw new InvalidOperationException($"Method with name {methodName} not found in class {controllerType.FullName}.");
        }

        var parameterProvider = new ParameterProvider(parameterAndTransformationInfo, additionalParameters.Where(x => x != null)!);

        var requestFactoryMiddleware = _httpRequestFactoryMiddlewareBuilder.BuildRequestFactoryMiddleware();

        var requestFactoryContext = new RequestFactoryContext(
            methodInfo,
            parameterProvider,
            callId);
 
        using var request = await requestFactoryMiddleware.CreateHttpRequest(requestFactoryContext);

        await _ridgeLogger.LogRequest(request);
        var response = await _httpClient.SendAsync(request);
        await _ridgeLogger.LogResponse(response);

        var exceptionManager = _serviceProvider.GetService<ExceptionManager>();
        if (exceptionManager != null)
        {
            exceptionManager.CheckIfExceptionOccuredAndThrowIfItDid(callId.ToString());
        }
        else
        {
            if (RidgeInstaller.WasInstallerUsed)
            {
                throw new InvalidOperationException(
                    $"You have used '{nameof(RidgeInstaller)}' and didn't set '{nameof(RidgeOptions)}.{nameof(RidgeOptions.ThrowExceptionInsteadOfReturning500)}' to true" +
                    $"which is invalid.  Please call '{nameof(RidgeOptions)}.{nameof(RidgeOptions.ThrowExceptionInsteadOfReturning500)}' to true and then use returned " +
                    $"'{nameof(WebApplicationFactory<object>)}' to create request.");
            }
        }
        
        return response;
    }
}
