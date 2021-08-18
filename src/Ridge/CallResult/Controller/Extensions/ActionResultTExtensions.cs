using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;

namespace Ridge.CallResult.Controller.Extensions
{
    public static class ActionResultTExtensions
    {
        [SuppressMessage("", "RS0030")]
        public static T GetResult<T>(this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult<T>)actionResult.Result;
            return ridgeResult.Result;
        }

        [SuppressMessage("", "RS0030")]
        public static HttpResponseMessage HttpResponseMessage<T>(this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult.Result;
            return ridgeResult.HttpResponseMessage;
        }

        [SuppressMessage("", "RS0030")]
        public static string ResultAsString<T>(this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult.Result;
            return ridgeResult.ResultAsString;
        }

        [SuppressMessage("", "RS0030")]
        public static HttpStatusCode StatusCode<T>(this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult<T>)actionResult.Result;
            return ridgeResult.HttpResponseMessage.StatusCode;
        }

        [SuppressMessage("", "RS0030")]
        public static bool IsSuccessStatusCode<T>(this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult.Result;
            return ridgeResult.IsSuccessStatusCode;
        }

        [SuppressMessage("", "RS0030")]
        public static bool IsRedirectStatusCode<T>(this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult.Result;
            return ridgeResult.IsRedirectStatusCode;
        }

        [SuppressMessage("", "RS0030")]
        public static bool IsClientErrorStatusCode<T>(this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult.Result;
            return ridgeResult.IsClientErrorStatusCode;
        }

        [SuppressMessage("", "RS0030")]
        public static bool IsServerErrorStatusCode<T>(this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult.Result;
            return ridgeResult.IsServerErrorStatusCode;
        }

        [SuppressMessage("", "RS0030")]
        public static ControllerCallResult<T> Unwrap<T>(this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            return (ControllerCallResult<T>)actionResult.Result;
        }

        [SuppressMessage("", "RS0030")]
        private static void EnsureActionResultContainsControllerCallResult<T>(ActionResult<T> actionResult)
        {
            if (actionResult.Result is not ControllerCallResult<T>)
            {
                throw new InvalidOperationException("Action result is incorrect. Action result must contain ControllerCallResult in Result");
            }
        }
    }
}
