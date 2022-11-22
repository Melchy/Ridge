using Microsoft.AspNetCore.Mvc;
using Ridge.GeneratorAttributes;
using System.Threading.Tasks;

namespace ApplicationWithDefaultSerialization.Controllers;

[ApiController]
[Route("[controller]")]
[GenerateCaller]
public class TestController : ControllerBase
{
    [HttpGet("ArgumentsWithoutAttributes/{fromRoute}")]
    public virtual async Task<ActionResult<SpecialComplexObject>> ArgumentsWithoutAttributes(
        ComplexObject complexObjectFromQuery,
        int fromRoute,
        int fromQuery)
    {
        var foo = new SpecialComplexObject()
        {
            ComplexObject = complexObjectFromQuery,
            FromQuery = fromQuery,
            FromRoute = fromRoute,
        };
        return foo;
    }
}

public class ComplexObject
{
    public string Str { get; set; } = null!;
    public NestedComplexObject NestedComplexObject { get; set; } = null!;
}

public class NestedComplexObject
{
    public string Str { get; set; } = null!;
    public int Integer { get; set; }
}

public class SpecialComplexObject
{
    public ComplexObject ComplexObject { get; set; } = null!;
    public int FromRoute { get; set; }
    public int FromQuery { get; set; }
}