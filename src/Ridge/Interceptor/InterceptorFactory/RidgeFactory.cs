using Castle.DynamicProxy;
using Ridge.LogWriter;
using Ridge.Pipeline;
using Ridge.Pipeline.Public;
using Ridge.Pipeline.Public.DefaulPipelineParts;
using Ridge.Transformers;
using Ridge.Transformers.DefaultTransformers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace Ridge.Interceptor.InterceptorFactory
{
    /// <summary>
    ///     Base factory.
    /// </summary>
    public abstract class RidgeFactory
    {
        private readonly List<IHttpRequestPipelinePart> _pipelineParts = new();
        private readonly List<IActionInfoTransformer> _actionInfoTransformers = new();

        internal readonly RequestBuilder RequestBuilder = new();

        /// <summary>
        ///     Adds <see cref="IHttpRequestPipelinePart"/> which can transform request after url is constructed.
        /// </summary>
        /// <param name="httpRequestPipelinePart"></param>
        public void AddHttpRequestPipelinePart(
            IHttpRequestPipelinePart httpRequestPipelinePart)
        {
            RequestBuilder.AddHttpRequestPipelinePart(httpRequestPipelinePart);
        }

        /// <summary>
        ///     Adds <see cref="IActionInfoTransformer"/> which can transform request before url is constructed.
        /// </summary>
        /// <param name="actionInfoTransformer"></param>
        public void AddActionInfoTransformer(
            IActionInfoTransformer actionInfoTransformer)
        {
            RequestBuilder.AddActionInfoTransformer(actionInfoTransformer);
        }

        /// <summary>
        ///     Adds <see cref="IActionInfoTransformer"/> which adds header to the request.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddHeader(
            string key,
            string? value)
        {
            RequestBuilder.AddHeader(key, value);
        }

        /// <summary>
        ///     Adds multiple <see cref="IActionInfoTransformer"/> which adds headers to the request.
        /// </summary>
        /// <param name="headers"></param>
        public void AddHeaders(
            IDictionary<string, string?> headers)
        {
            RequestBuilder.AddHeaders(headers);
        }

        /// <summary>
        ///     Adds pipeline part which sets Authorization.
        /// </summary>
        /// <param name="authenticationHeaderValue"></param>
        public void AddAuthorization(
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            RequestBuilder.AddAuthorization(authenticationHeaderValue);
        }

        private protected T CreateClassFromInterceptor<T>(
            IAsyncInterceptor interceptor)
            where T : class
        {
            var controllerProxy = ProxyGeneratorWithoutCallingCtor.CreateProxyWithoutCallingConstructor(typeof(T), interceptor.ToInterceptor());
            return (T)controllerProxy;
        }
        
        /// <summary>
        ///     Default proxy generator requires real instance of proxied object. We are never calling the real object
        ///     so we do not need real instance. Therefore we use  FormatterServices.GetUninitializedObject()
        ///     which allows construction of object without call to constructor.
        ///     Castle also needs to create instance even when used with CreateClassProxyTypeWithTarget so we have to
        ///     use little hack to set interceptor.
        /// </summary>
        private class ProxyGeneratorWithoutCallingCtor : ProxyGenerator
        {
            private ProxyGeneratorWithoutCallingCtor()
            {
            }

            public static object CreateProxyWithoutCallingConstructor(
                Type type,
                IInterceptor interceptor)
            {
                ProxyGeneratorWithoutCallingCtor generatorWithoutCallingCtor = new();
                return generatorWithoutCallingCtor.CreateClassProxyWithoutCallingConstructor(type, interceptor);
            }

            private object CreateClassProxyWithoutCallingConstructor(
                Type type,
                IInterceptor interceptor)
            {
                var proxyType = CreateClassProxyType(type, Array.Empty<Type>(), ProxyGenerationOptions.Default);
                var instance = GeneralHelpers.CreateObjectWithoutCallingConstructor(proxyType);
                SetInterceptor(instance, interceptor);
                return instance;
            }

            private void SetInterceptor(
                object proxy,
                IInterceptor interceptor)
            {
                var field = proxy.GetType().GetField("__interceptors", BindingFlags.Instance | BindingFlags.NonPublic);
                if (field == null)
                {
                    throw new InvalidOperationException("Internal castle windsor property - __interceptors not found.");
                }

                field.SetValue(proxy, new[] { interceptor });
            }
        }
    }

    /// <summary>
    ///     TODO
    /// </summary>
    public class RequestBuilder
    {
        private readonly List<IHttpRequestPipelinePart> _pipelineParts = new();
        private readonly List<IActionInfoTransformer> _actionInfoTransformers = new();

        internal WebCaller BuildWebCaller(
            HttpClient httpClient,
            ILogWriter logger)
        {
            return new WebCaller(httpClient, logger, _pipelineParts);
        }

        internal ActionInfoTransformersCaller BuildActionInfoTransformerCaller()
        {
            return new ActionInfoTransformersCaller(_actionInfoTransformers);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="httpRequestPipelinePart"></param>
        public void AddHttpRequestPipelinePart(
            IHttpRequestPipelinePart httpRequestPipelinePart)
        {
            _pipelineParts.Add(httpRequestPipelinePart);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="actionInfoTransformer"></param>
        public void AddActionInfoTransformer(
            IActionInfoTransformer actionInfoTransformer)
        {
            _actionInfoTransformers.Add(actionInfoTransformer);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddHeader(
            string key,
            string? value)
        {
            var addHeaderTransformer = new AddHeaderActionInfoTransformer(key, value);
            _actionInfoTransformers.Add(addHeaderTransformer);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="headers"></param>
        public void AddHeaders(
            IDictionary<string, string?> headers)
        {
            foreach (var header in headers)
            {
                AddHeader(header.Key, header.Value);
            }
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="authenticationHeaderValue"></param>
        public void AddAuthorization(
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            var addAuthenticationPipelinePart = new AddAuthenticationPipelinePart(authenticationHeaderValue);
            _pipelineParts.Add(addAuthenticationPipelinePart);
        }
    }
}
