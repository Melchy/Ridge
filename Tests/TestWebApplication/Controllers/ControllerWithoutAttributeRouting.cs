using Microsoft.AspNetCore.Mvc;
using Ridge;
using System.Threading.Tasks;

namespace TestWebApplication.Controllers
{
    [GenerateCaller]
    public class ControllerWithoutAttributeRouting : ControllerBase
    {
        public virtual async Task<ActionResult> HttpGetWithoutBody()
        {
            return await Task.FromResult(Ok());
        }

        public virtual async Task<ActionResult> Test(
            object obje)
        {
            return Ok();
        }
    }
}

