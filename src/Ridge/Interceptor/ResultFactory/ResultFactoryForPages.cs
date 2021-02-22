using Ridge.CallData;
using Ridge.Results;
using System;
using System.Net.Http;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Ridge.Interceptor.ResultFactory
{
    internal class ResultFactoryForPages : IResultFactory
    {
        public object Create<T>(HttpResponseMessage httpResponseMessage, string callId, MethodInfo methodInfo)
        {
            var resultString = httpResponseMessage.Content.ReadAsStringAsync().Result;
            CallDataDto callDataDto = CallDataDictionary.GetData(callId);
            if (callDataDto.Exception != null)
            {
                ExceptionDispatchInfo.Capture(callDataDto.Exception).Throw();
                throw new InvalidOperationException("This is never thrown"); // this line is never reached
            }

            if (callDataDto.PageModel == null)
            {
                throw new InvalidOperationException($"Call data are null.");
            }

            return new PageResult<T>(httpResponseMessage, (T)callDataDto.PageModel, resultString, httpResponseMessage.StatusCode);
        }
    }
}
