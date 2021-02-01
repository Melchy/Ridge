using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestWebApplication.Pages
{
    public class PageWithIncorrectReturnType : PageModel
    {
        public virtual void OnGetVoid()
        {
            return;
        }
        
        public virtual int OnGetInt()
        {
            return 1;
        }
    }
}