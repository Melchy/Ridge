using Microsoft.AspNetCore.Mvc;
using Ridge.Results;

namespace TestWebApplication.Controllers
{
    [Area("Area")]
    [Route("[controller]")]
    public class ControllerInArea : ControllerBase
    {
        [HttpGet("index")]
        public virtual ControllerResult Index()
        {
            return Ok();
        }
    }
}
