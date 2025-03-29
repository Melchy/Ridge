using Microsoft.AspNetCore.Mvc;
using Ridge.AspNetCore.GeneratorAttributes;

namespace ApplicationWithDefaultSerialization.Controllers;

[ApiController]
[Route("[controller]")]
[GenerateClient]
public class TransformEndpointAndMethodNameEndpoint : ControllerBase
{
    public void Execute()
    {
    }
}
