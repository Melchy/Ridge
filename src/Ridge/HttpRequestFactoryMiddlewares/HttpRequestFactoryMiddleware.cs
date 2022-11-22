using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.HttpRequestFactoryMiddlewares;

/// <summary>
///     Defines middleware that can be added to the pipeline creating httpRequest.
/// </summary>
public abstract class HttpRequestFactoryMiddleware
{
    internal HttpRequestFactoryMiddleware? RequestFactoryMiddlewareInner { get; set; } = null;

    /// <summary>
    ///     Method creating http request message.
    /// </summary>
    /// <param name="requestFactoryContext">
    ///     <see cref="RequestFactoryContext" /> which contains data used to create
    ///     <see cref="HttpRequestMessage" />.
    /// </param>
    /// <returns><see cref="HttpRequestMessage" /> which will be send to the server.</returns>
    public virtual async Task<HttpRequestMessage> CreateHttpRequest(
        RequestFactoryContext requestFactoryContext)
    {
        if (RequestFactoryMiddlewareInner == null)
        {
            throw new InvalidOperationException("Inner middleware can not be null.");
        }

        return await RequestFactoryMiddlewareInner.CreateHttpRequest(requestFactoryContext);
    }
}
