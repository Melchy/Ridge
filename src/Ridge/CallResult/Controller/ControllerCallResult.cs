using Microsoft.AspNetCore.Mvc;
using Ridge.CallResult.Controller.Extensions;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.CallResult.Controller
{
    /// <summary>
    ///     This class represents response from controller.
    ///     Be aware that this is not real ActionResult.
    ///     ControllerCallResult inherits ActionResult and IActionResult only to satisfy compiler.
    /// </summary>
    public class ControllerCallResult : ActionResult, IActionResult
    {
        /// <summary>
        ///     Response returned from server.
        /// </summary>
        public HttpResponseMessage HttpResponseMessage { get; }

        /// <summary>
        ///     Response content as a string.
        /// </summary>
        public string ResultAsString { get; }

        /// <summary>
        ///     Status code returned from server.
        /// </summary>
        public HttpStatusCode StatusCode { get; }


        /// <summary>
        ///     Indicates if status code has number between 200 and 300
        /// </summary>
        public bool IsSuccessStatusCode => (int)StatusCode >= 200 && (int)StatusCode <= 299;

        /// <summary>
        ///     Indicates if status code has number between 300 and 400
        /// </summary>
        public bool IsRedirectStatusCode => (int)StatusCode >= 300 && (int)StatusCode <= 399;

        /// <summary>
        ///     Indicates if status code has number between 400 and 500
        /// </summary>
        public bool IsClientErrorStatusCode => (int)StatusCode >= 400 && (int)StatusCode <= 499;

        /// <summary>
        ///     Indicates if status code has number between 500 and 600
        /// </summary>
        public bool IsServerErrorStatusCode => (int)StatusCode >= 500 && (int)StatusCode <= 599;

        internal ControllerCallResult(
            HttpResponseMessage httpResponseMessage,
            string resultAsString,
            HttpStatusCode statusCode)
        {
            HttpResponseMessage = httpResponseMessage;
            ResultAsString = resultAsString;
            StatusCode = statusCode;
        }

        /// <summary>
        ///     Do not call this method. This method is implemented only to satisfy compiler.
        /// </summary>
        /// <param name="context"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public override void ExecuteResult(
            ActionContext context)
        {
            throw new InvalidOperationException($"This method should not be used in tests. Instead use extension methods '{nameof(ActionResultTExtensions.Unwrap)}'," +
                                                $" '{nameof(ActionResultTExtensions.HttpResponseMessage)}', '{nameof(ActionResultTExtensions.ResultAsString)}' and others. See readme.");
        }

        /// <summary>
        ///     Do not call this method. This method is implemented only to satisfy compiler.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public override Task ExecuteResultAsync(
            ActionContext context)
        {
            throw new InvalidOperationException($"This method should not be used in tests. Instead use extension methods: '{nameof(ActionResultTExtensions.Unwrap)}'," +
                                                $" '{nameof(ActionResultTExtensions.HttpResponseMessage)}', '{nameof(ActionResultTExtensions.ResultAsString)}' and others. See readme.");
        }
    }
}
