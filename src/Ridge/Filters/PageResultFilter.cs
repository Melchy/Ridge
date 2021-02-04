using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ridge.CallData;
using System.Threading.Tasks;

namespace Ridge.Filters
{
    internal class PageResultFilter : RidgeFilter, IAsyncPageFilter
    {
        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            var pageHandlerExecutedContext = await next.Invoke();

            var (actionResult, continueTestCall) = UnwrapActionResultAndCheckIfTestCallShouldBeContinued(context.HttpContext, pageHandlerExecutedContext.Exception, pageHandlerExecutedContext.Result);
            pageHandlerExecutedContext.Result = actionResult;
            if (continueTestCall)
            {
                if (actionResult is PageResult pageResult)
                {
                    pageResult.ViewData ??= ((PageModel)context.HandlerInstance).ViewData;
                }
                CallDataDictionary.InsertModel(context.HttpContext, context.HandlerInstance);
            }
        }

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            return Task.CompletedTask;
        }
    }
}
