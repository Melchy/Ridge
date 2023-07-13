using Ridge.HttpRequestFactoryMiddlewares;

namespace AlterRequestGeneration.Tests;

public class TemperatureMiddleware : HttpRequestFactoryMiddleware
{
    public override async Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        var temperature = requestFactoryContext.ParameterProvider.GetAdditionalParameters().GetParameterByNameOrThrow("temperatureC").GetValueOrThrow<int>();
        requestFactoryContext.Headers.Add("temperatureC", temperature.ToString());
        return await base.CreateHttpRequest(requestFactoryContext);
    }
}
