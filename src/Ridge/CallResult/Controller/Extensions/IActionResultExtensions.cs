using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Net.Http;

namespace Ridge.CallResult.Controller.Extensions
{
    /// <summary>
    ///     Extension methods allowing work with <see cref="ControllerCallResult" /> wrapped in <see cref="IActionResult" />.
    /// </summary>
    public static class IActionResultExtensions
    {
        /// <summary>
        ///     Tries to deserialize response to T.
        /// </summary>
        /// <param name="actionResult"></param>
        /// <returns></returns>
        public static HttpResponseMessage HttpResponseMessage(
            this IActionResult actionResult)
        {
            EnsureActionResultIsContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult;
            return ridgeResult.HttpResponseMessage;
        }

        /// <summary>
        ///     Response content as a string.
        /// </summary>
        /// <param name="actionResult"></param>
        /// <returns></returns>
        public static string ResultAsString(
            this IActionResult actionResult)
        {
            EnsureActionResultIsContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult;
            return ridgeResult.ResultAsString;
        }

        /// <summary>
        ///     Status code returned from server.
        /// </summary>
        public static HttpStatusCode StatusCode(
            this IActionResult actionResult)
        {
            EnsureActionResultIsContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult;
            return ridgeResult.HttpResponseMessage.StatusCode;
        }

        /// <summary>
        ///     Indicates if status code has number between 200 and 300
        /// </summary>
        public static bool IsSuccessStatusCode(
            this IActionResult actionResult)
        {
            EnsureActionResultIsContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult;
            return ridgeResult.IsSuccessStatusCode;
        }

        /// <summary>
        ///     Indicates if status code has number between 300 and 400
        /// </summary>
        public static bool IsRedirectStatusCode(
            this IActionResult actionResult)
        {
            EnsureActionResultIsContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult;
            return ridgeResult.IsRedirectStatusCode;
        }

        /// <summary>
        ///     Indicates if status code has number between 400 and 500
        /// </summary>
        public static bool IsClientErrorStatusCode(
            this IActionResult actionResult)
        {
            EnsureActionResultIsContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult;
            return ridgeResult.IsClientErrorStatusCode;
        }

        /// <summary>
        ///     Indicates if status code has number between 500 and 600
        /// </summary>
        public static bool IsServerErrorStatusCode(
            this IActionResult actionResult)
        {
            EnsureActionResultIsContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult;
            return ridgeResult.IsServerErrorStatusCode;
        }


        /// <summary>
        ///     Get representation of controller response from this <see cref="ActionResult{TValue}" />.
        /// </summary>
        /// <param name="actionResult"></param>
        /// <returns></returns>
        public static ControllerCallResult Unwrap(
            this IActionResult actionResult)
        {
            EnsureActionResultIsContainsControllerCallResult(actionResult);
            return (ControllerCallResult)actionResult;
        }

        private static void EnsureActionResultIsContainsControllerCallResult(
            IActionResult actionResult)
        {
            if (actionResult is not ControllerCallResult)
            {
                throw new InvalidOperationException("IAction result is incorrect. IAction result must be ControllerCallResult");
            }
        }
    }
}
