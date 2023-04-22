using Microsoft.AspNetCore.Mvc;
using Ridge.Parameters;
using Ridge.Parameters.ClientParams;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.HttpRequestFactoryMiddlewares.Internal.InternalMiddlewares;

internal class AddRouteParametersByFromRouteAttributeMiddleware : HttpRequestFactoryMiddleware
{
    public override Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        var fromRouteParameters = GetFromRouteParams(requestFactoryContext.ParameterProvider);
        foreach (var fromRouteParameter in fromRouteParameters)
        {
            requestFactoryContext.UrlGenerationParameters[fromRouteParameter.Key] = fromRouteParameter.Value;
        }

        return base.CreateHttpRequest(requestFactoryContext);
    }

    private static IDictionary<string, object?> GetFromRouteParams(
        ParameterProvider parameterProvider)
    {
        var relevantParameters = parameterProvider
           .GetActionAndClientParametersLinked()
           .Where(x =>
                x.DoesParameterExistInAction() &&
                x.DoesParameterExistsInClient())
           .Where(x => !x.WasParameterAddedOrTransformed);

        var fromRouteParams = relevantParameters.Where(x =>
            GeneralHelpers.HasAttribute<FromRouteAttribute>(x.ActionParameter!.ParameterInfo));

        IDictionary<string, object?> routeDataDictionary = new Dictionary<string, object?>();
        foreach (var fromRouteParam in fromRouteParams)
        {
            ClientParameter clientParameter = fromRouteParam.ClientParameter!;
            var parameterNameInRequest = ParameterAnalyzer.GetAttributeNamePropertyOrParameterName(fromRouteParam);
            if (clientParameter.Value == null)
            {
                continue;
            }

            routeDataDictionary[parameterNameInRequest] = clientParameter.Value;
        }

        return routeDataDictionary;
    }
}
