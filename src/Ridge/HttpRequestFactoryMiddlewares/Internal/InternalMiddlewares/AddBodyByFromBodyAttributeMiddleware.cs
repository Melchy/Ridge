using Microsoft.AspNetCore.Mvc;
using Ridge.Parameters;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.HttpRequestFactoryMiddlewares.Internal.InternalMiddlewares;

internal class AddBodyByFromBodyAttributeMiddleware : HttpRequestFactoryMiddleware
{
    public override Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        var result = GetBody(requestFactoryContext.ParameterProvider);
        if (result.WasBodyFound)
        {
            requestFactoryContext.Body = result.Body;
        }

        return base.CreateHttpRequest(requestFactoryContext);
    }

    private static (object? Body, bool WasBodyFound) GetBody(
        ParameterProvider parameterProvider)
    {
        var relevantParameters = parameterProvider
           .GetActionAndCallerParametersLinked()
           .Where(x => x.DoesParameterExistsInCaller() && x.DoesParameterExistInAction())
           .Where(x => !x.WasParameterAddedOrTransformed);

        var parametersWithFromBody = relevantParameters.Where(
                x => GeneralHelpers.HasAttribute<FromBodyAttribute>(x.ActionParameter!.ParameterInfo))
           .ToList();

        if (parametersWithFromBody.Count > 1)
        {
            throw new InvalidOperationException(
                $"Action can not contain more than one {nameof(FromBodyAttribute)}");
        }

        if (parametersWithFromBody.Any())
        {
            return (parametersWithFromBody.First().CallerParameter!.Value, true);
        }

        return (null, false);
    }
}
