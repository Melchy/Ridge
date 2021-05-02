using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TestWebApplication.Controllers
{
    public class ControllerWithoutAttributeRouting : ControllerBase
    {
        public virtual async Task<ActionResult> HttpGetWithoutBody()
        {
            return await Task.FromResult(Ok());
        }
    }
}
