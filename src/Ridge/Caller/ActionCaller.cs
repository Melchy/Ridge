using Ridge.ActionInfo;
using Ridge.Interceptor;
using Ridge.Pipeline;
using Ridge.Pipeline.Public;
using Ridge.Response;
using Ridge.Serialization;
using Ridge.Transformers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Ridge.Caller;

/// <summary>
///     This call is used to create http request and call server.
///     The request is created from method info and method arguments.
/// </summary>
public class ActionCaller
{
    private readonly WebCaller _webCaller = null!;
    private readonly ActionInfoProvider _actionInfoProvider = null!;
    private readonly IHttpResponseCallFactory _httpResponseCallFactory = null!;
    private readonly ActionInfoTransformersCaller _actionInfoTransformersCaller = null!;
    private readonly IRequestResponseSerializer _serializer = null!;

    
    /// <summary>
    /// Create <see cref="ActionCaller" />.
    /// </summary>
    public ActionCaller()
    {
    }

    /// <summary>
    /// Creates http request for the given method and calls the server.
    /// </summary>
    /// <param name="arguments">Arguments of controller method.</param>
    /// <param name="methodName">Controller method for which the request will be generated.</param>
    /// <param name="controllerCaller">Provides information about caller settings.</param>
    /// <param name="callParameters">Argument types of controller method.</param>
    /// <param name="headers">Headers which will be added to the request</param>
    /// <param name="authenticationHeaderValue">Authentication headers which will be added to request</param>
    /// <param name="actionInfoTransformers">Action transformers which will be used for the request</param>
    /// <param name="httpRequestPipelineParts">HttpPipeline parts which will be used for the request</param>
    /// <typeparam name="TController">Controller to be called</typeparam>
    /// <typeparam name="TReturn">Return type</typeparam>
    /// <returns>Returns the response from server.</returns>
    public async Task<HttpCallResponse<TReturn>> CallAction<TReturn, TController>(
        IEnumerable<object?> arguments,
        string methodName,
        IControllerCaller controllerCaller,
        Type[] callParameters,
        IEnumerable<(string Key, string? Value)>? headers = null,
        AuthenticationHeaderValue? authenticationHeaderValue = null,
        IEnumerable<IActionInfoTransformer>? actionInfoTransformers = null,
        IEnumerable<IHttpRequestPipelinePart>? httpRequestPipelineParts = null)
    {
        var callId = Guid.NewGuid();
        var result = await CallActionCore<TController>(arguments,
            methodName,
            controllerCaller,
            callId,
            callParameters,
            headers,
            authenticationHeaderValue,
            actionInfoTransformers,
            httpRequestPipelineParts);
        return await _httpResponseCallFactory.CreateControllerCallResult<TReturn>(result, callId.ToString());
    }
    
    /// <summary>
    /// Creates http request for the given method and calls the server.
    /// </summary>
    /// <param name="arguments">Arguments of controller method.</param>
    /// <param name="methodName">Controller method for which the request will be generated.</param>
    /// <param name="controllerCaller">Provides information about caller settings.</param>
    /// <param name="callParameters">Argument types of controller method.</param>
    /// <param name="headers">Headers which will be added to the request</param>
    /// <param name="authenticationHeaderValue">Authentication headers which will be added to request</param>
    /// <param name="actionInfoTransformers">Action transformers which will be used for the request</param>
    /// <param name="httpRequestPipelineParts">HttpPipeline parts which will be used for the request</param>
    /// <typeparam name="TController">Controller to be called</typeparam>
    /// <returns>Returns the response from server.</returns>
    public async Task<HttpCallResponse> CallAction<TController>(
        IEnumerable<object?> arguments,
        string methodName,
        IControllerCaller controllerCaller,
        Type[] callParameters,
        IEnumerable<(string Key, string? Value)>? headers = null,
        AuthenticationHeaderValue? authenticationHeaderValue = null,
        IEnumerable<IActionInfoTransformer>? actionInfoTransformers = null,
        IEnumerable<IHttpRequestPipelinePart>? httpRequestPipelineParts = null)
    {
        var callId = Guid.NewGuid();
        var result = await CallActionCore<TController>(arguments,
            methodName,
            controllerCaller,
            callId,
            callParameters,
            headers,
            authenticationHeaderValue,
            actionInfoTransformers,
            httpRequestPipelineParts);
        return await _httpResponseCallFactory.CreateControllerCallResult(result, callId.ToString());
    }
    
    
    /// <summary>
    /// Creates http request for the given method and calls the server.
    /// </summary>
    /// <param name="arguments">Arguments of controller method.</param>
    /// <param name="methodName">Controller method for which the request will be generated.</param>
    /// <param name="controllerCaller">Provides information about caller settings.</param>
    /// <param name="callParameters">Argument types of controller method.</param>
    /// <param name="headers">Headers which will be added to the request</param>
    /// <param name="authenticationHeaderValue">Authentication headers which will be added to request</param>
    /// <param name="actionInfoTransformers">Action transformers which will be used for the request</param>
    /// <param name="httpRequestPipelineParts">HttpPipeline parts which will be used for the request</param>
    /// <typeparam name="TController">Controller to be called</typeparam>
    /// <returns>Returns the response from server.</returns>
    public async Task<HttpResponseMessage> CallActionWithHttpResponseMessageResult<TController>(
        IEnumerable<object?> arguments,
        string methodName,
        IControllerCaller controllerCaller,
        Type[] callParameters,
        IEnumerable<(string Key, string? Value)>? headers = null,
        AuthenticationHeaderValue? authenticationHeaderValue = null,
        IEnumerable<IActionInfoTransformer>? actionInfoTransformers = null,
        IEnumerable<IHttpRequestPipelinePart>? httpRequestPipelineParts = null)
    {
        var callId = Guid.NewGuid();
        var result = await CallActionCore<TController>(arguments,
            methodName,
            controllerCaller,
            callId,
            callParameters,
            headers,
            authenticationHeaderValue,
            actionInfoTransformers,
            httpRequestPipelineParts);
        return result;
    }

    private async Task<HttpResponseMessage> CallActionCore<TController>(
        IEnumerable<object?> arguments,
        string methodName,
        IControllerCaller controllerCaller,
        Guid callId,
        Type[] callParameters,
        IEnumerable<(string Key, string? Value)>? headers = null,
        AuthenticationHeaderValue? authenticationHeaderValue = null,
        IEnumerable<IActionInfoTransformer>? actionInfoTransformers = null,
        IEnumerable<IHttpRequestPipelinePart>? httpRequestPipelineParts = null)
    {
        var requestBuilder = controllerCaller.RequestBuilder.CreateNewBuilderByCopyingExisting();
        requestBuilder.AddHeaders(headers);
        requestBuilder.AddAuthenticationHeaderValue(authenticationHeaderValue);
        requestBuilder.AddHttpRequestPipelineParts(httpRequestPipelineParts);
        requestBuilder.AddActionInfoTransformers(actionInfoTransformers);
        var controllerType = typeof(TController);
        var methodInfo = controllerType.GetMethod(methodName, callParameters);

        if (methodInfo == null)
        {
            throw new InvalidOperationException($"Method with name {methodName} not found in class {controllerType.FullName}.");
        }

        var (url, actionInfo) = await _actionInfoProvider.GetInfo(arguments.ToList(), methodInfo, _actionInfoTransformersCaller);
        using var request = HttpRequestProvider.Create(
            actionInfo.HttpMethod,
            url,
            actionInfo.Body,
            callId,
            actionInfo.BodyFormat,
            actionInfo.Headers,
            _serializer);
        ExceptionManager.ExceptionManager.InsertEmptyDataToIndicateTestCall(callId);

        var result = await _webCaller.Call(request, actionInfo, new MethodInvocationInfo(arguments, methodInfo));
        return result;
    }
}
