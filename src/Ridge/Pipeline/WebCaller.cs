using Ridge.Interceptor;
using Ridge.LogWriter;
using Ridge.Pipeline.Infrastructure;
using Ridge.Pipeline.Public;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Pipeline
{
    internal class WebCaller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogWriter? _logger;
        private readonly List<IHttpRequestPipelinePart> _pipelineParts;

        public WebCaller(HttpClient httpClient, ILogWriter? logger, List<IHttpRequestPipelinePart> pipelineParts)
        {
            _httpClient = httpClient;
            _logger = logger;
            _pipelineParts = pipelineParts;
        }
        public Task<HttpResponseMessage> Call(
            HttpRequestMessage httpRequestMessage,
            IReadOnlyActionInfo actionInfo,
            InvocationInfo invocationInfo)
        {
            var finalPipelinePart = new CallWebAppPipelinePart(_httpClient, _logger);
            return HttpRequestPipelineExecutor.Execute(_pipelineParts, finalPipelinePart, httpRequestMessage, actionInfo, invocationInfo);
        }
    }
}
