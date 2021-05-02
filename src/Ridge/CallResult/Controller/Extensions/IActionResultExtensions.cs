using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Net.Http;

namespace Ridge.CallResult.Controller.Extensions
{
    public static class IActionResultExtensions
    {
        public static HttpResponseMessage HttpResponseMessage(this IActionResult actionResult)
        {
            EnsureActionResultIsContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult;
            return ridgeResult.HttpResponseMessage;
        }

        public static string ResultAsString(this IActionResult actionResult)
        {
            EnsureActionResultIsContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult;
            return ridgeResult.ResultAsString;
        }

        public static HttpStatusCode StatusCode(this IActionResult actionResult)
        {
            EnsureActionResultIsContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult;
            return ridgeResult.HttpResponseMessage.StatusCode;
        }

        public static bool IsSuccessStatusCode(this IActionResult actionResult)
        {
            EnsureActionResultIsContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult;
            return ridgeResult.IsSuccessStatusCode;
        }

        public static bool IsRedirectStatusCode(this IActionResult actionResult)
        {
            EnsureActionResultIsContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult;
            return ridgeResult.IsRedirectStatusCode;
        }

        public static bool IsClientErrorStatusCode(this IActionResult actionResult)
        {
            EnsureActionResultIsContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult;
            return ridgeResult.IsClientErrorStatusCode;
        }

        public static bool IsServerErrorStatusCode(this IActionResult actionResult)
        {
            EnsureActionResultIsContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult;
            return ridgeResult.IsServerErrorStatusCode;
        }

        public static ControllerCallResult Unwrap(this IActionResult actionResult)
        {
            EnsureActionResultIsContainsControllerCallResult(actionResult);
            return (ControllerCallResult)actionResult;
        }

        private static void EnsureActionResultIsContainsControllerCallResult(IActionResult actionResult)
        {
            if (actionResult is not ControllerCallResult)
            {
                throw new InvalidOperationException("IAction result is incorrect. IAction result must be ControllerCallResult");
            }
        }
    }
}
