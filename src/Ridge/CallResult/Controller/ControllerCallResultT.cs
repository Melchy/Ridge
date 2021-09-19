using Ridge.Serialization;
using System;
using System.Net;
using System.Net.Http;

namespace Ridge.CallResult.Controller
{
    public class ControllerCallResult<TResult> : ControllerCallResult
    {
        private readonly IRidgeSerializer _serializer;
        public TResult Result => GetResultOrThrow();

        internal ControllerCallResult(
            HttpResponseMessage httpResponseMessage,
            string resultAsString,
            HttpStatusCode statusCode,
            IRidgeSerializer serializer) : base(httpResponseMessage, resultAsString, statusCode)
        {
            _serializer = serializer;
        }

        /// <summary>
        ///     This method throws if result can not be deserialized into TResult
        ///     or when result is failed
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
                    return default!;
                }

                if (typeof(TResult) == typeof(string))
                {
                    return (TResult)(object)ResultAsString;
                }

                return _serializer.Deserialize<TResult>(ResultAsString);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Deserialization to type: " +
                                                    $"{typeof(TResult)} failed using serializer: {_serializer.GetSerializerName()}." +
                                                    $" Json that was sent from server: '{ResultAsString}'",
                    e);
            }
        }
    }
}
