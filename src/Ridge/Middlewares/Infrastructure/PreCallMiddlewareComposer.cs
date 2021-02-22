using Ridge.Interceptor;
using Ridge.Middlewares.DefaulMiddlewares;
using Ridge.Middlewares.Public;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Middlewares.Infrastructure
{
    internal static class PreCallMiddlewareComposer
    {
        public static void Execute(
            List<PreCallMiddleware> preCallMiddlewares,
            FinalPreCallMiddleware finalPreCallMiddleware,
            IInvocationInformation invocationInformation)
        {
            preCallMiddlewares.Reverse();
            var lastCallExecutor = new PreCallMiddlewareExecutor(finalPreCallMiddleware, null!, null!);
            var previous = lastCallExecutor;
            foreach (var callMiddleware in preCallMiddlewares)
            {
                previous = new PreCallMiddlewareExecutor(callMiddleware, previous.Execute, invocationInformation);
            }
            previous.Execute();
        }
    }
}
