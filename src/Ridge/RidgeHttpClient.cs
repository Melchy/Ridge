using Microsoft.AspNetCore.Mvc.Testing;
using Ridge.HttpRequestFactoryMiddlewares.Internal;
using Ridge.Serialization;
using System;
using System.Net.Http;

namespace Ridge;

/// <summary>
///     Http client used to perform RidgeCaller calls.
/// </summary>
public class RidgeHttpClient
{
    internal RidgeHttpClient(
        HttpClient httpClient,
        IRequestResponseSerializer? requestResponseSerializer,
        IServiceProvider serviceProvider,
        HttpRequestFactoryMiddlewareBuilder httpRequestFactoryMiddlewareBuilder)
    {
        HttpClient = httpClient;
        RequestResponseSerializer = requestResponseSerializer;
        ServiceProvider = serviceProvider;
        HttpRequestFactoryMiddlewareBuilder = httpRequestFactoryMiddlewareBuilder;
    }

    /// <summary>
    ///     Standard httpClient created by <see cref="WebApplicationFactory{TEntryPoint}" />.
    /// </summary>
    public HttpClient HttpClient { get; }

    internal IRequestResponseSerializer? RequestResponseSerializer { get; }
    internal IServiceProvider ServiceProvider { get; }
    internal HttpRequestFactoryMiddlewareBuilder HttpRequestFactoryMiddlewareBuilder { get; }
}
