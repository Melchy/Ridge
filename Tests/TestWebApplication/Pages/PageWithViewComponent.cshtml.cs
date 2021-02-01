using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestWebApplication.Pages
{
    public class PageWithViewComp : PageModel
    {
        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
