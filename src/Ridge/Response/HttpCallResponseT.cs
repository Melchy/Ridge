using Ridge.Serialization;
using System;
using System.Net;
using System.Net.Http;

namespace Ridge.Response
{
    /// <inheritdoc />
    public class HttpCallResponse<TResult> : HttpCallResponse
    {
        private readonly IRequestResponseSerializer _serializer;

        /// <summary>
        ///     Deserialize body content to TResult.
        /// </summary>
        public TResult Result => GetResultOrThrow();

        internal HttpCallResponse(
            HttpResponseMessage httpResponseMessage,
            string resultAsString,
            HttpStatusCode statusCode,
            IRequestResponseSerializer serializer)
            : base(httpResponseMessage, resultAsString, statusCode)
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
