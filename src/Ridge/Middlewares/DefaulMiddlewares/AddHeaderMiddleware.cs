using Ridge.Interceptor;
using Ridge.Middlewares.Public;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Middlewares.DefaulMiddlewares
{
    internal class AddHeaderMiddleware : CallMiddleware
    {
        private readonly string _key;
        private readonly string _value;

        public AddHeaderMiddleware(string key, string value)
        {
            _key = key;
            _value = value;
        }
        public override HttpResponseMessage Invoke(
            CallMiddlewareDelegate next,
            HttpRequestMessage httpRequestMessage,
            IReadOnlyInvocationInformation invocationInformation)
        {
            httpRequestMessage.Headers.Add(_key, _value);
            return next();
        }
    }
}
