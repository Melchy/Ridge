using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Ridge.ExceptionManager
{
    internal static class ExceptionManager
    {
        private static ConcurrentDictionary<string, Exception?> Data { get; } = new();

        public static void InsertEmptyDataToIndicateTestCall(
            Guid requestId)
        {
            Data[requestId.ToString()] = null;
        }

        public static bool IsTestCall(
            HttpContext httpContext)
        {
            var headerPresent = httpContext.Request.Headers.TryGetValue("ridgeCallId", out var value);
            if (!headerPresent)
            {
                return false;
            }

            var callIdPresent = Data.TryGetValue(value, out _);
            return callIdPresent;
        }

        public static void InsertException(
            HttpContext httpContext,
            Exception exception)
        {
            var callIdHeadersExist = httpContext.Request.Headers.TryGetValue("ridgeCallId", out var callIdHeaders);
            var callIdHeader = callIdHeaders.FirstOrDefault();
            if (callIdHeader == null || !callIdHeadersExist)
            {
                throw new InvalidOperationException("RidgeCallId not found in header. Did you setup caller correctly?");
            }

            Data[callIdHeader] = exception;
        }

        public static Exception? GetData(
            string callId)
        {
            var callDataFound = Data.TryGetValue(callId, out var data);
            if (!callDataFound)
            {
                throw new InvalidOperationException("RidgeCallId not found in dictionary. Did you setup caller correctly?");
            }

            return data;
        }
    }
}
