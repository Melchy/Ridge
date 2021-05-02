using Microsoft.AspNetCore.Mvc;
using Ridge.CallData;
using Ridge.CallResult.Controller;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Ridge.Interceptor.ResultFactory
{
    internal class ResultFactoryForController : IResultFactory
    {
        public async Task<object> Create<T>(HttpResponseMessage httpResponseMessage,
            string callId,
            MethodInfo methodInfo)
        {
            var actionReturnType = GeneralHelpers.GetReturnTypeOrGenericArgumentOfTask(methodInfo);
            var resultString = await httpResponseMessage.Content.ReadAsStringAsync();
            CallDataDto callDataDto = CallDataDictionary.GetData(callId);
            if (callDataDto.Exception != null)
            {
                ExceptionDispatchInfo.Capture(callDataDto.Exception).Throw();
                throw new InvalidOperationException("This is never thrown"); // this line is never reached
            }

            if (actionReturnType == typeof(ActionResult))
            {
                return new ControllerCallResult(httpResponseMessage, resultString, httpResponseMessage.StatusCode);
            }
            else if (GeneralHelpers.IsOrImplements(actionReturnType, typeof(ActionResult<>)))
            {
                var genericTypeOfActionResult = actionReturnType.GenericTypeArguments.First();
                var ridgeResult = GeneralHelpers.CreateInstance(
                    typeof(ControllerCallResult<>).MakeGenericType(genericTypeOfActionResult),
                    httpResponseMessage,
                    resultString,
                    httpResponseMessage.StatusCode);
                var actionResult = GeneralHelpers.CreateInstance(actionReturnType, ridgeResult);
                return actionResult;
            }
            else if (GeneralHelpers.IsOrImplements(actionReturnType, typeof(IActionResult)))
            {
                return new ControllerCallResult(httpResponseMessage, resultString, httpResponseMessage.StatusCode);
            }
            else
            {
                throw new InvalidOperationException($"Controller method must return {nameof(ActionResult)} or {nameof(ActionResult)}<T> or {nameof(IActionResult)}");
            }
        }
    }
}
