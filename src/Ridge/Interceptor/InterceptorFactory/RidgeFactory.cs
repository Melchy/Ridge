using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using Ridge.Middlewares.DefaulMiddlewares;
using Ridge.Middlewares.Infrastructure;
using Ridge.Middlewares.Public;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;

namespace Ridge.Interceptor.InterceptorFactory
{
    public abstract class RidgeFactory
    {
        private readonly List<CallMiddleware> _middlewares = new List<CallMiddleware>();

        protected Func<HttpRequestMessage, Task<HttpResponseMessage>> GetCaller(HttpClient httpClient, ILogger logger)
        {
            var finalMiddleware = new CallWebAppMiddleware(httpClient, logger);
            return (httpRequestMessage) => CallMiddlewareComposer.Execute(_middlewares, finalMiddleware, httpRequestMessage);
        }
        protected T CreateClassFromInterceptor<T>(IInterceptor interceptor)
        {
            var controllerProxy = MyProxyGenerator.CreateProxyWithoutCallingConstructor(typeof(T), interceptor);
            return (T)controllerProxy;
        }

        public void AddCallMiddleware(CallMiddleware callMiddleware)
        {
            _middlewares.Add(callMiddleware);
        }

        public void AddHeader(string key, string value)
        {
            var addHeaderMiddleware = new AddHeaderMiddleware(key, value);
            _middlewares.Add(addHeaderMiddleware);
        }

        public void AddHeaders(IReadOnlyDictionary<string, string> headers)
        {
            foreach (var header in headers)
            {
                var addHeaderMiddleware = new AddHeaderMiddleware(header.Key, header.Value);
                _middlewares.Add(addHeaderMiddleware);
            }
        }

        public void AddAuthorization(AuthenticationHeaderValue authenticationHeaderValue)
        {
            var addAuthenticationMiddleware = new AddAuthenticationMiddleware(authenticationHeaderValue);
            _middlewares.Add(addAuthenticationMiddleware);
        }

        /// <summary>
        /// Default proxy generator requires real instance of proxied object. We are never calling the real object
        /// so we do not need real instance. Therefore we use  FormatterServices.GetUninitializedObject()
        /// which allows construction of object without call to constructor.
        /// Castle also needs to create instance even when used with CreateClassProxyTypeWithTarget so we have to
        /// use little hack to set interceptor.
        /// </summary>
        private class MyProxyGenerator : ProxyGenerator
        {
            public static object CreateProxyWithoutCallingConstructor(Type type, IInterceptor interceptor)
            {
                MyProxyGenerator generator = new MyProxyGenerator();
                return generator.CreateClassProxyWithoutCallingConstructor(type, interceptor);
            }

            private object CreateClassProxyWithoutCallingConstructor(Type type, IInterceptor sourcererInterceptor)
            {
                var prxType = CreateClassProxyType(type, Array.Empty<Type>(), ProxyGenerationOptions.Default);
                var instance = GeneralHelpers.CreateObjectWithoutCallingConstructor(prxType);
                SetInterceptors(instance, sourcererInterceptor);
                return instance;
            }

            private void SetInterceptors(object proxy, params IInterceptor[] interceptors)
            {
                var field = proxy.GetType().GetField("__interceptors", BindingFlags.Instance | BindingFlags.NonPublic);
                if (field == null)
                {
                    throw new InvalidOperationException("Internal castle windsor property - __interceptors not found. This should never happen.");
                }
                field.SetValue(proxy, interceptors);
            }

            private MyProxyGenerator()
            {

            }
        }
    }
}
