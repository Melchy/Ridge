using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Ridge.ExceptionHandling;
using Ridge.HttpRequestFactoryMiddlewares.Internal;
using Ridge.Parameters;
using Ridge.Parameters.CustomParams;
using Ridge.Response;
using Ridge.Serialization;
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
public class ApplicationCaller
{
    private readonly RidgeHttpClient _ridgeHttpClient;

    /// <summary>
    /// Create application caller.
    /// </summary>
    /// <param name="ridgeHttpClient">Http client used to call server.</param>
    public ApplicationCaller(
        RidgeHttpClient ridgeHttpClient)
    {
        _ridgeHttpClient = ridgeHttpClient;
    }
    
    /// <summary>
    ///     Creates http request for the given method and calls the server.
    /// </summary>
    /// <param name="methodName">Controller method for which the request will be generated.</param>
    /// <param name="callParameters">Argument types of controller method.</param>
    /// <param name="customParameters">Custom parameters passed to caller.</param>
    /// <param name="parameterAndTransformationInfo">Information about parameters and transformations of parameters.</param>
    /// <typeparam name="TController">Controller to be called</typeparam>
    /// <typeparam name="TReturn">Return type</typeparam>
    /// <returns>Returns the response from server.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public async Task<HttpCallResponse<TReturn>> CallAction<TReturn, TController>(
        string methodName,
        Type[] callParameters,
        CustomParameter[] customParameters,
        IEnumerable<RawParameterAndTransformationInfo> parameterAndTransformationInfo)
    {
        var callId = Guid.NewGuid();
        var result = await CallActionCore<TController>(
            methodName,
            callId,
            callParameters,
            customParameters,
            parameterAndTransformationInfo);

        var serializer = SerializerProvider.GetSerializer(_ridgeHttpClient.ServiceProvider, _ridgeHttpClient.RequestResponseSerializer);
        var httpResponseCallFactory = new HttpResponseCallFactory(serializer);
        return await httpResponseCallFactory.CreateControllerCallResult<TReturn>(result);
    }

    /// <summary>
    ///     Creates http request for the given method and calls the server.
    /// </summary>
    /// <param name="methodName">Controller method for which the request will be generated.</param>
    /// <param name="callParameters">Argument types of controller method.</param>
    /// <param name="customParameters">Custom parameters passed to caller.</param>
    /// <param name="parameterAndTransformationInfo">Information about parameters and transformations of parameters.</param>
    /// <typeparam name="TController">Controller to be called</typeparam>
    /// <returns>Returns the response from server.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public async Task<HttpCallResponse> CallAction<TController>(
        string methodName,
        Type[] callParameters,
        CustomParameter[] customParameters,
        IEnumerable<RawParameterAndTransformationInfo> parameterAndTransformationInfo)
    {
        var callId = Guid.NewGuid();
        var result = await CallActionCore<TController>(
            methodName,
            callId,
            callParameters,
            customParameters,
            parameterAndTransformationInfo);
        var serializer = SerializerProvider.GetSerializer(_ridgeHttpClient.ServiceProvider, _ridgeHttpClient.RequestResponseSerializer);
        var httpResponseCallFactory = new HttpResponseCallFactory(serializer);
        return await httpResponseCallFactory.CreateControllerCallResult(result);
    }


    /// <summary>
    ///     Creates http request for the given method and calls the server.
    /// </summary>
    /// <param name="methodName">Controller method for which the request will be generated.</param>
    /// <param name="callParameters">Argument types of controller method.</param>
    /// <param name="customParameters">Custom parameters passed to caller.</param>
    /// <param name="parameterAndTransformationInfo">Information about parameters and transformations of parameters.</param>
    /// <typeparam name="TController">Controller to be called</typeparam>
    /// <returns>Returns the response from server.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public async Task<HttpResponseMessage> CallActionWithHttpResponseMessageResult<TController>(
        string methodName,
        Type[] callParameters,
        CustomParameter[] customParameters,
        IEnumerable<RawParameterAndTransformationInfo> parameterAndTransformationInfo)
    {
        var callId = Guid.NewGuid();
        var result = await CallActionCore<TController>(
            methodName,
            callId,
            callParameters,
            customParameters,
            parameterAndTransformationInfo);
        return result;
    }

    private async Task<HttpResponseMessage> CallActionCore<TController>(
        string methodName,
        Guid callId,
        Type[] actionParameters,
        CustomParameter[] customParameters,
        IEnumerable<RawParameterAndTransformationInfo> parameterAndTransformationInfo)
    {
        var controllerType = typeof(TController);
        var methodInfo = controllerType.GetMethod(methodName, actionParameters);
        if (methodInfo == null)
        {
            throw new InvalidOperationException($"Method with name {methodName} not found in class {controllerType.FullName}.");
        }

        var serializer = SerializerProvider.GetSerializer(_ridgeHttpClient.ServiceProvider, _ridgeHttpClient.RequestResponseSerializer);
        var parameterProvider = new ParameterProvider(parameterAndTransformationInfo, customParameters.Where(x => x != null)!);

        var requestFactoryMiddleware = _ridgeHttpClient.HttpRequestFactoryMiddlewareBuilder.BuildRequestFactoryMiddleware(
            _ridgeHttpClient.ServiceProvider.GetRequiredService<IActionDescriptorCollectionProvider>(),
            _ridgeHttpClient.ServiceProvider.GetRequiredService<LinkGenerator>(),
            serializer
        );

        var requestFactoryContext = new RequestFactoryContext(
            methodInfo,
            parameterProvider,
            callId);
        using var request = await requestFactoryMiddleware.CreateHttpRequest(requestFactoryContext);

        var response = await _ridgeHttpClient.HttpClient.SendAsync(request);

        var exceptionManager = _ridgeHttpClient.ServiceProvider.GetRequiredService<ExceptionManager>();
        exceptionManager.CheckIfExceptionOccuredAndThrowIfItDid(callId.ToString());
        return response;
    }
}
