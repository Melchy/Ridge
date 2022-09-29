using Castle.DynamicProxy;
using Ridge.CallData;
using Ridge.CallResult.Controller;
using Ridge.Interceptor.ActionInfo;
using Ridge.Interceptor.ResultFactory;
using Ridge.LogWriter;
using Ridge.Pipeline;
using Ridge.Serialization;
using Ridge.Transformers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Ridge.Interceptor
{
    internal class SendHttpRequestAsyncInterceptor : IAsyncInterceptor
    {
        private readonly ControllerCaller _controllerCaller;

        internal SendHttpRequestAsyncInterceptor(
            RequestBuilder requestBuilder,
            ILogWriter? logWriter,
            HttpClient httpClient,
            IServiceProvider serviceProvider,
            IRidgeSerializer? serializer,
            Action<MethodInfo>? methodValidation = null)
        {
            _controllerCaller = new ControllerCaller(requestBuilder, logWriter, httpClient, serviceProvider, serializer, methodValidation);
        }

        /// <summary>
        ///     Sync over async is tricky.
        ///     We can not use CallControllerAsync(invocation.Arguments, invocation.Method).GetAwaiter().GetResult() or
        ///     CallControllerAsync(invocation.Arguments, invocation.Method).Wait() these calls can cause deadlock.
        ///     Issue: Issue:
        ///     https://github.com/xunit/xunit/issues/864?fbclid=IwAR1d4nLltbDGyO6SlhCYmBBskw_OJfycxBxRf_82gR_M-g-68lLmcFNGjFU
        ///     Task.Run(() => CallControllerAsync(invocation.Arguments, invocation.Method)).Wait()
        ///     causes thread starvation when used in xunit test on single processor machine.
        ///     Issue: https://github.com/JSkimming/Castle.Core.AsyncInterceptor/pull/54#issuecomment-480953342
        ///     This solution replaces the synchronization context which should cause that new task is not limited by number of
        ///     threads
        ///     dictated in xunit synchronization context.
        /// </summary>
        /// <param name="invocation"></param>
        public void InterceptSynchronous(
            IInvocation invocation)
        {
            using (NoSynchronizationContextScope.Enter())
            {
                invocation.ReturnValue = Task.Run(() => CallControllerAsync(invocation.Arguments, invocation.Method)).GetAwaiter().GetResult();
            }
        }

        public void InterceptAsynchronous(
            IInvocation invocation)
        {
            throw new InvalidOperationException($"Method must return can not return {nameof(Task)}");
        }

        public void InterceptAsynchronous<TResult>(
            IInvocation invocation)
        {
            invocation.ReturnValue = InternalInterceptAsynchronous<TResult>(invocation);
        }

        private async Task<TResult> InternalInterceptAsynchronous<TResult>(
            IInvocation invocation)
        {
            var result = await CallControllerAsync(invocation.Arguments, invocation.Method);
            return (TResult)result!;
        }

        [SuppressMessage("", "CA1508", Justification = "false positive")]
        private async Task<object?> CallControllerAsync(
            IReadOnlyList<object?> arguments,
            MethodInfo method)
        {
            return await _controllerCaller.CallActionFromInterceptor(arguments, method);
        }
    }

    /// <summary>
    ///     TODO
    /// </summary>
    public class ControllerCaller
    {
        private readonly WebCaller _webCaller;
        private readonly ControllerInfoProvider _infoProvider;
        private readonly IResultFactory _resultFactory;
        private readonly ActionInfoTransformersCaller _actionInfoTransformersCaller;
        private readonly IRidgeSerializer _serializer;
        private readonly Action<MethodInfo>? _methodValidation;


        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="requestBuilder"></param>
        /// <param name="logWriter"></param>
        /// <param name="httpClient"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="serializer"></param>
        /// <param name="methodValidation"></param>
        public ControllerCaller(
            RequestBuilder requestBuilder,
            ILogWriter? logWriter,
            HttpClient httpClient,
            IServiceProvider serviceProvider,
            IRidgeSerializer? serializer,
            Action<MethodInfo>? methodValidation = null)
        {
            _serializer = SerializerProvider.GetSerializer(serviceProvider, serializer);
            _infoProvider = new ControllerInfoProvider(serviceProvider);
            _methodValidation = methodValidation;
            _resultFactory = new ResultFactoryForController(_serializer);
            _webCaller = requestBuilder.BuildWebCaller(httpClient, logWriter ?? new NullLogWriter());
            _actionInfoTransformersCaller = requestBuilder.BuildActionInfoTransformerCaller();
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public async Task<object?> CallActionFromInterceptor(
            IReadOnlyList<object?> arguments,
            MethodInfo method)
        {
            if (_methodValidation != null)
            {
                _methodValidation(method);
            }

            var callId = Guid.NewGuid();
            var result = await CallActionCore(arguments, method, callId);
            return await _resultFactory.Create(result, callId.ToString(), method);
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public async Task<ControllerCallResult> CallAction(
            IEnumerable<object?> arguments,
            MethodInfo method)
        {
            var callId = Guid.NewGuid();
            var result = await CallActionCore(arguments, method, callId);
            return await _resultFactory.CreateControllerCallResult(result, callId.ToString());
        }

        /// <summary>
        ///     TODO
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="method"></param>
        /// <typeparam name="TReturn"></typeparam>
        /// <returns></returns>
        public async Task<ControllerCallResult<TReturn>> CallAction<TReturn>(
            IEnumerable<object?> arguments,
            MethodInfo method)
        {
            var callId = Guid.NewGuid();
            var result = await CallActionCore(arguments, method, callId);
            return await _resultFactory.CreateControllerCallResult<TReturn>(result, callId.ToString());
        }

        private async Task<HttpResponseMessage> CallActionCore(
            IEnumerable<object?> arguments,
            MethodInfo method,
            Guid callId)
        {
            var (url, actionInfo) = await _infoProvider.GetInfo(arguments.ToList(), method, _actionInfoTransformersCaller);
            using var request = HttpRequestFactory.Create(
                actionInfo.HttpMethod,
                url,
                actionInfo.Body,
                callId,
                actionInfo.BodyFormat,
                actionInfo.Headers,
                _serializer);
            CallDataDictionary.InsertEmptyDataToIndicateTestCall(callId);

            var result = await _webCaller.Call(request, actionInfo, new InvocationInfo(arguments, method));
            return result;
        }
    }
}
