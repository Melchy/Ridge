using Castle.DynamicProxy;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ridge.Interceptor.ActionInfo;
using Ridge.Interceptor.ResultFactory;
using Ridge.LogWriter;
using Ridge.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Ridge.Interceptor.InterceptorFactory
{
    public class RazorPageFactory : RidgeFactory
    {
        private readonly HttpClient _httpClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogWriter? _logWriter;

        public RazorPageFactory(HttpClient httpClient, IServiceProvider serviceProvider, ILogWriter? logWriter = null)
        {
            _httpClient = httpClient;
            _serviceProvider = serviceProvider;
            _logWriter = logWriter;
        }

        /// <summary>
        /// Creates Razor page with intercepted methods.
        /// </summary>
        /// <typeparam name="TPage"></typeparam>
        /// <returns></returns>
        public TPage CreateRazorPage<TPage>() where TPage : PageModel
        {
            CheckIfPageActionsCanBeProxied<TPage>();
            var caller = GetWebCaller(_httpClient, _logWriter);
            var actionInfoTransformerCaller = GetActionInfoTransformerCaller();
            var razorPageInfoFactory = new RazorPageInfoFactory(_serviceProvider);
            var resultFactoryForPages = new ResultFactoryForPages();
            var interceptor = new SendHttpRequestAsyncInterceptor<TPage>(caller, razorPageInfoFactory, resultFactoryForPages, actionInfoTransformerCaller);
            return CreateClassFromInterceptor<TPage>(interceptor);
        }

        private static void CheckIfPageActionsCanBeProxied<T>()
        {
            var potentialActions = typeof(T).GetMethods();
            var actions = potentialActions.Where(x => RazorPageMethodHelpers.IsMethodValidHandler(x));
            CheckNonVirtualMethods<T>(actions);
            CheckReturnTypes<T>(actions);
        }

        private static void CheckReturnTypes<T>(IEnumerable<MethodInfo> actions)
        {
            var methodsWithInvalidReturnType = actions.Where(x =>
            {
                var returnType = GetReturnTypeOrGenericArgumentOfTask(x);
                if (!GeneralHelpers.IsOrImplements(returnType, typeof(PageResult<>)))
                {
                    return true;
                }

                if (returnType.GetGenericArguments()[0] != typeof(T))
                {
                    return true;
                }

                return false;
            });

            if (methodsWithInvalidReturnType.Any())
            {
                throw new InvalidOperationException(
                    $"Return type must be CustomActionResult with generic type of {typeof(T).Name}. " +
                    $"Following methods have incorrect return types: {string.Join(", ", methodsWithInvalidReturnType.Select(x => x.Name))}");
            }
        }

        private static void CheckNonVirtualMethods<T>(IEnumerable<MethodInfo> actions)
        {
            var nonVirtualActions = actions.Where(x => !x.IsVirtual);
            if (nonVirtualActions.Any())
            {
                throw new InvalidOperationException($"All actions in page {typeof(T).Name} must be virtual. " +
                                                    $"Following actions are not virtual: {string.Join(", ", nonVirtualActions.Select(x => x.Name))}");
            }
        }

        private static Type GetReturnTypeOrGenericArgumentOfTask(MethodInfo methodInfo)
        {
            var actionReturnType = methodInfo.ReturnType;
            if (!methodInfo.ReturnType.IsGenericType)
            {
                return methodInfo.ReturnType;
            }
            if (typeof(Task).IsAssignableFrom(methodInfo.ReturnType))
            {
                actionReturnType = methodInfo.ReturnType.GetGenericArguments()[0];
            }

            return actionReturnType;
        }
    }
}
