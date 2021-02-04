using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Ridge.CallData;
using System;
using System.Threading.Tasks;

namespace Ridge.Setup
{
    internal class ExceptionSavingMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeveloperExceptionPageMiddleware"/> class
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="diagnosticSource"></param>
        /// <param name="filters"></param>
        public ExceptionSavingMiddleware(
            RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }
        
        
        public async Task Invoke(HttpContext context)
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
