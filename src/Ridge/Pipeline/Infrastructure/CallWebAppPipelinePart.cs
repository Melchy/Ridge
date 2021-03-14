using Newtonsoft.Json;
using Ridge.Interceptor;
using Ridge.LogWriter;
using Ridge.Pipeline.Public;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Pipeline.Infrastructure
{
    internal class CallWebAppPipelinePart : IHttpRequestPipelinePart
    {
        private readonly HttpClient _httpClient;
        private readonly ILogWriter? _logger;

        public CallWebAppPipelinePart(HttpClient httpClient, ILogWriter? logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task<HttpResponseMessage> Invoke(
            Func<Task<HttpResponseMessage>> next,
            HttpRequestMessage httpRequestMessage,
            IReadOnlyActionInfo actionInfo,
            InvocationInfo invocationInfo)
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
