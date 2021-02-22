using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ridge.Interceptor;
using Ridge.Middlewares.Public;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Middlewares.DefaulMiddlewares
{
    public class CallWebAppMiddleware : CallMiddleware
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public CallWebAppMiddleware(HttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        public override HttpResponseMessage Invoke(
            CallMiddlewareDelegate next,
            HttpRequestMessage httpRequestMessage,
            IReadOnlyInvocationInformation invocationInformation)
        {
            var body = httpRequestMessage.Content;
            var bodyAsString = body == null ? null : httpRequestMessage.Content.ReadAsStringAsync().Result;
            var headers = JsonConvert.SerializeObject(httpRequestMessage.Headers);
            _logger.LogError("Created request: Url - '{Url}', body - '{Body}', headers - '{Headers}', HttpMethod: {HttpMethod}",
                httpRequestMessage.RequestUri,
                bodyAsString,
                headers,
                httpRequestMessage.Method
            );
            return _httpClient.SendAsync(httpRequestMessage).Result;
        }
    }
}
