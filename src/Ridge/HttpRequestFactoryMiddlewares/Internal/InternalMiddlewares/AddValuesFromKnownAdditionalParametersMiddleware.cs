using Ridge.Parameters.AdditionalParams;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.HttpRequestFactoryMiddlewares.Internal.InternalMiddlewares;

internal class AddValuesFromKnownAdditionalParametersMiddleware : HttpRequestFactoryMiddleware
{
    public override Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        var additionalParameters = requestFactoryContext.ParameterProvider.GetAdditionalParameters();
        ProcessHeaderParameter(requestFactoryContext, additionalParameters);
        ProcessBodyParameter(requestFactoryContext, additionalParameters);
        ProcessQueryOrRouteParameters(requestFactoryContext, additionalParameters);
        return base.CreateHttpRequest(requestFactoryContext);
    }

    private static void ProcessHeaderParameter(
        IRequestFactoryContext requestFactoryContext,
        AdditionalParameters additionalParameters)
    {
        var httpHeaders = additionalParameters
           .GetParametersByType<HttpHeaderParameter>();

        foreach (var httpHeader in httpHeaders)
        {
            requestFactoryContext.Headers.Add(httpHeader.Key, httpHeader.Value);
        }
    }

    private static void ProcessBodyParameter(
        IRequestFactoryContext requestFactoryContext,
        AdditionalParameters additionalParameters)
    {
        var bodyParameters = additionalParameters
           .GetParametersByType<BodyParameter>();

        if (bodyParameters.Any())
        {
            requestFactoryContext.Body = bodyParameters.First().Value;
        }
    }

    private static void ProcessQueryOrRouteParameters(
        IRequestFactoryContext requestFactoryContext,
        AdditionalParameters additionalParameters)
    {
        var queryOrRouteParameters = additionalParameters
           .GetParametersByType<QueryOrRouteParameter>();

        foreach (var queryOrRouteParameter in queryOrRouteParameters)
        {
            requestFactoryContext.UrlGenerationParameters[queryOrRouteParameter.Name] = queryOrRouteParameter.Value;
        }
    }
}
