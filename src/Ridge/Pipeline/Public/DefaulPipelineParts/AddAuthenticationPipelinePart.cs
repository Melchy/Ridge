using Ridge.Interceptor;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Ridge.Pipeline.Public.DefaulPipelineParts
{
    internal class AddAuthenticationPipelinePart : IHttpRequestPipelinePart
    {
        private readonly AuthenticationHeaderValue _authenticationHeaderValue;


        public AddAuthenticationPipelinePart(AuthenticationHeaderValue authenticationHeaderValue)
        {
            _authenticationHeaderValue = authenticationHeaderValue;
        }
        public async Task<HttpResponseMessage> InvokeAsync(
            Func<Task<HttpResponseMessage>> next,
            HttpRequestMessage httpRequestMessage,
            IReadOnlyActionInfo actionInfo,
            InvocationInfo invocationInfo)
        {
            httpRequestMessage.Headers.Authorization = _authenticationHeaderValue;
            return await next();
        }
    }
}
