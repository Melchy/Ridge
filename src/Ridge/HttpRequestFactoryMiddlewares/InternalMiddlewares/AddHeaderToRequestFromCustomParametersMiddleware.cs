using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.HttpRequestFactoryMiddlewares.InternalMiddlewares;

internal class AddHeaderToRequestFromCustomParametersMiddleware : HttpRequestFactoryMiddleware
{
    public override Task<HttpRequestMessage> CreateHttpRequest(
        RequestFactoryContext requestFactoryContext)
    {
        var httpHeaders = requestFactoryContext.CustomParametersProvider.GetCustomParametersByType<HttpHeader>();
        foreach (var httpHeader in httpHeaders)
        {
            if (httpHeader == null)
            {
                continue;
            }

            requestFactoryContext.Headers!.Add(httpHeader.Key, httpHeader.Value);
        }

        return base.CreateHttpRequest(requestFactoryContext);
    }
}
