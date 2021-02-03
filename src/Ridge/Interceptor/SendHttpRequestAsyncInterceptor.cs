using Castle.DynamicProxy;
using Ridge.CallData;
using Ridge.Interceptor.ActionInfo;
using Ridge.Interceptor.InterceptorFactory;
using Ridge.Interceptor.ResultFactory;
using Ridge.Middlewares;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Interceptor
{
    public class SendHttpRequestAsyncInterceptor<T> : AsyncInterceptorBase
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

        /// <summary>
        /// This method is called if method returns
        /// Task, void
        /// </summary>
        protected override Task InterceptAsync(IInvocation invocation, Func<IInvocation, Task> proceed)
        {
            throw new InvalidOperationException($"Interceptor works only when page returns CustomActionResult<T>.");
        }

        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, Func<IInvocation, Task<TResult>> proceed)
        {
            // Force asynchronous run. Synchronous run can cause race condition.
            await Task.Yield();
            var callId = Guid.NewGuid();
            var actionInfo = await _getInfo.GetInfo<T>(invocation.Arguments.ToList(), invocation.MethodInvocationTarget, _preCallMiddlewareCaller);
            using var request = HttpRequestFactory.Create(
                actionInfo.HttpMethod,
                actionInfo.Url,
                actionInfo.ActionArgumentsInfo.Body,
                callId,
                actionInfo.ActionArgumentsInfo.BodyFormat,
                actionInfo.ActionArgumentsInfo.HeaderParams);
            CallDataDictionary.InsertEmptyDataToIndicateTestCall(callId);


            var result = await _webCaller.Call(request, actionInfo.ActionArgumentsInfo);

            var returnValue = await _resultFactory.Create<T>(result, callId.ToString(), invocation.MethodInvocationTarget);
            invocation.ReturnValue = returnValue;
            return (TResult)returnValue;
        }
    }
}
