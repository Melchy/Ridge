using Microsoft.AspNetCore.Mvc;
using Ridge.Interceptor.ActionInfo;
using Ridge.Interceptor.ResultFactory;
using Ridge.LogWriter;
using Ridge.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Ridge.Interceptor.InterceptorFactory
{
    /// <summary>
    ///     This factory returns custom implementation of provided controller.
    ///     This custom implementation is created at runtime and it's methods replace
    ///     user implementation by http calls using mock server.
    /// </summary>
    public class ControllerFactory : RidgeFactory
    {
        private readonly HttpClient _httpClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogWriter? _logWriter;
        private readonly IRidgeSerializer _serializer;

        /// <summary>
        ///     Create controller factory.
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="serviceProvider">Service provider present in WebApplicationFactory.</param>
        /// <param name="logWriter">
        ///     Used to log requests and responses from server.
        ///     Use <see cref="XunitLogWriter" /> or <see cref="NunitLogWriter" /> or implement custom <see cref="ILogWriter" />
        /// </param>
        /// <param name="ridgeSerializer">
        ///     Serializer used to serialize and deserialize requests.
        ///     Serializer is by default chosen based on asp.net settings. If you need custom serializer implement
        ///     <see cref="IRidgeSerializer" />.
        /// </param>
        public ControllerFactory(
            HttpClient httpClient,
            IServiceProvider serviceProvider,
            ILogWriter? logWriter = null,
            IRidgeSerializer? ridgeSerializer = null)
        {
            _httpClient = httpClient;
            _serviceProvider = serviceProvider;
            _logWriter = logWriter;
            _serializer = SerializerProvider.GetSerializer(serviceProvider, ridgeSerializer);
        }

        /// <summary>
        /// Creates type inherited from <typeparamref name="TController"/>.
        /// This inherited type is created at runtime and it replaces all method calls
        /// by http calls using mock server.
        /// </summary>
        /// <typeparam name="TController"></typeparam>
        /// <returns></returns>
        public TController CreateController<TController>()
            where TController : class
        {
            CheckIfControllerActionsCanBeProxied<TController>();
            var webCaller = GetWebCaller(_httpClient, _logWriter);
            var actionInfoTransformerCaller = GetActionInfoTransformerCaller();
            var createInfoForController = new CreateInfoForController(_serviceProvider);
            var resultFactoryForController = new ResultFactoryForController(_serializer);
            var interceptor = new SendHttpRequestAsyncInterceptor<TController>(
                webCaller,
                createInfoForController,
                resultFactoryForController,
                actionInfoTransformerCaller,
                _serializer,
                EnsureCorrectReturnType);
            return CreateClassFromInterceptor<TController>(interceptor);
        }

        private void EnsureCorrectReturnType(
            MethodInfo method)
        {
            var returnType = method.ReturnType;
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                returnType = returnType.GenericTypeArguments[0];
            }

            if (!GeneralHelpers.IsOrImplements(returnType, typeof(ActionResult<>))
                && !GeneralHelpers.IsOrImplements(returnType, typeof(IActionResult)))
            {
                throw new InvalidOperationException($"Controller method must return {nameof(ActionResult)} or {nameof(ActionResult)}<T> or {nameof(IActionResult)}");
            }
        }

        private static void CheckIfControllerActionsCanBeProxied<T>()
        {
            var nonVirtualMethods = GetAllNonVirtualMethodsWithHttpXxxAttribute<T>();

            if (nonVirtualMethods.Any())
            {
                throw new InvalidOperationException("To use controller caller you must mark all methods with attribute HttpXXX as virtual. " +
                                                    $"Mark following methods in controller '{typeof(T).Name}' as virtual: " +
                                                    $"{string.Join(", ", nonVirtualMethods.Select(x => x.Name))}");
            }
        }

        private static IEnumerable<MethodInfo> GetAllNonVirtualMethodsWithHttpXxxAttribute<T>()
        {
            return typeof(T).GetMethods()
                .Where(m =>
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
