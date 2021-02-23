using Microsoft.AspNetCore.Mvc;
using Ridge.Results;
using System.Threading.Tasks;

namespace TestWebApplication.Controllers
{
    [Area("Area")]
    [Route("[controller]")]
    public class ControllerInArea : ControllerBase
    {
        [HttpGet("index")]
        public virtual async Task<ControllerResult> Index()
        {
            await Task.CompletedTask;
            return Ok();
        }
    }
}
