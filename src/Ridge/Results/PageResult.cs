using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Results
{
    /// <summary>
    /// This part of class represents result of page call
    /// </summary>
    public partial class PageResult<TPage> : IActionResult
    {
        public HttpResponseMessage HttpResponseMessage { get; }
        public TPage Model { get; }
        public string Response { get; }
        public HttpStatusCode StatusCode { get; }

        internal PageResult(HttpResponseMessage httpResponseMessage, TPage model, string response, HttpStatusCode statusCode)
        {
            HttpResponseMessage = httpResponseMessage;
            Model = model;
            Response = response;
            StatusCode = statusCode;
            _actionContext = null!;
            _actionResult = null!;
        }
    }

    /// <summary>
    /// This part of actionResult is used by asp.net
    /// </summary>
    public partial class PageResult<TPage> : IResultWrapper
    {
        private ActionContext? _actionContext { get; set; }
        private ActionResult _actionResult { get; }

#pragma warning disable CS8618
        private PageResult(ActionResult actionResult)
        {
            _actionResult = actionResult;
        }
#pragma warning restore CS8618
        [DebuggerHidden]
        Task IActionResult.ExecuteResultAsync(ActionContext context)
        {
            throw new InvalidOperationException("This method should never be called.");
        }

        public static implicit operator PageResult<TPage>(ActionResult actionResult)
        {
            return new PageResult<TPage>(actionResult);
        }

        public ActionResult GetInnerActionResult()
        {
            return _actionResult;
        }
    }
}
