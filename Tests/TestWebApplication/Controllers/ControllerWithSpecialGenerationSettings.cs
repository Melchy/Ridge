using Microsoft.AspNetCore.Mvc;
using Ridge;
using System.Threading.Tasks;

namespace TestWebApplication.Controllers;

[GenerateStronglyTypedCallerForTesting(UseHttpResponseMessageAsReturnType = true)]
public class ControllerWithSpecialGenerationSettings : ControllerBase
{
    public async Task<ActionResult<string>> SimpleGet()
    {
        return "return";
    }
}
