using Castle.DynamicProxy;
using Ridge.CallData;
using Ridge.Interceptor.ActionInfo;
using Ridge.Interceptor.ResultFactory;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Interceptor
{
    public class SendHttpRequestAsyncInterceptor<T> : AsyncInterceptorBase
    {
        private readonly IGetInfo _getInfo;
        private readonly IResultFactory _resultFactory;
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _requestBuilder;

        public SendHttpRequestAsyncInterceptor(
            Func<HttpRequestMessage, Task<HttpResponseMessage>> requestBuilder,
            IGetInfo getInfo,
            IResultFactory resultFactory)
        {
            _getInfo = getInfo;
            _resultFactory = resultFactory;
            _requestBuilder = requestBuilder;
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
            var actionInfo = _getInfo.GetInfo<T>(invocation.Arguments.ToList(), invocation.MethodInvocationTarget);
            using var request = HttpRequestFactory.Create(
                actionInfo.HttpMethod,
                actionInfo.Url,
                actionInfo.ActionArgumentsInfo.Body,
                callId,
                actionInfo.ActionArgumentsInfo.BodyFormat,
                actionInfo.ActionArgumentsInfo.HeaderParams);
            CallDataDictionary.InsertEmptyDataToIndicateTestCall(callId);


            var result = await _requestBuilder(request);

            var returnValue = await _resultFactory.Create<T>(result, callId.ToString(), invocation.MethodInvocationTarget);
            invocation.ReturnValue = returnValue;
            return (TResult)returnValue;
        }
    }
}
