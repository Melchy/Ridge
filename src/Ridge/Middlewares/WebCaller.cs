using Microsoft.Extensions.Logging;
using Ridge.Interceptor;
using Ridge.Middlewares.DefaulMiddlewares;
using Ridge.Middlewares.Infrastructure;
using Ridge.Middlewares.Public;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Middlewares
{
    internal class WebCaller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly List<CallMiddleware> _middlewares;

        public WebCaller(HttpClient httpClient, ILogger logger, List<CallMiddleware> middlewares)
        {
            _httpClient = httpClient;
            _logger = logger;
            _middlewares = middlewares;
        }
        public Task<HttpResponseMessage> Call(HttpRequestMessage httpRequestMessage, IReadOnlyInvocationInformation invocationInformation)
        {
            var finalMiddleware = new CallWebAppMiddleware(_httpClient, _logger);
            return CallMiddlewareComposer.Execute(_middlewares, finalMiddleware, httpRequestMessage, invocationInformation);
        }
    }
}
