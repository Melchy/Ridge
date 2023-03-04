using Microsoft.AspNetCore.Routing;
using Ridge.Serialization;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Ridge.HttpRequestFactoryMiddlewares.Internal.InternalMiddlewares;

internal class FinalHttpRequestMiddleware : HttpRequestFactoryMiddleware
{
    private readonly LinkGenerator _linkGenerator;
    private readonly IRequestResponseSerializer _requestResponseSerializer;

    public FinalHttpRequestMiddleware(
        LinkGenerator linkGenerator,
        IRequestResponseSerializer requestResponseSerializer)
    {
        _linkGenerator = linkGenerator;
        _requestResponseSerializer = requestResponseSerializer;
    }

    public override Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        if (requestFactoryContext.HttpMethod == null)
        {
            throw new InvalidOperationException($"{nameof(requestFactoryContext.HttpMethod)} can not be null when creating request.");
        }

        if (requestFactoryContext.HttpContentType == null)
        {
            throw new InvalidOperationException($"{nameof(requestFactoryContext.HttpContentType)} can not be null when creating request.");
        }

        if (requestFactoryContext.Headers == null)
        {
            throw new InvalidOperationException($"{nameof(requestFactoryContext.Headers)} can not be null when creating request.");
        }

        var uri = CreateUri(requestFactoryContext.UrlGenerationParameters);
        var httpRequestMessage = Create(requestFactoryContext.HttpMethod,
            uri,
            (ContentData: requestFactoryContext.Body, WasBodySet: requestFactoryContext.RequestHasBody),
            requestFactoryContext.CallId,
            requestFactoryContext.HttpContentType,
            requestFactoryContext.Headers,
            _requestResponseSerializer);

        return Task.FromResult(httpRequestMessage);
    }

    private static HttpRequestMessage Create(
        string httpMethod,
        string url,
        (object? ContentData, bool WasBodySet) body,
        Guid callId,
        string contentType,
        HttpRequestHeaders headers,
        IRequestResponseSerializer serializer)
    {
        var httpMethodObject = new HttpMethod(httpMethod);
        var request = new HttpRequestMessage
        {
            RequestUri = new Uri(url, UriKind.RelativeOrAbsolute),
            Method = httpMethodObject,
        };

        if (body.WasBodySet)
        {
            if (httpMethodObject == HttpMethod.Post ||
                httpMethodObject == HttpMethod.Get ||
                httpMethodObject == HttpMethod.Delete ||
                httpMethodObject == HttpMethod.Put)
            {
                request.Content = CreateContent(contentType, body.ContentData, serializer);
            }
        }

        foreach (var headerParam in headers)
        {
            request.Headers.Add(headerParam.Key, headerParam.Value.Select(x => x));
        }

        request.Headers.Add("ridgeCallId", callId.ToString());
        return request;
    }

    private static ByteArrayContent CreateContent(
        string contentType,
        object? content,
        IRequestResponseSerializer serializer)
    {
        var serializedContent = serializer.Serialize(content) ?? "null";
        return new StringContent(serializedContent, Encoding.UTF8, contentType);
    }

    private string CreateUri(
        object routeParams)
    {
        var uri = _linkGenerator.GetPathByRouteValues("", routeParams);
        if (uri == null)
        {
            throw new InvalidOperationException(
                "Could not generate uri.");
        }

        return uri;
    }
}
