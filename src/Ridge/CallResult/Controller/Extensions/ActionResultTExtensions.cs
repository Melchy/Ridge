using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Net.Http;

namespace Ridge.CallResult.Controller.Extensions
{
    public static class ActionResultTExtensions
    {
        public static T GetResult<T>(this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult<T>)actionResult.Result;
            return ridgeResult.Result;
        }

        public static HttpResponseMessage HttpResponseMessage<T>(this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult.Result;
            return ridgeResult.HttpResponseMessage;
        }

        public static string ResultAsString<T>(this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult.Result;
            return ridgeResult.ResultAsString;
        }

        public static HttpStatusCode StatusCode<T>(this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult<T>)actionResult.Result;
            return ridgeResult.HttpResponseMessage.StatusCode;
        }

        public static bool IsSuccessStatusCode<T>(this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult.Result;
            return ridgeResult.IsSuccessStatusCode;
        }

        public static bool IsRedirectStatusCode<T>(this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult.Result;
            return ridgeResult.IsRedirectStatusCode;
        }

        public static bool IsClientErrorStatusCode<T>(this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult.Result;
            return ridgeResult.IsClientErrorStatusCode;
        }

        public static bool IsServerErrorStatusCode<T>(this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult.Result;
            return ridgeResult.IsServerErrorStatusCode;
        }

        public static ControllerCallResult<T> Unwrap<T>(this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            return (ControllerCallResult<T>)actionResult.Result;
        }

        private static void EnsureActionResultContainsControllerCallResult<T>(ActionResult<T> actionResult)
        {
            if (actionResult.Result is not ControllerCallResult<T>)
            {
                throw new InvalidOperationException("Action result is incorrect. Action result must contain ControllerCallResult in Result");
            }
        }
    }
}
