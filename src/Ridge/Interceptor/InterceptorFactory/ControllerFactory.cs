using Castle.DynamicProxy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ridge.Interceptor.ActionInfo;
using Ridge.Interceptor.ResultFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace Ridge.Interceptor.InterceptorFactory
{
    public class ControllerFactory : RidgeFactory
    {
        private readonly HttpClient _httpClient;
        private readonly IServiceProvider _serviceProvider;

        public ControllerFactory(HttpClient httpClient, IServiceProvider serviceProvider)
        {
            _httpClient = httpClient;
            _serviceProvider = serviceProvider;
        }
        public TController CreateController<TController>()
        {
            CheckIfControllerActionsCanBeProxied<TController>();
            var webCaller = GetWebCaller(_httpClient, _serviceProvider.GetService<ILogger<ControllerFactory>>());
            var preCallMiddlewareCaller = GetPreCallMiddlewareCaller();
            var createInfoForController = new CreateInfoForController(_serviceProvider);
            var resultFactoryForController = new ResultFactoryForController();
            var interceptor = new SendHttpRequestAsyncInterceptor<TController>(
                webCaller,
                createInfoForController,
                resultFactoryForController,
                preCallMiddlewareCaller).ToInterceptor();
            return CreateClassFromInterceptor<TController>(interceptor);
        }

        private static void CheckIfControllerActionsCanBeProxied<T>()
        {
            var nonVirtualMethods = GetAllNonVirtualMethodsWithHttpXxxAttribute<T>();

            if (nonVirtualMethods.Any())
            {
                throw new InvalidOperationException($"To use controller caller you must mark all methods with attribute HttpXXX as virtual. " +
                                                    $"Mark following methods in controller '{typeof(T).Name}' as virtual: " +
                                                    $"{string.Join(", ", nonVirtualMethods.Select(x => x.Name))}");
            }
        }

        private static IEnumerable<MethodInfo> GetAllNonVirtualMethodsWithHttpXxxAttribute<T>()
        {
            return typeof(T).GetMethods().Where(m =>
                    GeneralHelpers.HasAttribute<HttpPostAttribute>(m) ||
                    GeneralHelpers.HasAttribute<HttpGetAttribute>(m) ||
                    GeneralHelpers.HasAttribute<HttpPutAttribute>(m) ||
                    GeneralHelpers.HasAttribute<HttpDeleteAttribute>(m) ||
                    GeneralHelpers.HasAttribute<HttpPatchAttribute>(m) ||
                    GeneralHelpers.HasAttribute<HttpHeadAttribute>(m) ||
                    GeneralHelpers.HasAttribute<HttpOptionsAttribute>(m))
                .Where(m => !m.IsVirtual);
        }
    }
}
