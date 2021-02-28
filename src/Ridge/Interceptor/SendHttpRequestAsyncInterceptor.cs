using Castle.DynamicProxy;
using Ridge.CallData;
using Ridge.Interceptor.ActionInfo;
using Ridge.Interceptor.InterceptorFactory;
using Ridge.Interceptor.ResultFactory;
using Ridge.Middlewares;
using Ridge.Results;
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
        private readonly PreCallMiddlewareCaller _preCallMiddlewareCaller;
        private readonly Action<MethodInfo>? _methodValidation;

        internal SendHttpRequestAsyncInterceptor(
            WebCaller webCaller,
            IGetInfo getInfo,
            IResultFactory resultFactory,
            PreCallMiddlewareCaller preCallMiddlewareCaller,
            Action<MethodInfo>? methodValidation = null)
        {
            _webCaller = webCaller;
            _getInfo = getInfo;
            _resultFactory = resultFactory;
            _preCallMiddlewareCaller = preCallMiddlewareCaller;
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

        [SuppressMessage("","CA1508", Justification = "false positive")]
        private async Task<object?> CallControllerAsync(IReadOnlyList<object> arguments, MethodInfo method)
        {
            if (_methodValidation != null)
            {
                _methodValidation(method);
            }
            var callId = Guid.NewGuid();
            var actionInfo = await _getInfo.GetInfo<T>(arguments.ToList(), method, _preCallMiddlewareCaller);
            using var request = HttpRequestFactory.Create(
                actionInfo.ActionArgumentsInfo.HttpMethod,
                actionInfo.Url,
                actionInfo.ActionArgumentsInfo.Body,
                callId,
                actionInfo.ActionArgumentsInfo.BodyFormat,
                actionInfo.ActionArgumentsInfo.HeaderParams);
            CallDataDictionary.InsertEmptyDataToIndicateTestCall(callId);

            var result = await _webCaller.Call(request, actionInfo.ActionArgumentsInfo);

            return await _resultFactory.Create<T>(result, callId.ToString(), method);
        }
    }
}
