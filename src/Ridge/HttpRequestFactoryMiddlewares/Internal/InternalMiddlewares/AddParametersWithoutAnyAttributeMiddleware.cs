using Microsoft.AspNetCore.Mvc;
using Ridge.Parameters;
using Ridge.Parameters.ActionAndClientParams;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.HttpRequestFactoryMiddlewares.Internal.InternalMiddlewares;

internal class AddParametersWithoutAnyAttributeMiddleware : HttpRequestFactoryMiddleware
{
    public override Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        var potentialBodyArgumentWithoutFromAttribute = GetBodyFromParametersWithoutAttribute(requestFactoryContext);
        var fromQueryAndFromRouteParameters = GetFromQueryAndFromRouteWithoutAttribute(requestFactoryContext.ParameterProvider);

        if (potentialBodyArgumentWithoutFromAttribute.WasBodyFound)
        {
            requestFactoryContext.Body = potentialBodyArgumentWithoutFromAttribute.Body;
        }

        foreach (var queryParameter in fromQueryAndFromRouteParameters)
        {
            requestFactoryContext.UrlGenerationParameters[queryParameter.Key] = queryParameter.Value;
        }

        return base.CreateHttpRequest(requestFactoryContext);
    }

    private static (object? Body, bool WasBodyFound) GetBodyFromParametersWithoutAttribute(
        IRequestFactoryContext requestFactoryContext)
    {
        var relevantParameters = requestFactoryContext.ParameterProvider
           .GetActionAndClientParametersLinked()
           .Where(x => x.DoesParameterExistsInClient() && x.DoesParameterExistInAction())
           .Where(x => !x.WasParameterAddedOrTransformed);

        var potentialBodyArgumentWithoutFromAttribute = GetParametersWithoutMvcAttributes(relevantParameters)
           .Where(x =>
                !GeneralHelpers.IsSimpleType(x.ActionParameter!.Type))
           .Select(x => x.ClientParameter!.Value);

        if (potentialBodyArgumentWithoutFromAttribute.Any())
        {
            return (potentialBodyArgumentWithoutFromAttribute.FirstOrDefault(), true);
        }

        return (null, false);
    }

    private static IDictionary<string, object?> GetFromQueryAndFromRouteWithoutAttribute(
        ParameterProvider parameterProvider)
    {
        var relevantParameters = parameterProvider
           .GetActionAndClientParametersLinked()
           .Where(x =>
                x.DoesParameterExistInAction() &&
                x.DoesParameterExistsInClient())
           .Where(x => !x.WasParameterAddedOrTransformed);

        // With these arguments we can not decide if they should be bound fromQuery or fromRoute
        // we use algorithm for fromQuery params because it can bind even lists and complex objects.
        // If user adds complex object which should be bounded FromRoute the parameter is created but asp.net core will not bind it.
        var argumentsWhichMayBeFromQueryOrFromRouteAttribute = GetParametersWithoutMvcAttributes(relevantParameters);

        return ParameterAnalyzer.AnalyzeQueryOrRouteParameters(argumentsWhichMayBeFromQueryOrFromRouteAttribute);
    }

    private static IEnumerable<ActionAndClientParameterLinked> GetParametersWithoutMvcAttributes(
        IEnumerable<ActionAndClientParameterLinked> controllerAndClientParametersLinked)
    {
        return controllerAndClientParametersLinked.Where(x =>
        {
            if (x.ActionParameter == null)
            {
                return false;
            }

            return !GeneralHelpers.HasAttribute<FromRouteAttribute>(x.ActionParameter.ParameterInfo) &&
                   !GeneralHelpers.HasAttribute<FromQueryAttribute>(x.ActionParameter.ParameterInfo) &&
                   !GeneralHelpers.HasAttribute<FromBodyAttribute>(x.ActionParameter.ParameterInfo) &&
                   !GeneralHelpers.HasAttribute<FromHeaderAttribute>(x.ActionParameter.ParameterInfo) &&
                   !GeneralHelpers.HasAttribute<FromServicesAttribute>(x.ActionParameter.ParameterInfo) &&
                   !GeneralHelpers.HasAttribute<ModelBinderAttribute>(x.ActionParameter.ParameterInfo);
        });
    }
}
