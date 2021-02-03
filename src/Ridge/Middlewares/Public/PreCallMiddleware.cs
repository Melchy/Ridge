using Ridge.Interceptor;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Middlewares.Public
{
    public abstract class PreCallMiddleware
    {
        public abstract Task Invoke(PreCallMiddlewareDelegate next, IInvocationInformation invocationInformation);
    }
}
