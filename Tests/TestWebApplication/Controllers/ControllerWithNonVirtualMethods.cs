using Microsoft.AspNetCore.Mvc;
using Ridge.AspNetCore.GeneratorAttributes;

namespace TestWebApplication.Controllers;

[Route("[controller]")]
[GenerateClient]
public class ControllerWithNonVirtualMethods : ControllerBase
{
    [HttpGet("ReturnTypeInNestedClass")]
    public virtual ClassInDifferentNamespace.Nested ReturnTypeInNestedClass(
        ClassInDifferentNamespace.Nested nested)
    {
        return nested;
    }


    [HttpGet("ReturnTypeInDifferentNamespace")]
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
