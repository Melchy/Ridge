using Newtonsoft.Json;
using Ridge.Interceptor;
using Ridge.Interceptor.InterceptorFactory;
using Ridge.LogWriter;
using Ridge.Middlewares.Public;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Middlewares.DefaulMiddlewares
{
    public class CallWebAppMiddleware : CallMiddleware
    {
        private readonly HttpClient _httpClient;
        private readonly ILogWriter? _logger;

        public CallWebAppMiddleware(HttpClient httpClient, ILogWriter? logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        public override async Task<HttpResponseMessage> Invoke(
            CallMiddlewareDelegate next,
            HttpRequestMessage httpRequestMessage,
            IReadOnlyInvocationInformation invocationInformation)
        {
            if (_logger != null)
            {
                var body = httpRequestMessage.Content;
                var bodyAsString = body == null ? null : await httpRequestMessage.Content.ReadAsStringAsync();
                var headers = JsonConvert.SerializeObject(httpRequestMessage.Headers);
                _logger.WriteLine("Ridge generated request:");
                _logger.WriteLine($"{httpRequestMessage.Method} {httpRequestMessage.RequestUri}");
                _logger.WriteLine($"{bodyAsString}");
                _logger.WriteLine($"Headers:");
                _logger.WriteLine($"{headers}");
                _logger.WriteLine("");
            }
            return await _httpClient.SendAsync(httpRequestMessage);
        }
    }
}
