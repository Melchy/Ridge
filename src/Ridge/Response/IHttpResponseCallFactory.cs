using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Response;

internal interface IHttpResponseCallFactory
{
    Task<HttpCallResponse<TReturn>> CreateControllerCallResult<TReturn>(
        HttpResponseMessage httpResponseMessage,
        string callId);

    Task<HttpCallResponse> CreateControllerCallResult(
        HttpResponseMessage httpResponseMessage,
        string callId);
}
