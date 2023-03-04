using Ridge.Parameters;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace Ridge.HttpRequestFactoryMiddlewares.Internal;

internal class RequestFactoryContext : IRequestFactoryContext
{
    private object? _body;

    public RequestFactoryContext(
        MethodInfo calledControllerMethodInfo,
        ParameterProvider parameterProvider,
        Guid callId,
        IServiceProvider serviceProvider)
    {
        CalledControllerMethodInfo = calledControllerMethodInfo;
        ParameterProvider = parameterProvider;
        CallId = callId;
        ServiceProvider = serviceProvider;
    }

    public ParameterProvider ParameterProvider { get; }
    public Guid CallId { get; }
    public IServiceProvider ServiceProvider { get; }
    public MethodInfo CalledControllerMethodInfo { get; }

    public object? Body
    {
        get => _body;
        set
        {
            RequestHasBody = true;
            _body = value;
        }
    }

    public bool RequestHasBody { get; set; }

    public IDictionary<string, object?> UrlGenerationParameters { get; set; } = new Dictionary<string, object?>();
    public string HttpContentType { get; } = "application/json";
    public HttpRequestHeaders Headers { get; set; } = new HttpRequestMessage().Headers;
    public string HttpMethod { get; set; } = "GET";
}
