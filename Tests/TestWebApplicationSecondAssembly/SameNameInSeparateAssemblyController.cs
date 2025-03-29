using Microsoft.AspNetCore.Mvc;
using Ridge.AspNetCore.GeneratorAttributes;

namespace TestWebApplicationSecondAssembly;

[Route("separateAssembly2")]
[GenerateClient]
public class SameNameInSeparateAssemblyController : ControllerBase
{
    [HttpGet("index")]
    public virtual async Task<ActionResult> Index()
    {
        await Task.CompletedTask;
        return Ok();
    }
}
