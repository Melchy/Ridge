using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;

namespace Ridge.Results
{
    /// <summary>
    /// This part of ControllerResult is used by asp.net
    /// </summary>
    public partial class ControllerResult<TResult> : ControllerResult, IConvertToActionResult
    {
        public ControllerResult(TResult value)
        {
            ActionResultT = new ActionResult<TResult>(value);
        }

        public ControllerResult(ActionResult result)
        {
            ActionResultT = new ActionResult<TResult>(result);
        }

        private ActionResult<TResult> ActionResultT { get; }

        public static implicit operator ControllerResult<TResult>(TResult value)
        {
            return new ControllerResult<TResult>(value);
        }

        public static implicit operator ControllerResult<TResult>(ActionResult result)
        {
            return new ControllerResult<TResult>(result);
        }

        IActionResult IConvertToActionResult.Convert()
        {
            return ((IConvertToActionResult)ActionResultT).Convert();
        }
    }


    /// <summary>
    /// This part of class represents result of page call
    /// </summary>
    public partial class  ControllerResult<TResult>
    {
        public TResult Result => GetResultOrThrow();

        [UsedImplicitly]
        internal ControllerResult(
            HttpResponseMessage httpResponseMessage,
            string resultAsString,
            HttpStatusCode statusCode) : base(httpResponseMessage, resultAsString, statusCode)
        {
            ActionResultT = null!;
        }

        /// <summary>
        /// This method throws if result can not be deserialized into TResult
        /// or when result is failed
        /// </summary>
        /// <returns></returns>
        private TResult GetResultOrThrow()
        {
            if (!HttpResponseMessage.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Request failed. Status Code: {StatusCode}, HttpContent: '{ResultAsString}'");
            }
            try
            {
                if (string.IsNullOrEmpty(ResultAsString))
                {
                    return default(TResult)!;
                }
                if (typeof(TResult) == typeof(string))
                {
                    return (TResult)(object)ResultAsString;
                }
                return JsonConvert.DeserializeObject<TResult>(ResultAsString);
            }
            catch (JsonException e)
            {
                throw new InvalidOperationException($"Deserialization to type: {typeof(TResult)} failed. Json that was sent from server: '{ResultAsString}'", e);
            }
        }
    }
}
