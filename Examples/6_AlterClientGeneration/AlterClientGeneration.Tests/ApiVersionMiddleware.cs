using Ridge.HttpRequestFactoryMiddlewares;

namespace AlterClientGeneration.Tests;

public class CountryCodeMiddleware : HttpRequestFactoryMiddleware
{
    public override async Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        var countryCodePassedToClient = requestFactoryContext.ParameterProvider.GetClientParameters().GetParameterByNameOrThrow("countryCode");
        requestFactoryContext.Headers.Add("countryCode", countryCodePassedToClient.GetValueOrThrow<string>());
        return await base.CreateHttpRequest(requestFactoryContext);
    }
}
