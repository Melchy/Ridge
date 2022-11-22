using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.HttpRequestFactoryMiddlewares.InternalMiddlewares;

internal class AddHeaderHttpRequestFactoryMiddleware : HttpRequestFactoryMiddleware
{
    private readonly string _key;
    private readonly string? _value;

    public AddHeaderHttpRequestFactoryMiddleware(
        string key,
        string? value)
    {
        _key = key;
        _value = value;
    }

    public override Task<HttpRequestMessage> CreateHttpRequest(
        RequestFactoryContext requestFactoryContext)
    {
        requestFactoryContext.Headers!.Add(_key, _value);
        return base.CreateHttpRequest(requestFactoryContext);
    }
}
