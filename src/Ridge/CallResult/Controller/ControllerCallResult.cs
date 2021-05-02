using Microsoft.AspNetCore.Mvc;
using Ridge.CallResult.Controller.Extensions;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.CallResult.Controller
{
    public class ControllerCallResult : ActionResult, IActionResult
    {
        public HttpResponseMessage HttpResponseMessage { get; }
        public string ResultAsString { get; }
        public HttpStatusCode StatusCode { get; }

        public ControllerCallResult(
            HttpResponseMessage httpResponseMessage,
            string resultAsString,
            HttpStatusCode statusCode)
        {
            HttpResponseMessage = httpResponseMessage;
            ResultAsString = resultAsString;
            StatusCode = statusCode;
        }

        public override void ExecuteResult(ActionContext context)
        {
            throw new InvalidOperationException($"This method should not be used in tests. Instead use extension methods '{nameof(ActionResultTExtensions.Unwrap)}'," +
                                                $" '{nameof(ActionResultTExtensions.HttpResponseMessage)}', '{nameof(ActionResultTExtensions.ResultAsString)}' and others.");
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            throw new InvalidOperationException($"This method should not be used in tests. Instead use extension methods: '{nameof(ActionResultTExtensions.Unwrap)}'," +
                                                $" '{nameof(ActionResultTExtensions.HttpResponseMessage)}', '{nameof(ActionResultTExtensions.ResultAsString)}' and others.");
        }


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
    }
}
