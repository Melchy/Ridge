using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Ridge.CallData
{
    public static class CallDataDictionary
    {
        private static ConcurrentDictionary<string, CallDataDto> Data { get; } = new ConcurrentDictionary<string, CallDataDto>();

        public static void InsertEmptyDataToIndicateTestCall(Guid requestId)
        {
            Data[requestId.ToString()] = new CallDataDto();
        }
        
        public static bool IsTestCall(HttpContext httpContext)
        {
            var headerPresent = httpContext.Request.Headers.TryGetValue("callId", out var value);
            if (!headerPresent)
            {
                return false;
            }

            var callIdPresent = Data.TryGetValue(value, out _);
            return callIdPresent;
        }
        
        public static void InsertModel(HttpContext httpContext, object? model)
        {
            var callIdHeadersExist = httpContext.Request.Headers.TryGetValue("callId", out var callIdHeaders);
            var callIdHeader = callIdHeaders.FirstOrDefault();
            if (callIdHeader == null || !callIdHeadersExist)
            {
                throw new InvalidOperationException("CallId not found in header. Did you setup caller correctly?");
            }
            Data[callIdHeader] = new CallDataDto(){PageModel = model, Exception = null};
        }
        
        public static void InsertException(HttpContext httpContext, Exception exception)
        {
            var callIdHeadersExist = httpContext.Request.Headers.TryGetValue("callId", out var callIdHeaders);
            var callIdHeader = callIdHeaders.FirstOrDefault();
            if (callIdHeader == null || !callIdHeadersExist)
            {
                throw new InvalidOperationException("CallId not found in header. Did you setup caller correctly?");
            }
            Data[callIdHeader] = new CallDataDto(){PageModel = null, Exception = exception};
        }
        
        public static CallDataDto GetData(string callId)
        {
            var callDataFound = Data.TryGetValue(callId, out var data);
            if (!callDataFound)
            {
                throw new InvalidOperationException("CallId not found in dictionary. Did you setup caller correctly?");
            }

            if (data == null)
            {
                throw new InvalidOperationException("CallId found in dictionary but data are se to null. Did you clear CallId Header?");
            }

            return data;
        }

        public static bool IsEmpty()
        {
            return Data.IsEmpty;
        }

        public static void Clear()
        {
            Data.Clear();
        }
    }
}
