using Microsoft.AspNetCore.Mvc;
using Ridge.AspNetCore.GeneratorAttributes;

namespace ApplicationWithDefaultSerialization.Controllers;

[ApiController]
[Route("[controller]")]
[GenerateClient]
public class NoTransformationWithSingleEndpoint : ControllerBase
{
    public void SomeRandomMethod()
    {
    }
}
