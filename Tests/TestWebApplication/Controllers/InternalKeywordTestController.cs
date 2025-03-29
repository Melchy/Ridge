using Microsoft.AspNetCore.Mvc;
using Ridge.AspNetCore.GeneratorAttributes;

namespace TestWebApplication.Controllers;

// No Client is generated for those controllers because asp.net core does not allow internal
// If client is generated then it should throw syntax error because it would be public and it would access internal Foo class
[Route("internalClass")]
[ApiController]
[GenerateClient]
internal class InternalKeywordTestController : ControllerBase
{
    [HttpGet("test")]
    public void Test(
        Foo foo)
    {
    }
}


[Route("internalMethod")]
[ApiController]
[GenerateClient]
internal class InternalMethodController : ControllerBase
{
    [HttpGet("method")]
    internal void Test(
        Foo foo)
    {
    }
}

internal class Foo
{
}
