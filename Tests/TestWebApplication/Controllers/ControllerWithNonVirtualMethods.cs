using Microsoft.AspNetCore.Mvc;

namespace TestWebApplication.Controllers
{
    [Route("[controller]")]
    public class ControllerWithNonVirtualMethods : ControllerBase
    {
        [HttpGet("a")]
        public virtual ActionResult Index()
        {
            return Ok();
        }

        [HttpGet("b")]
        public ActionResult NonVirtual()
        {
            return Ok();
        }
        
        [HttpGet("c")]
        public ActionResult NonVirtual2()
        {
            return Ok();
        }
    }
}
