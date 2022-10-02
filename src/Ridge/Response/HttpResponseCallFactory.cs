using Ridge.Serialization;
using System;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Ridge.Response;

internal class HttpResponseCallFactory : IHttpResponseCallFactory
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
        CheckIfExceptionOccuredAndThrowIfItDid(callId);

        var resultString = await httpResponseMessage.Content.ReadAsStringAsync();
        var ridgeResult = new HttpCallResponse<TReturn>(
            httpResponseMessage,
            resultString,
            httpResponseMessage.StatusCode,
            _serializer);
        return ridgeResult;
    }

    public async Task<HttpCallResponse> CreateControllerCallResult(
        HttpResponseMessage httpResponseMessage,
        string callId)
    {
        CheckIfExceptionOccuredAndThrowIfItDid(callId);
        var resultString = await httpResponseMessage.Content.ReadAsStringAsync();
        var ridgeResult = new HttpCallResponse(
            httpResponseMessage,
            resultString,
            httpResponseMessage.StatusCode);
        return ridgeResult;
    }

    private static void CheckIfExceptionOccuredAndThrowIfItDid(
        string callId)
    {
        Exception? exceptionWhichOccuredInApplication = ExceptionManager.ExceptionManager.GetData(callId);
        if (exceptionWhichOccuredInApplication != null)
        {
            ExceptionDispatchInfo.Capture(exceptionWhichOccuredInApplication).Throw();
            throw new InvalidOperationException("This is never thrown"); // this line is never reached
        }
    }
}
