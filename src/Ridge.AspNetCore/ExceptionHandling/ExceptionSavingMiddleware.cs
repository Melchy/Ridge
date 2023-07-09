using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ridge.AspNetCore.Options;
using System;
using System.Threading.Tasks;

namespace Ridge.AspNetCore.ExceptionHandling;

internal class ExceptionSavingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Func<Exception, bool> _exceptionSavingFilter;
    
    public ExceptionSavingMiddleware(
        RequestDelegate next,
        IOptions<RidgeAspNetCoreOptions> options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _exceptionSavingFilter = options.Value?.ExceptionRethrowFilter ?? (_ => true);
    }


    public async Task Invoke(
        HttpContext context)
    {
        var configuration = context.RequestServices.GetService<IConfiguration>();
        if (configuration?["Ridge:IsTestCall"] != "true")
        {
            await _next(context);
            return;
        }

        var exceptionManager = context.RequestServices.GetRequiredService<ExceptionManager>();

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (_exceptionSavingFilter(ex))
            {
                exceptionManager.InsertException(context, ex);
            }
            
            throw;
        }
    }
}
