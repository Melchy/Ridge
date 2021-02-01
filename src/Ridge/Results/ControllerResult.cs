using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Results
{
    /// <summary>
    /// This part of ControllerResult is used by asp.net
    /// </summary>
    public partial class ControllerResult : IActionResult, IResultWrapper
    {
        private ActionResult _actionResult { get; }

#pragma warning disable CS8618
        protected ControllerResult(ActionResult actionResult)
        {
            _actionResult = actionResult;
        }
#pragma warning restore CS8618

        [DebuggerHidden]
        public ActionResult GetInnerActionResult()
        {
            return _actionResult;
        }

        public static implicit operator ControllerResult(ActionResult actionResult)
        {
            return new ControllerResult(actionResult);
        }

        public Task ExecuteResultAsync(
            ActionContext context)
        {
            throw new InvalidOperationException("This method should never be called.");
        }
    }


    /// <summary>
    /// This part of class represents result of page call
    /// </summary>
    public partial class ControllerResult
    {
        public HttpResponseMessage HttpResponseMessage { get; }
        public string ResultAsString { get; }
        public HttpStatusCode StatusCode { get; }

        public bool IsSuccessStatusCode
        {
            get { return ((int)StatusCode >= 200) && ((int)StatusCode <= 299); }
        }

        public bool IsRedirectStatusCode
        {
            get { return ((int)StatusCode >= 300) && ((int)StatusCode <= 399); }
        }

        public bool IsClientErrorStatusCode
        {
            get { return ((int)StatusCode >= 400) && ((int)StatusCode <= 499); }
        }

        public bool IsServerErrorStatusCode
        {
            get { return ((int)StatusCode >= 500) && ((int)StatusCode <= 599); }
        }

        [UsedImplicitly]
        internal ControllerResult(HttpResponseMessage httpResponseMessage, string resultAsString, HttpStatusCode statusCode)
        {
            HttpResponseMessage = httpResponseMessage;
            ResultAsString = resultAsString;
            StatusCode = statusCode;
            _actionResult = null!;
        }
    }
}
