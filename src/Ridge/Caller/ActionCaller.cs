using Ridge.ActionInfo;
using Ridge.Interceptor;
using Ridge.LogWriter;
using Ridge.Pipeline;
using Ridge.Pipeline.Public;
using Ridge.Response;
using Ridge.Serialization;
using Ridge.Transformers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Ridge.Caller;

/// <summary>
///     This call is used to create http request and call server.
///     The request is created from method info and method arguments.
/// </summary>
public class ActionCaller
{
    private readonly WebCaller _webCaller;
    private readonly ActionInfoProvider _actionInfoProvider;
    private readonly IHttpResponseCallFactory _httpResponseCallFactory;
    private readonly ActionInfoTransformersCaller _actionInfoTransformersCaller;
    private readonly IRequestResponseSerializer _serializer;


    /// <summary>
    ///     Create <see cref="ActionCaller" />.
    /// </summary>
    /// <param name="requestBuilder">
    ///     Builder which can be used to add <see cref="IActionInfoTransformer" /> and
    ///     <see cref="IHttpRequestPipelinePart" />.
    /// </param>
    /// <param name="logWriter">Logger which logs generated request and responses.</param>
    /// <param name="httpClient">HttpClient used to call the server.</param>
    /// <param name="serviceProvider">ServiceProvider used to gather information about the server.</param>
    /// <param name="serializer">Serializer used to serialize and deserialize requests.</param>
    public ActionCaller(
        RequestBuilder requestBuilder,
        ILogWriter? logWriter,
        HttpClient httpClient,
        IServiceProvider serviceProvider,
        IRequestResponseSerializer? serializer)
    {
        _serializer = SerializerProvider.GetSerializer(serviceProvider, serializer);
        _actionInfoProvider = new ActionInfoProvider(serviceProvider);
        _httpResponseCallFactory = new HttpResponseCallFactory(_serializer);
        _webCaller = requestBuilder.BuildWebCaller(httpClient, logWriter ?? new NullLogWriter());
        _actionInfoTransformersCaller = requestBuilder.BuildActionInfoTransformerCaller();
    }

    /// <summary>
    ///     Creates http request for the given method and calls the server.
    /// </summary>
    /// <param name="arguments">Arguments of controller method.</param>
    /// <param name="method">Method on controller for which the request will be generated.</param>
    /// <returns>Returns the response from server.</returns>
    public async Task<HttpResponseMessage> CallActionWithHttpResponseMessageResult(
        IEnumerable<object?> arguments,
        MethodInfo method)
    {
        var callId = Guid.NewGuid();
        return await CallActionCore(arguments, method, callId);
    }


    /// <summary>
    ///     Creates http request for the given method and calls the server.
    /// </summary>
    /// <param name="arguments">Arguments of controller method.</param>
    /// <param name="method">Method on controller for which the request will be generated.</param>
    /// <returns>Returns the response from server.</returns>
    public async Task<HttpCallResponse> CallAction(
        IEnumerable<object?> arguments,
        MethodInfo method)
    {
        var callId = Guid.NewGuid();
        var result = await CallActionCore(arguments, method, callId);
        return await _httpResponseCallFactory.CreateControllerCallResult(result, callId.ToString());
    }

    /// <summary>
    ///     Creates http request for the given method and calls the server.
    /// </summary>
    /// <param name="arguments">Arguments of controller method.</param>
    /// <param name="method">Method on controller for which the request will be generated.</param>
    /// <typeparam name="TReturn">Response type of the controller.</typeparam>
    /// <returns>Returns the response from server.</returns>
    public async Task<HttpCallResponse<TReturn>> CallAction<TReturn>(
        IEnumerable<object?> arguments,
        MethodInfo method)
    {
        var callId = Guid.NewGuid();
        var result = await CallActionCore(arguments, method, callId);
        return await _httpResponseCallFactory.CreateControllerCallResult<TReturn>(result, callId.ToString());
    }

    private async Task<HttpResponseMessage> CallActionCore(
        IEnumerable<object?> arguments,
        MethodInfo method,
        Guid callId)
    {
        var (url, actionInfo) = await _actionInfoProvider.GetInfo(arguments.ToList(), method, _actionInfoTransformersCaller);
        using var request = HttpRequestProvider.Create(
            actionInfo.HttpMethod,
            url,
            actionInfo.Body,
            callId,
            actionInfo.BodyFormat,
            actionInfo.Headers,
            _serializer);
        ExceptionManager.ExceptionManager.InsertEmptyDataToIndicateTestCall(callId);

        var result = await _webCaller.Call(request, actionInfo, new MethodInvocationInfo(arguments, method));
        return result;
    }
}
