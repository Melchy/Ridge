﻿using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.HttpRequestFactoryMiddlewares.Internal.InternalMiddlewares;

internal class AddQueryParametersByFromQueryAttributeMiddleware : HttpRequestFactoryMiddleware
{
    public override Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        var relevantParameters = requestFactoryContext.ParameterProvider
           .GetControllerAndCallerParametersLinked()
           .Where(x =>
                x.DoesParameterExistInController() &&
                x.DoesParameterExistsInCaller())
           .Where(x => !x.WasParameterAddedOrTransformed);

        var fromQueryParams = relevantParameters.Where(x =>
            GeneralHelpers.HasAttribute<FromQueryAttribute>(x.ActionParameter!.ParameterInfo));

        var queryParameters = ParameterAnalyzer.AnalyzeQueryOrRouteParameters(fromQueryParams);

        foreach (var queryParameter in queryParameters)
        {
            requestFactoryContext.UrlGenerationParameters[queryParameter.Key] = queryParameter.Value;
        }

        return base.CreateHttpRequest(requestFactoryContext);
    }
}