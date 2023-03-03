using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Ridge.HttpRequestFactoryMiddlewares.Internal.InternalMiddlewares;
using Ridge.Serialization;
using Ridge.Setup;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ridge.HttpRequestFactoryMiddlewares.Internal;

/// <summary>
///     Used to register <see cref="HttpRequestFactoryMiddleware" /> and build middleware pipeline.
/// </summary>
internal class HttpRequestFactoryMiddlewareBuilder
{
    private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
    private readonly LinkGenerator _linkGenerator;
    private readonly SerializerProvider _serializerProvider;
    private readonly List<HttpRequestFactoryMiddleware> _requestFactoryMiddleware;

    public HttpRequestFactoryMiddlewareBuilder(
        IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
        LinkGenerator linkGenerator,
        SerializerProvider serializerProvider,
        IOptions<RidgeOptions> ridgeOptions)
    {
        ArgumentNullException.ThrowIfNull(actionDescriptorCollectionProvider);
        ArgumentNullException.ThrowIfNull(linkGenerator);
        ArgumentNullException.ThrowIfNull(serializerProvider);
        ArgumentNullException.ThrowIfNull(ridgeOptions);

        _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        _linkGenerator = linkGenerator;
        _serializerProvider = serializerProvider;
        _requestFactoryMiddleware = ridgeOptions.Value.HttpRequestFactoryMiddlewares;
    }

    public HttpRequestFactoryMiddleware BuildRequestFactoryMiddleware()
    {
        var setHttpMethodAreaAndBaseUrlGenerationParametersMiddleware = new SetHttpMethodAreaAndBaseUrlGenerationParametersMiddleware(_actionDescriptorCollectionProvider);
        var addParametersWithoutAnyAttribute = new AddParametersWithoutAnyAttributeMiddleware();


        var addValuesFromKnownCustomParametersMiddleware = new AddValuesFromKnownCustomParametersMiddleware();
        var addBodyByFromBodyMiddleware = new AddBodyByFromBodyAttributeMiddleware();
        var addHeadersByFromHeader = new AddHeadersByFromHeaderAttributeMiddleware();
        var addQueryParametersByFromQueryMiddleware = new AddQueryParametersByFromQueryAttributeMiddleware();
        var addRouteParametersByFromRouteMiddleware = new AddRouteParametersByFromRouteAttributeMiddleware();
        var addTransformedOrAddedParametersMiddleware = new AddTransformedOrAddedParametersMiddleware();


        var finalMiddleware = new FinalHttpRequestMiddleware(_linkGenerator, _serializerProvider.GetSerializer());

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
}
