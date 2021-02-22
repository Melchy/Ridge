using Ridge.Interceptor;
using Ridge.Middlewares.Public;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Middlewares.Infrastructure
{
    internal class CallMiddlewareExecutor
    {
        private readonly CallMiddleware _callMiddlewareCurrent;
        private readonly CallMiddlewareDelegate _next;
        private readonly HttpRequestMessage _httpRequestMessage;
        private readonly IReadOnlyInvocationInformation _invocationInformation;

        public CallMiddlewareExecutor(CallMiddleware callMiddlewareCurrent,
            CallMiddlewareDelegate next,
            HttpRequestMessage httpRequestMessage,
            IReadOnlyInvocationInformation invocationInformation)
        {
            _callMiddlewareCurrent = callMiddlewareCurrent;
            _next = next;
            _httpRequestMessage = httpRequestMessage;
            _invocationInformation = invocationInformation;
        }

        public HttpResponseMessage Execute()
        {
            return _callMiddlewareCurrent.Invoke(_next, _httpRequestMessage, _invocationInformation);
        }
    }
}
