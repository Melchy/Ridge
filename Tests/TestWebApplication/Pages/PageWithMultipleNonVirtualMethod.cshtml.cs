using Microsoft.AspNetCore.Mvc.RazorPages;
using Ridge.Results;
using System.Threading.Tasks;

namespace TestWebApplication.Pages
{
    public class PageWithMultipleNonVirtualMethod : PageModel
    {
        public async Task<PageResult<PageWithMultipleNonVirtualMethod>> OnGetNonVirtual()
        {
            return await Task.FromResult(Page());
        }

        public async Task<PageResult<PageWithMultipleNonVirtualMethod>> OnPostNonVirtual()
        {
            return await Task.FromResult(Page());
        }

        public virtual async Task<PageResult<PageWithMultipleNonVirtualMethod>> OnGetSomething()
        {
            return await Task.FromResult(Page());
        }
    }
}
