using Castle.DynamicProxy;
using Ridge.CallData;
using Ridge.Interceptor.ActionInfo;
using Ridge.Interceptor.InterceptorFactory;
using Ridge.Interceptor.ResultFactory;
using Ridge.Pipeline;
using Ridge.Results;
using Ridge.Transformers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Ridge.Interceptor
{
    public class SendHttpRequestAsyncInterceptor<T> : IAsyncInterceptor
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

        /// <summary>
        /// Sync over async is tricky.
        /// We can not use CallControllerAsync(invocation.Arguments, invocation.Method).GetAwaiter().GetResult() or
        /// CallControllerAsync(invocation.Arguments, invocation.Method).Wait() these calls can cause deadlock.
        /// Issue: Issue: https://github.com/xunit/xunit/issues/864?fbclid=IwAR1d4nLltbDGyO6SlhCYmBBskw_OJfycxBxRf_82gR_M-g-68lLmcFNGjFU
        ///
        /// Task.Run(() => CallControllerAsync(invocation.Arguments, invocation.Method)).GetAwaiter().GetResult()
        /// causes thread starvation when used in xunit test on single processor machine.
        /// Issue: https://github.com/JSkimming/Castle.Core.AsyncInterceptor/pull/54#issuecomment-480953342
        /// This solution replaces the synchronization context which should cause that new task are not limited by number of threads
        /// dictated in xunit synchronization context.
        /// </summary>
        /// <param name="invocation"></param>
        public void InterceptSynchronous(IInvocation invocation)
        {
            //var foo = SynchronizationContext.Current;
            //using (NoSynchronizationContextScope.Enter())

            {
                invocation.ReturnValue = Task.Run(() => CallControllerAsync(invocation.Arguments, invocation.Method)).GetAwaiter().GetResult();
            }
        }

        public void InterceptAsynchronous(IInvocation invocation)
        {
            throw new InvalidOperationException($"Method must return Task<{nameof(ControllerResult)}>");
        }

        public void InterceptAsynchronous<TResult>(IInvocation invocation)
        {
            invocation.ReturnValue = InternalInterceptAsynchronous<TResult>(invocation);
        }

        private async Task<TResult> InternalInterceptAsynchronous<TResult>(IInvocation invocation)
        {
            var foo = await CallControllerAsync(invocation.Arguments, invocation.Method);
            return (TResult)foo!;
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
