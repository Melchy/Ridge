using Ridge.GeneratorAttributes;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.HttpRequestFactoryMiddlewares.Internal.InternalMiddlewares;

internal class AddTransformedOrAddedParametersMiddleware : HttpRequestFactoryMiddleware
{
    public override Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        AddTransformedAndAddedParameters(requestFactoryContext);

        return base.CreateHttpRequest(requestFactoryContext);
    }

    private static void AddTransformedAndAddedParameters(
        IRequestFactoryContext requestFactoryContext)
    {
        var parameters = requestFactoryContext.ParameterProvider
           .GetActionAndCallerParametersLinked()
           .Where(x => x.DoesParameterExistsInCaller())
           .Where(x => x.WasParameterAddedOrTransformed);

        foreach (var parameter in parameters)
        {
            var mapping = parameter.CallerParameter!.AddedOrTransformedParameterMapping;
            if (mapping == null)
            {
                continue;
            }

            if (mapping == ParameterMapping.None)
            {
                continue;
            }

            if (mapping == ParameterMapping.MapToBody)
            {
                requestFactoryContext.Body = parameter.CallerParameter.Value;
            }
            else if (mapping == ParameterMapping.MapToHeader)
            {
                var headers = ParameterAnalyzer.AnalyzeHeader(parameter);
                foreach (var header in headers)
                {
                    requestFactoryContext.Headers.Add(header.Key, header.Value);
                }
            }
            else if (mapping == ParameterMapping.MapToQueryOrRouteParameter)
            {
                var queryOrRouteParameters = ParameterAnalyzer.AnalyzeQueryOrRouteParameter(parameter);
                foreach (var queryOrRouteParameter in queryOrRouteParameters)
                {
                    requestFactoryContext.UrlGenerationParameters.Add(queryOrRouteParameter.Key, queryOrRouteParameter.Value);
                }
            }
        }
    }
}
