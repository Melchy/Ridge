﻿using Microsoft.AspNetCore.Routing;
using Ridge.DelegationHandlers;
using Ridge.Serialization;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Ridge.HttpRequestFactoryMiddlewares.InternalMiddlewares;

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
        RequestFactoryContext requestFactoryContext)
    {
        if (requestFactoryContext.HttpMethod == null)
        {
            throw new InvalidOperationException($"{nameof(requestFactoryContext.HttpMethod)} can not be null when creating request.");
        }

        if (requestFactoryContext.ContentType == null)
        {
            throw new InvalidOperationException($"{nameof(requestFactoryContext.ContentType)} can not be null when creating request.");
        }

        if (requestFactoryContext.Headers == null)
        {
            throw new InvalidOperationException($"{nameof(requestFactoryContext.Headers)} can not be null when creating request.");
        }

        var uri = CreateUri(requestFactoryContext.RouteParams);
        var httpRequestMessage = Create(requestFactoryContext.HttpMethod,
            uri,
            requestFactoryContext.Body,
            requestFactoryContext.CallId,
            requestFactoryContext.ContentType,
            requestFactoryContext.Headers,
            _requestResponseSerializer);

        httpRequestMessage.Options.Set(
            new HttpRequestOptionsKey<RequestDescription>(RequestDescription.OptionsKey),
            new RequestDescription(requestFactoryContext.Body,
                new ReadOnlyDictionary<string, object?>(requestFactoryContext.RouteParams),
                requestFactoryContext.ContentType,
                requestFactoryContext.HttpMethod,
                requestFactoryContext.Headers,
                requestFactoryContext.MethodInfo,
                requestFactoryContext.Arguments,
                requestFactoryContext.CustomParametersProvider));

        return Task.FromResult(httpRequestMessage);
    }

    private static HttpRequestMessage Create(
        string httpMethod,
        string url,
        object? contentData,
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
        if (httpMethodObject == HttpMethod.Post ||
            httpMethodObject == HttpMethod.Get ||
            httpMethodObject == HttpMethod.Delete ||
            httpMethodObject == HttpMethod.Put)
        {
            request.Content = CreateContent(contentType, contentData, serializer);
        }

        foreach (var headerParam in headers)
        {
            request.Headers.Add(headerParam.Key, headerParam.Value.Select(x => x));
        }

        request.Headers.Add("ridgeCallId", callId.ToString());
        return request;
    }

    private static ByteArrayContent? CreateContent(
        string contentType,
        object? content,
        IRequestResponseSerializer serializer)
    {
        if (contentType == "application/json")
        {
            var serializedContent = serializer.Serialize(content);
            return new StringContent(serializedContent!, Encoding.UTF8, contentType);
        }

        if (contentType == "application/x-www-form-urlencoded")
        {
            var contentAsDictionary = GeneralHelpers.ToKeyValue(content);
            return new FormUrlEncodedContent(contentAsDictionary!);
        }

        throw new InvalidOperationException($"Unsupported content type {contentType}");
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