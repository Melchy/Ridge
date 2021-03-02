using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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
    public partial class ControllerResult : IConvertToActionResult
    {
        [DebuggerHidden]
        public ActionResult ActionResult { get; }

#pragma warning disable CS8618
        public ControllerResult(ActionResult actionResult)
        {
            ActionResult = actionResult ?? throw new ArgumentNullException(nameof(actionResult));;
        }

        protected ControllerResult()
        {
        }

#pragma warning restore CS8618
        public static implicit operator ControllerResult(ActionResult actionResult)
        {
            return new ControllerResult(actionResult);
        }

        public IActionResult Convert()
        {
            return ActionResult;
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
            ActionResult = null!;
        }
    }
}
