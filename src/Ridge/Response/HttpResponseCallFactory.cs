using Ridge.Serialization;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Response;

internal class HttpResponseCallFactory
{
    private readonly IRequestResponseSerializer _serializer;

    public HttpResponseCallFactory(
        IRequestResponseSerializer serializer)
    {
        _serializer = serializer;
    }

    public async Task<HttpCallResponse<TReturn>> CreateControllerCallResult<TReturn>(
        HttpResponseMessage httpResponseMessage,
        string callId)
    {
        var resultString = await httpResponseMessage.Content.ReadAsStringAsync();
        var httpCallResponse = new HttpCallResponse<TReturn>(
            httpResponseMessage,
            resultString,
            httpResponseMessage.StatusCode,
            _serializer);
        return httpCallResponse;
    }

    public async Task<HttpCallResponse> CreateControllerCallResult(
        HttpResponseMessage httpResponseMessage,
        string callId)
    {
        var resultString = await httpResponseMessage.Content.ReadAsStringAsync();
        var httpCallResponse = new HttpCallResponse(
            httpResponseMessage,
            resultString,
            httpResponseMessage.StatusCode);
        return httpCallResponse;
    }
}
