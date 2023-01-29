using Ridge.Parameters.CustomParams;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.HttpRequestFactoryMiddlewares.Internal.InternalMiddlewares;

internal class AddValuesFromKnownCustomParametersMiddleware : HttpRequestFactoryMiddleware
{
    public override Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        var customParameters = requestFactoryContext.ParameterProvider.GetCustomParameters();
        ProcessHeaderParameter(requestFactoryContext, customParameters);
        ProcessBodyParameter(requestFactoryContext, customParameters);
        ProcessQueryOrRouteParameters(requestFactoryContext, customParameters);
        return base.CreateHttpRequest(requestFactoryContext);
    }

    private static void ProcessHeaderParameter(
        IRequestFactoryContext requestFactoryContext,
        CustomParameters customParameters)
    {
        var httpHeaders = customParameters
           .GetParametersByType<HttpHeaderParameter>();

        foreach (var httpHeader in httpHeaders)
        {
            requestFactoryContext.Headers.Add(httpHeader.Key, httpHeader.Value);
        }
    }

    private static void ProcessBodyParameter(
        IRequestFactoryContext requestFactoryContext,
        CustomParameters customParameters)
    {
        var bodyParameters = customParameters
           .GetParametersByType<BodyParameter>();

        if (bodyParameters.Any())
        {
            requestFactoryContext.Body = bodyParameters.First().Value;
        }
    }

    private static void ProcessQueryOrRouteParameters(
        IRequestFactoryContext requestFactoryContext,
        CustomParameters customParameters)
    {
        var queryOrRouteParameters = customParameters
           .GetParametersByType<QueryOrRouteParameter>();

        foreach (var queryOrRouteParameter in queryOrRouteParameters)
        {
            requestFactoryContext.UrlGenerationParameters[queryOrRouteParameter.Name] = queryOrRouteParameter.Value;
        }
    }
}
