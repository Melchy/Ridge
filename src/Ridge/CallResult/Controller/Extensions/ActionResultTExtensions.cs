using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;

namespace Ridge.CallResult.Controller.Extensions
{
    /// <summary>
    ///     Extension methods allowing work with <see cref="ControllerCallResult{TResult}" /> wrapped in
    ///     <see cref="ActionResult{TValue}" />.
    /// </summary>
    public static class ActionResultTExtensions
    {
        /// <summary>
        ///     Tries to deserialize response to T.
        /// </summary>
        /// <param name="actionResult"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [SuppressMessage("", "RS0030")]
        public static T GetResult<T>(
            this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult<T>)actionResult.Result;
            return ridgeResult.Result;
        }

        /// <summary>
        ///     Response from the server.
        /// </summary>
        /// <param name="actionResult"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [SuppressMessage("", "RS0030")]
        public static HttpResponseMessage HttpResponseMessage<T>(
            this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult.Result;
            return ridgeResult.HttpResponseMessage;
        }

        /// <summary>
        ///     Response content as a string.
        /// </summary>
        /// <param name="actionResult"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [SuppressMessage("", "RS0030")]
        public static string ResultAsString<T>(
            this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult.Result;
            return ridgeResult.ResultAsString;
        }

        /// <summary>
        ///     Status code returned from server.
        /// </summary>
        [SuppressMessage("", "RS0030")]
        public static HttpStatusCode StatusCode<T>(
            this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult<T>)actionResult.Result;
            return ridgeResult.HttpResponseMessage.StatusCode;
        }

        /// <summary>
        ///     Indicates if status code has number between 200 and 300
        /// </summary>
        [SuppressMessage("", "RS0030")]
        public static bool IsSuccessStatusCode<T>(
            this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult.Result;
            return ridgeResult.IsSuccessStatusCode;
        }

        /// <summary>
        ///     Indicates if status code has number between 300 and 400
        /// </summary>
        [SuppressMessage("", "RS0030")]
        public static bool IsRedirectStatusCode<T>(
            this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult.Result;
            return ridgeResult.IsRedirectStatusCode;
        }

        /// <summary>
        ///     Indicates if status code has number between 400 and 500
        /// </summary>
        [SuppressMessage("", "RS0030")]
        public static bool IsClientErrorStatusCode<T>(
            this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult.Result;
            return ridgeResult.IsClientErrorStatusCode;
        }

        /// <summary>
        ///     Indicates if status code has number between 500 and 600
        /// </summary>
        [SuppressMessage("", "RS0030")]
        public static bool IsServerErrorStatusCode<T>(
            this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            var ridgeResult = (ControllerCallResult)actionResult.Result;
            return ridgeResult.IsServerErrorStatusCode;
        }

        /// <summary>
        ///     Get representation of controller response from this <see cref="ActionResult{TValue}" />.
        /// </summary>
        /// <param name="actionResult"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [SuppressMessage("", "RS0030")]
        public static ControllerCallResult<T> Unwrap<T>(
            this ActionResult<T> actionResult)
        {
            EnsureActionResultContainsControllerCallResult(actionResult);
            return (ControllerCallResult<T>)actionResult.Result;
        }

        [SuppressMessage("", "RS0030")]
        private static void EnsureActionResultContainsControllerCallResult<T>(
            ActionResult<T> actionResult)
        {
            if (actionResult.Result is not ControllerCallResult<T>)
            {
                throw new InvalidOperationException("Action result is incorrect. Action result must contain ControllerCallResult in Result");
            }
        }
    }
}
