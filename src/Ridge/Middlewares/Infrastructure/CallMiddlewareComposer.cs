using Ridge.Interceptor;
using Ridge.Middlewares.DefaulMiddlewares;
using Ridge.Middlewares.Public;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Middlewares.Infrastructure
{
    internal static class CallMiddlewareComposer
    {
        public static HttpResponseMessage Execute(
            List<CallMiddleware> callMiddlewares,
            CallWebAppMiddleware callWebAppMiddleware,
            HttpRequestMessage httpRequestMessage,
            IReadOnlyInvocationInformation invocationInformation)
        {
            callMiddlewares.Reverse();
            var lastCallExecutor = new CallMiddlewareExecutor(callWebAppMiddleware, null!, httpRequestMessage, invocationInformation);
            var previous = lastCallExecutor;
            foreach (var callMiddleware in callMiddlewares)
            {
                previous = new CallMiddlewareExecutor(callMiddleware, previous.Execute, httpRequestMessage, invocationInformation);
            }
            return previous.Execute();
        }
    }
}
