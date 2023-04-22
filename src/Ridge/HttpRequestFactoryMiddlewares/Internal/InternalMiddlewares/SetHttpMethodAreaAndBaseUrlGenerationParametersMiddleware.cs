using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Ridge.HttpRequestFactoryMiddlewares.Internal.InternalMiddlewares;

internal class SetHttpMethodAreaAndBaseUrlGenerationParametersMiddleware : HttpRequestFactoryMiddleware
{
    private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

    public SetHttpMethodAreaAndBaseUrlGenerationParametersMiddleware(
        IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
    {
        _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
    }

    public override Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        var actionDescriptor = GetActionDescriptor(requestFactoryContext.CalledControllerMethodInfo);
        var routeDescription = actionDescriptor.RouteValues.ToDictionary(x => x.Key, x => (object?)x.Value);
        requestFactoryContext.UrlGenerationParameters = GeneralHelpers.MergeDictionaries(routeDescription, requestFactoryContext.UrlGenerationParameters);
        requestFactoryContext.HttpMethod = GetHttpMethod(actionDescriptor);
        return base.CreateHttpRequest(requestFactoryContext);
    }

    private ControllerActionDescriptor GetActionDescriptor(
        MethodInfo methodInfo)
    {
        var actions = _actionDescriptorCollectionProvider.ActionDescriptors.Items
           .Where(x => x is ControllerActionDescriptor)
           .Cast<ControllerActionDescriptor>();

        var actionDescriptor = actions.FirstOrDefault(x => x.MethodInfo == methodInfo.GetBaseDefinition());
        if (actionDescriptor == null)
        {
            throw new InvalidOperationException($"Controller action for method {methodInfo.Name} not found.");
        }

        return actionDescriptor;
    }

    private static string GetHttpMethod(
        ControllerActionDescriptor controllerActionDescriptor)
    {
        var httpMethodMetadata = controllerActionDescriptor.EndpointMetadata.FirstOrDefault(x => x is HttpMethodMetadata) as HttpMethodMetadata;
        if (httpMethodMetadata == null)
        {
            return "GET";
        }

        var httpMethod = httpMethodMetadata.HttpMethods.ElementAtOrDefault(0);
        if (httpMethod == null)
        {
            return "GET";
        }

        return httpMethod;
    }
}
