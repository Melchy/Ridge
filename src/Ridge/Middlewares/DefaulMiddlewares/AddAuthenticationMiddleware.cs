using Ridge.Interceptor;
using Ridge.Middlewares.Public;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Ridge.Middlewares.DefaulMiddlewares
{
    internal class AddAuthenticationMiddleware : CallMiddleware
    {
        private readonly AuthenticationHeaderValue _authenticationHeaderValue;


        public AddAuthenticationMiddleware(AuthenticationHeaderValue authenticationHeaderValue)
        {
            _authenticationHeaderValue = authenticationHeaderValue;
        }
        public override async Task<HttpResponseMessage> Invoke(
            CallMiddlewareDelegate next,
            HttpRequestMessage httpRequestMessage,
            IReadOnlyInvocationInformation invocationInformation)
        {
            httpRequestMessage.Headers.Authorization = _authenticationHeaderValue;
            return await next();
        }
    }
}
