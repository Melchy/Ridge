using Ridge.Serialization;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Response;

internal class HttpResponseCallFactory
{
    private readonly IRequestResponseSerializer _serializer;

    public HttpResponseCallFactory(
        SerializerProvider serializerProvider)
    {
        ArgumentNullException.ThrowIfNull(serializerProvider);
        _serializer = serializerProvider.GetSerializer();
    }

    public async Task<HttpCallResponse<TReturn>> CreateControllerCallResult<TReturn>(
        HttpResponseMessage httpResponseMessage)
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
        HttpResponseMessage httpResponseMessage)
    {
        var resultString = await httpResponseMessage.Content.ReadAsStringAsync();
        var httpCallResponse = new HttpCallResponse(
            httpResponseMessage,
            resultString,
            httpResponseMessage.StatusCode);
        return httpCallResponse;
    }
}
