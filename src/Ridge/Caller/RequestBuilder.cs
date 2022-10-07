using Ridge.ActionInfo;
using Ridge.LogWriter;
using Ridge.Pipeline;
using Ridge.Pipeline.Public;
using Ridge.Pipeline.Public.DefaulPipelineParts;
using Ridge.Transformers;
using Ridge.Transformers.DefaultTransformers;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Ridge.Caller;

/// <summary>
///     This builder is used to register <see cref="IHttpRequestPipelinePart"/> and <see cref="IActionInfoTransformer"/>
///     which will be later used to transform request.
/// </summary>
public class RequestBuilder
{
    private readonly List<IHttpRequestPipelinePart> _pipelineParts = new();
    private readonly List<IActionInfoTransformer> _actionInfoTransformers = new();

    /// <summary>
    ///     Create webCaller which calls all the <see cref="IHttpRequestPipelinePart" />
    ///     and then server.
    /// </summary>
    /// <param name="httpClient">Client used to call server. </param>
    /// <param name="logger">Logger to log generated request and response.</param>
    /// <returns>
    ///     <see cref="WebCaller" />
    /// </returns>
    internal WebCaller BuildWebCaller(
        HttpClient httpClient,
        ILogWriter logger)
    {
        return new WebCaller(httpClient, logger, _pipelineParts);
    }


    /// <summary>
    ///     Creates <see cref="ActionInfoTransformersCaller" />
    /// </summary>
    /// <returns>
    ///     <see cref="ActionInfoTransformersCaller" />
    /// </returns>
    public ActionInfoTransformersCaller BuildActionInfoTransformerCaller()
    {
        return new ActionInfoTransformersCaller(_actionInfoTransformers);
    }

    /// <summary>
    ///     Add <see cref="IHttpRequestPipelinePart"/> which will be used to transform <see cref="HttpRequestMessage"/>.
    /// </summary>
    /// <param name="httpRequestPipelineParts"><see cref="IHttpRequestPipelinePart"/> to add.</param>
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
    ///     Adds <see cref="IActionInfoTransformer"/> which will be later used to transform <see cref="IActionInfo"/>.
    /// </summary>
    /// <param name="actionInfoTransformers"><see cref="IActionInfoTransformer"/>  to add.</param>
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
    
    /// <summary>
    ///     Adds headers to the request. This method actually adds <see cref="IActionInfoTransformer"/>
    ///     which then adds the header to request.
    /// </summary>
    /// <param name="headers">Headers to add.</param>
    public void AddHeaders(
        IEnumerable<(string Key, string? Value)>? headers)
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
    ///     Adds <see cref="AuthenticationHeaderValue"/> to the request. This method actually adds <see cref="IActionInfoTransformer"/>
    ///     which then adds the header to request.
    /// </summary>
    /// <param name="authenticationHeaderValue"><see cref="AuthenticationHeaderValue"/> to add.</param>
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

    /// <summary>
    ///     Creates new RequestBuilder and copies all the existing <see cref="IHttpRequestPipelinePart" />
    ///     and <see cref="IActionInfoTransformer" />.
    /// </summary>
    /// <returns></returns>
    public RequestBuilder CreateNewBuilderByCopyingExisting()
    {
        var newRequestBuilder = new RequestBuilder();
        newRequestBuilder.AddHttpRequestPipelineParts(_pipelineParts);
        newRequestBuilder.AddActionInfoTransformers(_actionInfoTransformers);
        return newRequestBuilder;
    }

    private void AddHeader(
        string key,
        string? value)
    {
        var addHeaderTransformer = new AddHeaderActionInfoTransformer(key, value);
        _actionInfoTransformers.Add(addHeaderTransformer);
    }
}
