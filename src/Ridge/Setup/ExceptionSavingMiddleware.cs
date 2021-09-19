using Microsoft.AspNetCore.Http;
using Ridge.CallData;
using System;
using System.Threading.Tasks;

namespace Ridge.Setup
{
    internal class ExceptionSavingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionSavingMiddleware(
            RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }


        public async Task Invoke(
            HttpContext context)
        {
            if (!CallDataDictionary.IsTestCall(context))
            {
                await _next(context);
                return;
            }

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                CallDataDictionary.InsertException(context, ex);
                throw;
            }
        }
    }
}
