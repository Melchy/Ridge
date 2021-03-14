using Castle.DynamicProxy;
using Ridge.CallData;
using Ridge.Interceptor.ActionInfo;
using Ridge.Interceptor.InterceptorFactory;
using Ridge.Interceptor.ResultFactory;
using Ridge.Pipeline;
using Ridge.Results;
using Ridge.Transformers;
using stakx.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Ridge.Interceptor
{
    public class SendHttpRequestAsyncInterceptor<T> : AsyncInterceptor
    {
        private readonly WebCaller _webCaller;
        private readonly IGetInfo _getInfo;
        private readonly IResultFactory _resultFactory;
        private readonly ActionInfoTransformersCaller _actionInfoTransformersCaller;
        private readonly Action<MethodInfo>? _methodValidation;

        internal SendHttpRequestAsyncInterceptor(
            WebCaller webCaller,
            IGetInfo getInfo,
            IResultFactory resultFactory,
            ActionInfoTransformersCaller actionInfoTransformersCaller,
            Action<MethodInfo>? methodValidation = null)
        {
            _webCaller = webCaller;
            _getInfo = getInfo;
            _resultFactory = resultFactory;
            _actionInfoTransformersCaller = actionInfoTransformersCaller;
            _methodValidation = methodValidation;
        }

        protected override void Intercept(
            IInvocation invocation)
        {
            throw new InvalidOperationException("Synchronous methods are not supported by ridge.");
        }


        protected override async ValueTask InterceptAsync(
            IAsyncInvocation invocation)
        {
            invocation.Result = await CallControllerAsync(invocation.Arguments, invocation.Method);
        }

        [SuppressMessage("", "CA1508", Justification = "false positive")]
        private async Task<object?> CallControllerAsync(
            IReadOnlyList<object?> arguments,
            MethodInfo method)
        {
            if (_methodValidation != null)
            {
                _methodValidation(method);
            }

            var callId = Guid.NewGuid();
            var (url, actionInfo) = await _getInfo.GetInfo<T>(arguments.ToList(), method, _actionInfoTransformersCaller);
            using var request = HttpRequestFactory.Create(
                actionInfo.HttpMethod,
                url,
                actionInfo.Body,
                callId,
                actionInfo.BodyFormat,
                actionInfo.Headers);
            CallDataDictionary.InsertEmptyDataToIndicateTestCall(callId);

            var result = await _webCaller.Call(request, actionInfo, new InvocationInfo(arguments, method));
            return await _resultFactory.Create<T>(result, callId.ToString(), method);
        }
    }

    public class InvocationInfo
    {
        public InvocationInfo(
            IEnumerable<object?> arguments,
            MethodInfo methodInfo)
        {
            Arguments = arguments;
            MethodInfo = methodInfo;
        }

        public IEnumerable<object?> Arguments { get; }
        public MethodInfo MethodInfo { get; }
    }
}
