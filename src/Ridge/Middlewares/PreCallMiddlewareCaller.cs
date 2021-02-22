using Ridge.Interceptor;
using Ridge.Middlewares.DefaulMiddlewares;
using Ridge.Middlewares.Infrastructure;
using Ridge.Middlewares.Public;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ridge.Middlewares
{
    internal class PreCallMiddlewareCaller
    {
        private readonly List<PreCallMiddleware> _middlewares;

        public PreCallMiddlewareCaller(List<PreCallMiddleware> middlewares)
        {
            _middlewares = middlewares;
        }

        public void Call(IInvocationInformation invocationInformation)
        {
            var finalMiddleware = new FinalPreCallMiddleware();
            PreCallMiddlewareComposer.Execute(_middlewares, finalMiddleware, invocationInformation);
        }
    }
}
