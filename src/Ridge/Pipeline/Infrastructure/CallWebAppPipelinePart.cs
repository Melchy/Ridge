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
        private readonly ILogWriter _logger;

        public CallWebAppPipelinePart(
            HttpClient httpClient,
            ILogWriter logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<HttpResponseMessage> InvokeAsync(
            Func<Task<HttpResponseMessage>> next,
            HttpRequestMessage httpRequestMessage,
            IReadOnlyActionInfo actionInfo,
            InvocationInfo invocationInfo)
        {
            var requestBody = httpRequestMessage.Content;
            var requestBodyAsString = requestBody == null ? null : await requestBody.ReadAsStringAsync();
            _logger.WriteLine("Request:");
            _logger.WriteLine($"{httpRequestMessage}");
            _logger.WriteLine("Body:");
            _logger.WriteLine($"{requestBodyAsString}");
            _logger.WriteLine("");

            var response = await _httpClient.SendAsync(httpRequestMessage);

            var responseBodyAsString = await response.Content.ReadAsStringAsync();
            _logger.WriteLine("Response:");
            _logger.WriteLine($"{response}");
            _logger.WriteLine("Body:");
            _logger.WriteLine($"{responseBodyAsString}");
            _logger.WriteLine("");
            

            return response;
        }
    }
}
