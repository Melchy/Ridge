using Ridge.Serialization;
using System;
using System.Net;
using System.Net.Http;

namespace Ridge.Response;

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
        string contentAsString,
        HttpStatusCode statusCode,
        IRequestResponseSerializer serializer)
        : base(httpResponseMessage, contentAsString, statusCode)
    {
        _serializer = serializer;
    }

    /// <summary>
    ///     This method throws if result can not be deserialized into TResult.
    ///     When the response is not 2xx exception is thrown.
    /// </summary>
    /// <returns></returns>
    private TResult GetResultOrThrow()
    {
        if (!HttpResponseMessage.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Request failed. Status Code: '{StatusCode}', Response: '{ContentAsString}'");
        }

        try
        {
            if (string.IsNullOrEmpty(ContentAsString))
            {
                return default!;
            }

            if (typeof(TResult) == typeof(string))
            {
                return (TResult)(object)ContentAsString;
            }

            return _serializer.Deserialize<TResult>(ContentAsString);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Deserialization to type: " +
                                                $"'{typeof(TResult)}' failed. Serializer used : '{_serializer.GetType().FullName}'." +
                                                $" Json sent from server: '{ContentAsString}'",
                e);
        }
    }
}
