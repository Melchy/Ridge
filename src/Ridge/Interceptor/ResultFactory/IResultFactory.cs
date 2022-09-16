using Ridge.CallResult.Controller;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Ridge.Interceptor.ResultFactory
{
    internal interface IResultFactory
    {
        public Task<object> Create(
            HttpResponseMessage httpResponseMessage,
            string callId,
            MethodInfo methodInfo);

        Task<ControllerCallResult<TReturn>> CreateControllerCallResult<TReturn>(
            HttpResponseMessage httpResponseMessage,
            string callId);

        Task<ControllerCallResult> CreateControllerCallResult(
            HttpResponseMessage httpResponseMessage,
            string callId);
    }
}
