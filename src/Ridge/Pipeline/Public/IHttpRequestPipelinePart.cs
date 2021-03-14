using Ridge.Interceptor;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Pipeline.Public
{
    public interface IHttpRequestPipelinePart
    {
        /// <summary>
        /// This method is invoked right before application is called.
        /// PipelineParts are executed in order in which they were registered.
        /// </summary>
        /// <param name="next">Calls next pipelinePart. Last pipelinePart calls the app.</param>
        /// <param name="httpRequestMessage">Request which will be used to call the app. This request should be transformed in your pipeline part.</param>
        /// <param name="actionInfo">Read only information about action which will be called.</param>
        /// <param name="invocationInfo">Information about method which was called.</param>
        /// <returns></returns>
        public Task<HttpResponseMessage> Invoke(Func<Task<HttpResponseMessage>> next, HttpRequestMessage httpRequestMessage, IReadOnlyActionInfo actionInfo, InvocationInfo invocationInfo);
    }
}
