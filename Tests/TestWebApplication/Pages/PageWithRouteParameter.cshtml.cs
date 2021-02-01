using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ridge.Results;

namespace TestWebApplication.Pages
{
    public class PageWithRouteParameter : PageModel
    {
        public virtual async Task<PageResult<PageWithRouteParameter>> OnGet([FromRoute] string routeParam)
        {
            return await Task.FromResult(new OkObjectResult(routeParam));
        }
    }
}
