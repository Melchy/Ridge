using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Ridge.Interceptor.ResultFactory
{
    internal interface IResultFactory
    {
        public Task<object> Create<T>(
            HttpResponseMessage httpResponseMessage,
            string callId,
            MethodInfo methodInfo);
    }
}
