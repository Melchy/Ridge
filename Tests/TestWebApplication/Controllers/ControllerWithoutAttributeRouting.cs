using Microsoft.AspNetCore.Mvc;
using Ridge;
using System.Threading.Tasks;

namespace TestWebApplication.Controllers
{
    [GenerateCallerForTesting]
    public class ControllerWithoutAttributeRouting : ControllerBase
    {
        public virtual async Task<ActionResult> HttpGetWithoutBody()
        {
            return await Task.FromResult(Ok());
        }
    }
}

