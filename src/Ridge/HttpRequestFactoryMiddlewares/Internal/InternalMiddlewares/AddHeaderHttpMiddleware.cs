using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.HttpRequestFactoryMiddlewares.Internal.InternalMiddlewares;

internal class AddHeaderHttpMiddleware : HttpRequestFactoryMiddleware
{
    private readonly string _key;
    private readonly string? _value;

    public AddHeaderHttpMiddleware(
        string key,
        string? value)
    {
        _key = key;
        _value = value;
    }

    public override Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        requestFactoryContext.Headers.Add(_key, _value);
        return base.CreateHttpRequest(requestFactoryContext);
    }
}
