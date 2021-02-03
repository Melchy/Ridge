using Ridge.Interceptor;
using Ridge.Middlewares.Public;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Middlewares.Infrastructure
{
    public class PreCallMiddlewareExecutor
    {
        private readonly PreCallMiddleware _preCallMiddlewareCurrent;
        private readonly PreCallMiddlewareDelegate _next;
        private readonly IInvocationInformation _invocationInformation;

        public PreCallMiddlewareExecutor(PreCallMiddleware preCallMiddlewareCurrent,
            PreCallMiddlewareDelegate next,
            IInvocationInformation invocationInformation)
        {
            _preCallMiddlewareCurrent = preCallMiddlewareCurrent;
            _next = next;
            _invocationInformation = invocationInformation;
        }

        public async Task Execute()
        {
            await _preCallMiddlewareCurrent.Invoke(_next, _invocationInformation);
        }
    }
}
