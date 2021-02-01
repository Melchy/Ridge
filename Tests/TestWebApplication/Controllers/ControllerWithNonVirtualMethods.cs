using Microsoft.AspNetCore.Mvc;
using Ridge.Results;

namespace TestWebApplication.Controllers
{
    [Route("[controller]")]
    public class ControllerWithNonVirtualMethods : ControllerBase
    {
        [HttpGet("a")]
        public virtual ControllerResult Index()
        {
            return Ok();
        }

        [HttpGet("b")]
        public ControllerResult NonVirtual()
        {
            return Ok();
        }
        
        [HttpGet("c")]
        public ControllerResult NonVirtual2()
        {
            return Ok();
        }
    }
}
