using FluentReflections;
using Ridge.CallData;
using Ridge.Results;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Ridge.Interceptor.ResultFactory
{
    internal class ResultFactoryForController : IResultFactory
    {
        #pragma warning disable CS1998
        public object Create<T>(HttpResponseMessage httpResponseMessage,
            string callId,
            MethodInfo methodInfo)
        {
            var actionReturnType = GeneralHelpers.GetReturnTypeOrGenericArgumentOfTask(methodInfo);
            var resultString = Task.Run(async () => httpResponseMessage.Content.ReadAsStringAsync().Result).GetAwaiter().GetResult();
            CallDataDto callDataDto = CallDataDictionary.GetData(callId);
            if (callDataDto.Exception != null)
            {
                ExceptionDispatchInfo.Capture(callDataDto.Exception).Throw();
                throw new InvalidOperationException("This is never thrown"); // this line is never reached
            }

            if (actionReturnType == typeof(ControllerResult))
            {
                return GeneralHelpers.CreateInstance(actionReturnType, httpResponseMessage, resultString, httpResponseMessage.StatusCode);
            }
            else if(actionReturnType.Reflection().IsOrImplements(typeof(ControllerResult<>)))
            {
                return GeneralHelpers.CreateInstance(actionReturnType, httpResponseMessage, resultString, httpResponseMessage.StatusCode);
            }
            else
            {
                throw new InvalidOperationException($"Controller method must return {nameof(ControllerResult)} or {nameof(ControllerResult)}<T>");
            }
        }
    }
    #pragma warning restore CS1998
}
