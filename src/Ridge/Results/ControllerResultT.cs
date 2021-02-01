using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;

namespace Ridge.Results
{
    /// <summary>
    /// This part of ControllerResult is used by asp.net
    /// </summary>
    public partial class ControllerResult<TResult> : ControllerResult
    {
        private ActionResult _actionResult { get; }

        public ControllerResult(
            TResult actionResult) : this(new OkObjectResult(actionResult))
        {

        }

#pragma warning disable CS8618
        private ControllerResult(ActionResult actionResult) : base(actionResult)
        {
            _actionResult = actionResult;
        }
#pragma warning restore CS8618

        public static implicit operator ControllerResult<TResult>(ActionResult actionResult)
        {
            return new ControllerResult<TResult>(actionResult);
        }

        public static implicit operator ControllerResult<TResult>(TResult actionResult)
        {
            return new ControllerResult<TResult>(new OkObjectResult(actionResult));
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
            _actionResult = null!;
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
                return JsonConvert.DeserializeObject<TResult>(ResultAsString);
            }
            catch (JsonException e)
            {
                throw new InvalidOperationException($"Deserialization to type: {typeof(TResult)} failed. Json that was sent from server: '{ResultAsString}'", e);
            }
        }
    }
}
