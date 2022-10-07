using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ridge;
using System.Threading.Tasks;

namespace TestWebApplication.Controllers;

[GenerateCallerForTesting(UseHttpResponseMessageAsReturnType = true)]
[TransformTypeInCaller(fromType: typeof(TestController.ComplexArgument), toType: typeof(void))]
[TransformTypeInCaller(fromType: typeof(ILogger), toType: typeof(void))]
[TransformTypeInCaller(fromType: typeof(int), toType: typeof(string))]
public class ControllerWithSpecialGenerationSettings : ControllerBase
{
    public async Task<ActionResult<string>> SimpleGet()
    {
        return "return";
    }

    public async Task<ActionResult<string>> TypeTransformation(
        TestController.ComplexArgument complexArgument,
        ILogger logger,
        int a)
    {
        return "return";
    }
}
