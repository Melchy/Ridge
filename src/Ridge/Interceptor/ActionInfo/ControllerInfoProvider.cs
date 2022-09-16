using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Ridge.Transformers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Ridge.Interceptor.ActionInfo
{
    internal class ControllerInfoProvider
    {
        private readonly LinkGenerator _linkGenerator;
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

        public ControllerInfoProvider(
            IServiceProvider serviceProvider)
        {
            _linkGenerator = serviceProvider.GetService<LinkGenerator>();
            _actionDescriptorCollectionProvider = serviceProvider.GetService<IActionDescriptorCollectionProvider>();
        }

        public async Task<(string url, Dtos.ActionInfo actionArgumentsInfo)> GetInfo(
            IEnumerable<object?> arguments,
            MethodInfo methodInfo,
            ActionInfoTransformersCaller actionInfoTransformersCaller)
        {
            var methodActionInfo = Dtos.ActionInfo.CreateActionInfo(arguments, methodInfo);
            var actionDescriptor = GetActionDescriptor(methodInfo);
            var httpMethodAsString = GetHttpMethod(actionDescriptor);

            var routeDescription = actionDescriptor.RouteValues.ToDictionary(x => x.Key, x => (object?)x.Value);
            methodActionInfo.RouteParams = GeneralHelpers.MergeDictionaries(routeDescription, methodActionInfo.RouteParams);
            methodActionInfo.HttpMethod = httpMethodAsString;
            await actionInfoTransformersCaller.Call(methodActionInfo, new InvocationInfo(arguments, methodInfo));
            var url = CreateUri(methodActionInfo.RouteParams);
            return (url, methodActionInfo);
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

        private string GetHttpMethod(
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
                throw new InvalidOperationException("Http method could not be found.");
            }

            return httpMethod;
        }

        private string CreateUri(
            object routeParams)
        {
            var uri = _linkGenerator.GetPathByRouteValues("", routeParams);
            if (uri == null)
            {
                throw new InvalidOperationException(
                    "Could not generate uri.");
            }

            return uri;
        }
    }
}
