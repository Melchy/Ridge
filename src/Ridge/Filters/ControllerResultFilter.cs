using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace Ridge.Filters
{
    public class ControllerResultFilter : RidgeFilter, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var pageHandlerExecutedContext = await next.Invoke();
            var (actionResult, _) = UnwrapActionResultAndCheckIfTestCallShouldBeContinued(pageHandlerExecutedContext.HttpContext, pageHandlerExecutedContext.Exception, pageHandlerExecutedContext.Result);
            pageHandlerExecutedContext.Result = actionResult;
        }
    }
}
