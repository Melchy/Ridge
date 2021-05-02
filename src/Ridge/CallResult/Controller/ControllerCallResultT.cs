using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;

namespace Ridge.CallResult.Controller
{
    public class ControllerCallResult<TResult> : ControllerCallResult
    {
        public TResult Result => GetResultOrThrow();

        internal ControllerCallResult(
            HttpResponseMessage httpResponseMessage,
            string resultAsString,
            HttpStatusCode statusCode) : base(httpResponseMessage, resultAsString, statusCode)
        {
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
