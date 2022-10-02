using Ridge.ActionInfo;
using Ridge.Interceptor;
using Ridge.LogWriter;
using Ridge.Pipeline.Infrastructure;
using Ridge.Pipeline.Public;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Pipeline
{
    /// <summary>
    ///     Executes all <see cref="IHttpRequestPipelinePart" /> and then calls server.
    /// </summary>
    public class WebCaller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogWriter _logger;
        private readonly List<IHttpRequestPipelinePart> _pipelineParts;

        /// <summary>
        ///     Create Web Caller
        /// </summary>
        /// <param name="httpClient">Http client which calls the request.</param>
        /// <param name="logger">Logger to log request and response.</param>
        /// <param name="pipelineParts">Pipeline to be executed.</param>
        public WebCaller(
            HttpClient httpClient,
            ILogWriter logger,
            List<IHttpRequestPipelinePart> pipelineParts)
        {
            _httpClient = httpClient;
            _logger = logger;
            _pipelineParts = pipelineParts;
        }

        /// <summary>
        ///     Executes all <see cref="IHttpRequestPipelinePart" /> and then calls server.
        /// </summary>
        /// <param name="httpRequestMessage">Message to transform and then send to server.</param>
        /// <param name="actionInfo">
        ///     Read only action info which can be used in <see cref="IHttpRequestPipelinePart" /> to gather
        ///     information.
        /// </param>
        /// <param name="methodInvocationInfo">
        ///     Invocation info which can be used in <see cref="IHttpRequestPipelinePart" /> to
        ///     gather information.
        /// </param>
        /// <returns></returns>
        public Task<HttpResponseMessage> Call(
            HttpRequestMessage httpRequestMessage,
            IReadOnlyActionInfo actionInfo,
            MethodInvocationInfo methodInvocationInfo)
        {
            var finalPipelinePart = new CallWebAppPipelinePart(_httpClient, _logger);
            return HttpRequestPipelineExecutor.Execute(_pipelineParts, finalPipelinePart, httpRequestMessage, actionInfo, methodInvocationInfo);
        }
    }
}
