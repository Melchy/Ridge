using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace Ridge.DelegationHandlers;

/// <summary>
///     Contains all the information about request gathered by ridge
/// </summary>
public class RequestDescription
{
    /// <summary>
    /// Key which identified this object in <see cref="HttpRequestOptions"/>.
    /// </summary>
    public static string OptionsKey = nameof(RequestDescription);

    /// <summary>
    /// Body of request
    /// </summary>
    public object? Body { get; }

    /// <summary>
    /// Route parameters
    /// </summary>
    public IReadOnlyDictionary<string, object?> RouteParams { get; }

    /// <summary>
    /// Http content type
    /// </summary>
    public string HttpContentType { get; }

    /// <summary>
    /// Http headers
    /// </summary>
    public HttpHeaders Headers { get; }

    /// <summary>
    /// Controller method which should be called when request is processed.
    /// </summary>
    public MethodInfo CalledMethod { get; }

    /// <summary>
    /// Arguments passed to Caller
    /// </summary>
    public IEnumerable<object?> PassedArguments { get; }

    /// <summary>
    /// Http method
    /// </summary>
    public string HttpMethod { get; } = null!;

    /// <summary>
    /// Custom parameters provided by user in CustomParameters argument
    /// </summary>
    public CustomParameterProvider CustomParametersProvider { get; }

    /// <summary>
    /// Creates <see cref="RequestDescription"/> 
    /// </summary>
    /// <param name="body">Http body</param>
    /// <param name="routeParams">Route parameters</param>
    /// <param name="httpContentType">Http content type</param>
    /// <param name="httpMethod">Http method</param>
    /// <param name="headers">Http headers</param>
    /// <param name="calledMethod">Controller method which should be called when request is processed</param>
    /// <param name="passedArguments">Arguments passed to Caller</param>
    /// <param name="customParametersProvider">Custom parameters provided by user in CustomParameters argument</param>
    public RequestDescription(
        object? body,
        IReadOnlyDictionary<string, object?> routeParams,
        string httpContentType,
        string httpMethod,
        HttpHeaders headers,
        MethodInfo calledMethod,
        IEnumerable<object?> passedArguments,
        CustomParameterProvider customParametersProvider)
    {
        Body = body;
        RouteParams = routeParams;
        HttpContentType = httpContentType;
        HttpMethod = httpMethod;
        Headers = headers;
        CalledMethod = calledMethod;
        PassedArguments = passedArguments;
        CustomParametersProvider = customParametersProvider;
    }
}
