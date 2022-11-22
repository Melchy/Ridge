using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Ridge.HttpRequestFactoryMiddlewares.InternalMiddlewares;
using Ridge.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace Ridge.HttpRequestFactoryMiddlewares;

/// <summary>
///     Used to register <see cref="HttpRequestFactoryMiddleware" /> and build middleware pipeline.
/// </summary>
public class HttpRequestFactoryMiddlewareBuilder
{
    private List<HttpRequestFactoryMiddleware> _requestFactoryMiddleware { get; } = new();

    internal HttpRequestFactoryMiddleware BuildRequestFactoryMiddleware(
        IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
        LinkGenerator linkGenerator,
        IRequestResponseSerializer requestResponseSerializer)
    {
        var initialMiddleware = new InitialHttpRequestMiddleware(actionDescriptorCollectionProvider);
        var headersMiddleware = new AddHeaderToRequestFromCustomParametersMiddleware();
        var finalMiddleware = new FinalHttpRequestMiddleware(linkGenerator, requestResponseSerializer);

        var requestFactoryMiddlewareArray = _requestFactoryMiddleware
           .Prepend(headersMiddleware) // this middleware will end up second
           .Prepend(initialMiddleware) // this middleware will end up first
           .Append(finalMiddleware)
           .ToArray();
        for (var i = 0; i < requestFactoryMiddlewareArray.Length - 1; i++)
        {
            requestFactoryMiddlewareArray[i].RequestFactoryMiddlewareInner = requestFactoryMiddlewareArray[i + 1];
        }

        return requestFactoryMiddlewareArray[0];
    }


    /// <summary>
    ///     Add <see cref="HttpRequestFactoryMiddleware" />.
    /// </summary>
    /// <param name="httpRequestFactoryMiddlewares">Add one or more <see cref="HttpRequestFactoryMiddleware" />.</param>
    public void AddHttpRequestFactoryMiddlewares(
        IEnumerable<HttpRequestFactoryMiddleware> httpRequestFactoryMiddlewares)
    {
        foreach (var httpRequestFactoryMiddleware in httpRequestFactoryMiddlewares)
        {
            _requestFactoryMiddleware.Add(httpRequestFactoryMiddleware);
        }
    }

    /// <summary>
    ///     Add headers to request. This method actually adds <see cref="HttpRequestFactoryMiddleware" /> which adds the
    ///     headers.
    /// </summary>
    /// <param name="headers">Headers to add.</param>
    public void AddHeaders(
        IEnumerable<HttpHeader>? headers)
    {
        if (headers == null)
        {
            return;
        }

        foreach (var header in headers)
        {
            AddHeader(header.Key, header.Value);
        }
    }

    internal HttpRequestFactoryMiddlewareBuilder CreateNewBuilderByCopyingExisting()
    {
        var httpRequestFactoryMiddlewareBuilder = new HttpRequestFactoryMiddlewareBuilder();
        httpRequestFactoryMiddlewareBuilder.AddHttpRequestFactoryMiddlewares(_requestFactoryMiddleware);
        return httpRequestFactoryMiddlewareBuilder;
    }

    private void AddHeader(
        string key,
        string? value)
    {
        var addHeaderTransformer = new AddHeaderHttpRequestFactoryMiddleware(key, value);
        _requestFactoryMiddleware.Add(addHeaderTransformer);
    }
}
