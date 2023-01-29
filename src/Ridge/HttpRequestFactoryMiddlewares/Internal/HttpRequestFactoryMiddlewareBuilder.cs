using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Ridge.HttpRequestFactoryMiddlewares.Internal.InternalMiddlewares;
using Ridge.Parameters.CustomParams;
using Ridge.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace Ridge.HttpRequestFactoryMiddlewares.Internal;

/// <summary>
///     Used to register <see cref="HttpRequestFactoryMiddleware" /> and build middleware pipeline.
/// </summary>
internal class HttpRequestFactoryMiddlewareBuilder
{
    private List<HttpRequestFactoryMiddleware> _requestFactoryMiddleware { get; } = new();

    internal HttpRequestFactoryMiddleware BuildRequestFactoryMiddleware(
        IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
        LinkGenerator linkGenerator,
        IRequestResponseSerializer requestResponseSerializer)
    {
        var setHttpMethodAreaAndBaseUrlGenerationParametersMiddleware = new SetHttpMethodAreaAndBaseUrlGenerationParametersMiddleware(actionDescriptorCollectionProvider);
        var addParametersWithoutAnyAttribute = new AddParametersWithoutAnyAttributeMiddleware();


        var addValuesFromKnownCustomParametersMiddleware = new AddValuesFromKnownCustomParametersMiddleware();
        var addBodyByFromBodyMiddleware = new AddBodyByFromBodyAttributeMiddleware();
        var addHeadersByFromHeader = new AddHeadersByFromHeaderAttributeMiddleware();
        var addQueryParametersByFromQueryMiddleware = new AddQueryParametersByFromQueryAttributeMiddleware();
        var addRouteParametersByFromRouteMiddleware = new AddRouteParametersByFromRouteAttributeMiddleware();
        var addTransformedOrAddedParametersMiddleware = new AddTransformedOrAddedParametersMiddleware();
        
       
        var finalMiddleware = new FinalHttpRequestMiddleware(linkGenerator, requestResponseSerializer);

        var requestFactoryMiddlewareArray = _requestFactoryMiddleware
           .Prepend(addValuesFromKnownCustomParametersMiddleware) // 8 custom parameters
           .Prepend(addTransformedOrAddedParametersMiddleware) // 7 Transformed or added parameters
           .Prepend(addRouteParametersByFromRouteMiddleware) // 6
           .Prepend(addQueryParametersByFromQueryMiddleware) // 5
           .Prepend(addHeadersByFromHeader) // 4
           .Prepend(addBodyByFromBodyMiddleware) // 3
           .Prepend(addParametersWithoutAnyAttribute) // 2 
           .Prepend(setHttpMethodAreaAndBaseUrlGenerationParametersMiddleware) // first middleware
            // custom user middlewares will be here
           .Append(finalMiddleware) // last middleware
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
        IEnumerable<HttpHeaderParameter> headers)
    {
        foreach (var header in headers)
        {
            AddHeader(header.Key, header.Value);
        }
    }

    private void AddHeader(
        string key,
        string? value)
    {
        var addHeaderTransformer = new AddHeaderHttpMiddleware(key, value);
        _requestFactoryMiddleware.Add(addHeaderTransformer);
    }
}
