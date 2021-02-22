using Castle.DynamicProxy;
using Ridge.CallData;
using Ridge.Interceptor.ActionInfo;
using Ridge.Interceptor.InterceptorFactory;
using Ridge.Interceptor.ResultFactory;
using Ridge.Middlewares;
using Ridge.Results;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Interceptor
{
    public class SendHttpRequestAsyncInterceptor<T> : IInterceptor
    {
        private readonly WebCaller _webCaller;
        private readonly IGetInfo _getInfo;
        private readonly IResultFactory _resultFactory;
        private readonly PreCallMiddlewareCaller _preCallMiddlewareCaller;

        internal SendHttpRequestAsyncInterceptor(
            WebCaller webCaller,
            IGetInfo getInfo,
            IResultFactory resultFactory,
            PreCallMiddlewareCaller preCallMiddlewareCaller)
        {
            _webCaller = webCaller;
            _getInfo = getInfo;
            _resultFactory = resultFactory;
            _preCallMiddlewareCaller = preCallMiddlewareCaller;
        }

        [SuppressMessage("", "CA1508", Justification="False positive")]
        public void Intercept(
            IInvocation invocation)
        {
            var callId = Guid.NewGuid();
            var actionInfo = _getInfo.GetInfo<T>(invocation.Arguments.ToList(), invocation.MethodInvocationTarget, _preCallMiddlewareCaller);
            using var request = HttpRequestFactory.Create(
                actionInfo.HttpMethod,
                actionInfo.Url,
                actionInfo.ActionArgumentsInfo.Body,
                callId,
                actionInfo.ActionArgumentsInfo.BodyFormat,
                actionInfo.ActionArgumentsInfo.HeaderParams);
            CallDataDictionary.InsertEmptyDataToIndicateTestCall(callId);

            var result = _webCaller.Call(request, actionInfo.ActionArgumentsInfo);

            var returnValue = _resultFactory.Create<T>(result, callId.ToString(), invocation.MethodInvocationTarget);
            var returnType = invocation.Method.ReturnType;
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                invocation.ReturnValue = Task.FromResult((dynamic)returnValue);
                return;
            }
            invocation.ReturnValue = returnValue;
        }
    }
}
