using Microsoft.AspNetCore.Mvc;
using Ridge.Parameters;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Ridge.HttpRequestFactoryMiddlewares.Internal.InternalMiddlewares;

internal class AddHeadersByFromHeaderAttributeMiddleware : HttpRequestFactoryMiddleware
{
    public override Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        var fromRouteParameters = GetHeadersFromMethodArguments(requestFactoryContext.ParameterProvider);
        foreach (var fromRouteParameter in fromRouteParameters)
        {
            requestFactoryContext.Headers.Add(fromRouteParameter.Key, fromRouteParameter.Value);
        }

        return base.CreateHttpRequest(requestFactoryContext);
    }

    private static HttpRequestHeaders GetHeadersFromMethodArguments(
        ParameterProvider parameterProvider)
    {
        var relevantParameters = parameterProvider
           .GetActionAndClientParametersLinked()
           .Where(x =>
                x.DoesParameterExistInAction() &&
                x.DoesParameterExistsInClient())
           .Where(x => !x.WasParameterAddedOrTransformed);

        var fromHeadParams = relevantParameters.Where(x =>
            GeneralHelpers.HasAttribute<FromHeaderAttribute>(x.ActionParameter!.ParameterInfo));
        // HttpRequestMessage has internal constructor therefore we need to create it using HttpRequestMessage 
        using var message = new HttpRequestMessage();
        var headers = message.Headers;
        foreach (var fromHeadParam in fromHeadParams)
        {
            var headersToAdd = ParameterAnalyzer.AnalyzeHeader(fromHeadParam);
            foreach (var headerToAdd in headersToAdd)
            {
                headers.Add(headerToAdd.Key, headerToAdd.Value);
            }
        }

        return headers;
    }
}
