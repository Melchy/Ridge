using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Ridge.ExceptionHandling;

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
            exceptionManager.InsertException(context, ex);
            throw;
        }
    }
}