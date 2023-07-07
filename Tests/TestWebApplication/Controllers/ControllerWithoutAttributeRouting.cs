using Microsoft.AspNetCore.Mvc;
using Ridge.AspNetCore.GeneratorAttributes;
using System.Threading.Tasks;

namespace TestWebApplication.Controllers;

[GenerateClient]
public class ControllerWithoutAttributeRouting : ControllerBase
{
    public virtual async Task<ActionResult> HttpGetWithoutBody()
    {
        return await Task.FromResult(Ok());
    }

    public virtual Task<ActionResult> Test(
        object obje)
    {
        return Task.FromResult<ActionResult>(Ok());
    }
}
