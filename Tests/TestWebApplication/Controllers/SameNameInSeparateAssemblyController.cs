using Microsoft.AspNetCore.Mvc;
using Ridge.AspNetCore.GeneratorAttributes;
using System.Threading.Tasks;

namespace TestWebApplication.Controllers;

[Route("separateAssembly1")]
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
