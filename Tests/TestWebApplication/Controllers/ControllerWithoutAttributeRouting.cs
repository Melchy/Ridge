using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
