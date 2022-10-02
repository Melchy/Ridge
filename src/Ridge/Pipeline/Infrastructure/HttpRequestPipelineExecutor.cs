using Ridge.ActionInfo;
using Ridge.Interceptor;
using Ridge.Pipeline.Public;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Pipeline.Infrastructure
{
    internal static class HttpRequestPipelineExecutor
    {
        public static async Task<HttpResponseMessage> Execute(
            List<IHttpRequestPipelinePart> pipelineParts,
            CallWebAppPipelinePart callWebAppPipelinePart,
            HttpRequestMessage httpRequestMessage,
            IReadOnlyActionInfo actionInfo,
            MethodInvocationInfo methodInvocationInfo)
        {
            pipelineParts.Reverse();
            Wrapper lastCallExecutor = new(callWebAppPipelinePart, null!, httpRequestMessage, actionInfo, methodInvocationInfo);
            var previous = lastCallExecutor;
            foreach (var pipelinePart in pipelineParts)
            {
                previous = new Wrapper(pipelinePart, previous.Execute, httpRequestMessage, actionInfo, methodInvocationInfo);
            }

            return await previous.Execute();
        }

        private class Wrapper
        {
            private readonly IHttpRequestPipelinePart _pipelinePartCurrent;
            private readonly Func<Task<HttpResponseMessage>> _next;
            private readonly HttpRequestMessage _httpRequestMessage;
            private readonly IReadOnlyActionInfo _actionInfo;
            private readonly MethodInvocationInfo _methodInvocationInfo;

            public Wrapper(
                IHttpRequestPipelinePart pipelinePartCurrent,
                Func<Task<HttpResponseMessage>> next,
                HttpRequestMessage httpRequestMessage,
                IReadOnlyActionInfo actionInfo,
                MethodInvocationInfo methodInvocationInfo)
            {
                _pipelinePartCurrent = pipelinePartCurrent;
                _next = next;
                _httpRequestMessage = httpRequestMessage;
                _actionInfo = actionInfo;
                _methodInvocationInfo = methodInvocationInfo;
            }

            public async Task<HttpResponseMessage> Execute()
            {
                return await _pipelinePartCurrent.InvokeAsync(_next, _httpRequestMessage, _actionInfo, _methodInvocationInfo);
            }
        }
    }
}
