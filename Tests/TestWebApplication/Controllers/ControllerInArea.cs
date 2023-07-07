using Microsoft.AspNetCore.Mvc;
using Ridge.AspNetCore.GeneratorAttributes;
using System.Threading.Tasks;

namespace TestWebApplication.Controllers;

[Area("Area")]
[Route("[controller]")]
[GenerateClient]
public class ControllerInArea : ControllerBase
{
    [HttpGet("index")]
    public virtual async Task<ActionResult> Index()
    {
        await Task.CompletedTask;
        return Ok();
    }
}
