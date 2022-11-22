using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Reflection;

namespace Ridge.HttpRequestFactoryMiddlewares;

/// <summary>
///     Contains information about request. This Context will be later used to generate
///     <see cref="RequestFactoryContext" />
/// </summary>
public class RequestFactoryContext
{
    /// <summary>
    ///     Creates <see cref="RequestFactoryContext" />
    /// </summary>
    /// <param name="customParametersProvider">Custom parameters passed by user to customParameters argument in caller.</param>
    /// <param name="methodInfo">Controller method which should be called when request is processed.</param>
    /// <param name="arguments">Arguments passed to caller.</param>
    /// <param name="callId">Id used to identify call. This id is added to header.</param>
    public RequestFactoryContext(
        CustomParameterProvider customParametersProvider,
        MethodInfo methodInfo,
        IEnumerable<object?> arguments,
        Guid callId)
    {
        CustomParametersProvider = customParametersProvider;
        MethodInfo = methodInfo;
        Arguments = arguments;
        CallId = callId;
    }

    /// <summary>
    ///     Arguments passed to caller.
    /// </summary>
    public IEnumerable<object?> Arguments { get; set; }

    /// <summary>
    ///     Id used to identify call. This id is added to header.
    /// </summary>
    public Guid CallId { get; }

    /// <summary>
    ///     Controller method which should be called when request is processed.
    /// </summary>
    public MethodInfo MethodInfo { get; set; }

    /// <summary>
    ///     Custom parameters passed by user to customParameters argument in caller.
    /// </summary>
    public CustomParameterProvider CustomParametersProvider { get; set; }

    /// <summary>
    ///     Request body.
    /// </summary>
    public object? Body { get; set; }

    /// <summary>
    ///     Route parameters in request.
    /// </summary>
    public IDictionary<string, object?> RouteParams { get; set; } = new Dictionary<string, object?>();

    /// <summary>
    ///     Http content type.
    /// </summary>
    public string ContentType { get; set; } = null!; // This value is set in first HttpRequestFactoryMiddleware

    /// <summary>
    ///     Http headers.
    /// </summary>
    public HttpRequestHeaders Headers { get; set; } = null!; // This value is set in first HttpRequestFactoryMiddleware

    /// <summary>
    ///     Http method.
    /// </summary>
    public string HttpMethod { get; set; } = null!; // This value is set in first HttpRequestFactoryMiddleware
}
