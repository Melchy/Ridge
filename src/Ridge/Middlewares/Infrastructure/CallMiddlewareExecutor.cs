using Ridge.Middlewares.Public;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Middlewares.Infrastructure
{
    internal class CallMiddlewareExecutor
    {
        private readonly CallMiddleware _callMiddlewareCurrent;
        private readonly CallMiddlewareDelegate _next;
        private HttpRequestMessage _httpRequestMessage;

        public CallMiddlewareExecutor(CallMiddleware callMiddlewareCurrent,
            CallMiddlewareDelegate next,
            HttpRequestMessage httpRequestMessage)
        {
            _callMiddlewareCurrent = callMiddlewareCurrent;
            _next = next;
            _httpRequestMessage = httpRequestMessage;
        }

        public Task<HttpResponseMessage> Execute()
        {
            return _callMiddlewareCurrent.Invoke(_next, _httpRequestMessage);
        }
    }
}
