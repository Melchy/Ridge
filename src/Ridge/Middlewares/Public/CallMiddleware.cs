using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Middlewares.Public
{
    public abstract class CallMiddleware
    {
        public abstract Task<HttpResponseMessage> Invoke(CallMiddlewareDelegate next, HttpRequestMessage httpRequestMessage);
    }
}
