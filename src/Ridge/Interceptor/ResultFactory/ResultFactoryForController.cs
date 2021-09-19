using Microsoft.AspNetCore.Mvc;
using Ridge.CallData;
using Ridge.CallResult.Controller;
using Ridge.Serialization;
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
        private readonly IRidgeSerializer _serializer;

        public ResultFactoryForController(
            IRidgeSerializer serializer)
        {
            _serializer = serializer;
        }

        public async Task<object> Create<T>(
            HttpResponseMessage httpResponseMessage,
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

            if (GeneralHelpers.IsOrImplements(actionReturnType, typeof(ActionResult<>)))
            {
                var genericTypeOfActionResult = actionReturnType.GenericTypeArguments.First();
                var ridgeResult = GeneralHelpers.CreateInstance(
                    typeof(ControllerCallResult<>).MakeGenericType(genericTypeOfActionResult),
                    httpResponseMessage,
                    resultString,
                    httpResponseMessage.StatusCode,
                    _serializer);
                var actionResult = GeneralHelpers.CreateInstance(actionReturnType, ridgeResult);
                return actionResult;
            }

            if (GeneralHelpers.IsOrImplements(actionReturnType, typeof(IActionResult)))
            {
                return new ControllerCallResult(httpResponseMessage, resultString, httpResponseMessage.StatusCode);
            }

            throw new InvalidOperationException($"Controller method must return {nameof(ActionResult)} or {nameof(ActionResult)}<T> or {nameof(IActionResult)}");
        }
    }
}
