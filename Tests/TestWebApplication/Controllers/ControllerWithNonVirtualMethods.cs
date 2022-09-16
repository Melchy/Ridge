using Microsoft.AspNetCore.Mvc;
using Ridge;
using WeirdNamespace;

namespace TestWebApplication.Controllers
{
    [Route("[controller]")]
    [GenerateStronglyTypedCallerForTesting]
    public class ControllerWithNonVirtualMethods : ControllerBase
    {
        [HttpGet("XXXX")]
        public virtual ClassInDifferentNamespace.Nested ReturnTypeInNestedClass(
            ClassInDifferentNamespace.Nested nested)
        {
            return nested;
        }


        [HttpGet("XXXX")]
        public virtual ClassInDifferentNamespace ReturnTypeInDifferentNamespace(
            ClassInDifferentNamespace classInDifferentNamespace)
        {
            return classInDifferentNamespace;
        }
        
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
