using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ridge.Results;

namespace TestWebApplication.Pages
{
    public class GeneralPageModel : PageModel
    {
        public Test Test { get; set; } = new Test(){ParamFromBody = "asd"};
        
        public virtual async Task<PageResult<GeneralPageModel>> OnGetBadRequest()
        {
            return await Task.FromResult(BadRequest("Bad request error"));
        }
        
        public virtual async Task<PageResult<GeneralPageModel>> OnGetRedirect()
        {
            return await Task.FromResult(RedirectToPage(new {handler="Redirected"}));
        }
        
        public virtual async Task<PageResult<GeneralPageModel>> OnGetRedirected()
        {
            return await Task.FromResult(new OkObjectResult("Redirected!!"));
        }

        public virtual PageResult<GeneralPageModel> OnGet()
        {
            return Page();
        }

        public virtual Task<PageResult<GeneralPageModel>> OnGetThrowsInvalidOperationException()
        {
            throw new InvalidOperationException("Error");
        }
        
        public virtual async Task<PageResult<GeneralPageModel>> OnGetReturn500()
        {
           return await Task.FromResult(new StatusCodeResult(StatusCodes.Status500InternalServerError));
        }
        
        public virtual async Task<PageResult<GeneralPageModel>> OnGetFooStatic()
        {
            Test.ParamFromBody = "Foo";
            return await Task.FromResult(Page());
        }

        public virtual PageResult<GeneralPageModel> OnGetFoo2([FromBody] string ParamFromBody, [FromQuery] string ParamFromQuery)
        {
            Test.ParamFromBody = ParamFromBody;
            Test.ParamFromQuery = ParamFromQuery;
            return Page();
        }
    }

    public class Test
    {
        public string ParamFromBody { get; set; } = null!;
        public string ParamFromQuery { get; set; } = null!;
    }
}
