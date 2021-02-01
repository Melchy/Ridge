using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ridge.CallData;
using System;

namespace Ridge.Filters
{
    public class RidgeFilter : IOrderedFilter
    {
        // This filter should be one of the last filters applied
        public int Order { get => Int32.MaxValue - 1;  }

        public (ActionResult?, bool continueTestCall) UnwrapActionResultAndCheckIfTestCallShouldBeContinued(HttpContext context, Exception? exception, object result)
        {
            if (exception != null)
            {
                return (null, false);
            }

            if (result is IResultWrapper resultWrapper)
            {
                var innerResult = resultWrapper.GetInnerActionResult();
                if (!CallDataDictionary.IsTestCall(context))
                {
                    return (innerResult, false);
                }
                return (innerResult, true);
            }

            return (null, false);
        }
    }
}
