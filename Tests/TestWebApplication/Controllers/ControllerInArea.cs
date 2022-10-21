using Microsoft.AspNetCore.Mvc;
using Ridge;
using System.Threading.Tasks;

namespace TestWebApplication.Controllers
{
    [Area("Area")]
    [Route("[controller]")]
    [GenerateCaller]
    public class ControllerInArea : ControllerBase
    {
        [HttpGet("index")]
        public virtual async Task<ActionResult> Index()
        {
            await Task.CompletedTask;
            return Ok();
        }
    }
}
