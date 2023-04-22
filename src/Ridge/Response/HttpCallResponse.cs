using System.Net;
using System.Net.Http;

namespace Ridge.Response;

/// <summary>
///     This class represents http call response.
/// </summary>
public class HttpCallResponse
{
    /// <summary>
    ///     Response returned from server.
    /// </summary>
    public HttpResponseMessage HttpResponseMessage { get; }

    /// <summary>
    ///     Response content as a string.
    /// </summary>
    public string ContentAsString { get; }

    /// <summary>
    ///     Status code returned from server.
    /// </summary>
    public HttpStatusCode StatusCode { get; }


    /// <summary>
    ///     Indicates if status code has number between 200 and 300
    /// </summary>
    public bool IsSuccessStatusCode => (int)StatusCode >= 200 && (int)StatusCode <= 299;

    /// <summary>
    ///     Indicates if status code has number between 300 and 400
    /// </summary>
    public bool IsRedirectStatusCode => (int)StatusCode >= 300 && (int)StatusCode <= 399;

    /// <summary>
    ///     Indicates if status code has number between 400 and 500
    /// </summary>
    public bool IsClientErrorStatusCode => (int)StatusCode >= 400 && (int)StatusCode <= 499;

    /// <summary>
    ///     Indicates if status code has number between 500 and 600
    /// </summary>
    public bool IsServerErrorStatusCode => (int)StatusCode >= 500 && (int)StatusCode <= 599;

    internal HttpCallResponse(
        HttpResponseMessage httpResponseMessage,
        string contentAsString,
        HttpStatusCode statusCode)
    {
        HttpResponseMessage = httpResponseMessage;
        ContentAsString = contentAsString;
        StatusCode = statusCode;
    }
}
