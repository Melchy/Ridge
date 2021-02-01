using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ridge.Results;

namespace TestWebApplication.Pages
{
    public class PageWithIncorrectReturnTypeInCustomActionResult : PageModel
    {
        public virtual async Task<PageResult<PageWithMultipleNonVirtualMethod>> OnGetGenericTypeOfCustomActionResultIsWrong()
        {
            return await Task.FromResult(Page());
        }
        
        public virtual async Task<PageResult<PageWithMultipleNonVirtualMethod>> OnGetIncorrectToo()
        {
            return await Task.FromResult(Page());
        }
    }
}
