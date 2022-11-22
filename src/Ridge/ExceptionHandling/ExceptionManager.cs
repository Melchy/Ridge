using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace Ridge.ExceptionHandling;

internal class ExceptionManager
{
    private ConcurrentDictionary<string, Exception?> Data { get; } = new();

    public void InsertException(
        HttpContext httpContext,
        Exception exception)
    {
        var callIdHeadersExist = httpContext.Request.Headers.TryGetValue("ridgeCallId", out var callIdHeaders);
        var callIdHeader = callIdHeaders.FirstOrDefault();
        if (callIdHeader == null || !callIdHeadersExist)
        {
            throw new InvalidOperationException("RidgeCallId not found in header. This header is used to save exceptions and was deleted.");
        }

        Data[callIdHeader] = exception;
    }

    private Exception? GetExceptionOrDefault(
        string callId)
    {
        var callDataFound = Data.TryGetValue(callId, out var data);
        if (!callDataFound)
        {
            return null;
        }

        return data;
    }

    public void CheckIfExceptionOccuredAndThrowIfItDid(
        string callId)
    {
        Exception? exceptionWhichOccuredInApplication = GetExceptionOrDefault(callId);
        if (exceptionWhichOccuredInApplication == null)
        {
            return;
        }

        ExceptionDispatchInfo.Capture(exceptionWhichOccuredInApplication).Throw();
        throw new InvalidOperationException("This is never thrown"); // this line is never reached
    }
}