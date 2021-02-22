using Ridge.Interceptor;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Middlewares.Public
{
    public abstract class PreCallMiddleware
    {
        public abstract void Invoke(PreCallMiddlewareDelegate next, IInvocationInformation invocationInformation);
    }
}
