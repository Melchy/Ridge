using Ridge.LogWriter;
using Ridge.Pipeline;
using Ridge.Pipeline.Public;
using Ridge.Pipeline.Public.DefaulPipelineParts;
using Ridge.Transformers;
using Ridge.Transformers.DefaultTransformers;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

// TODO zmeni slozky
namespace Ridge.Interceptor;

/// <summary>
///     TODO
/// </summary>
public class RequestBuilder
{
    private readonly List<IHttpRequestPipelinePart> _pipelineParts = new();
    private readonly List<IActionInfoTransformer> _actionInfoTransformers = new();

    internal WebCaller BuildWebCaller(
        HttpClient httpClient,
        ILogWriter logger)
    {
        return new WebCaller(httpClient, logger, _pipelineParts);
    }

    internal ActionInfoTransformersCaller BuildActionInfoTransformerCaller()
    {
        return new ActionInfoTransformersCaller(_actionInfoTransformers);
    }

    /// <summary>
    ///     TODO
    /// </summary>
    /// <param name="httpRequestPipelineParts"></param>
    public void AddHttpRequestPipelineParts(
        IEnumerable<IHttpRequestPipelinePart>? httpRequestPipelineParts)
    {
        if (httpRequestPipelineParts == null)
        {
            return;
        }

        foreach (var httpRequestPipelinePart in httpRequestPipelineParts)
        {
            _pipelineParts.Add(httpRequestPipelinePart);
        }
    }

    /// <summary>
    ///     TODO
    /// </summary>
    /// <param name="actionInfoTransformers"></param>
    public void AddActionInfoTransformers(
        IEnumerable<IActionInfoTransformer>? actionInfoTransformers)
    {
        if (actionInfoTransformers == null)
        {
            return;
        }

        foreach (var actionInfoTransformer in actionInfoTransformers)
        {
            _actionInfoTransformers.Add(actionInfoTransformer);
        }
    }

    private void AddHeader(
        string key,
        string? value)
    {
        var addHeaderTransformer = new AddHeaderActionInfoTransformer(key, value);
        _actionInfoTransformers.Add(addHeaderTransformer);
    }

    /// <summary>
    ///     TODO
    /// </summary>
    /// <param name="headers"></param>
    public void AddHeaders(
        IEnumerable<KeyValuePair<string, string?>>? headers)
    {
        if (headers == null)
        {
            return;
        }

        foreach (var header in headers)
        {
            AddHeader(header.Key, header.Value);
        }
    }

    /// <summary>
    ///     TODO
    /// </summary>
    /// <param name="authenticationHeaderValue"></param>
    public void AddAuthenticationHeaderValue(
        AuthenticationHeaderValue? authenticationHeaderValue)
    {
        if (authenticationHeaderValue == null)
        {
            return;
        }

        var addAuthenticationPipelinePart = new AddAuthenticationPipelinePart(authenticationHeaderValue);
        _pipelineParts.Add(addAuthenticationPipelinePart);
    }
}
