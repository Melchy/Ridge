using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ridge.GeneratorAttributes;
using System;
using System.Threading.Tasks;

namespace TestWebApplication.Controllers;

[GenerateCaller(UseHttpResponseMessageAsReturnType = true)]
[TransformParameterInCaller(fromType: typeof(TestController.ComplexArgument), toType: typeof(void), ParameterMapping.None)]
[TransformParameterInCaller(fromType: typeof(ILogger), toType: typeof(void), ParameterMapping.None)]
[TransformParameterInCaller(fromType: typeof(float), toType: typeof(string), ParameterMapping.None, Optional = true, GeneratedParameterName = "renamed")]
[AddParameterToCaller(typeof(int?), "addedParameter", ParameterMapping.None, Optional = false)]
[AddParameterToCaller(typeof(string), "addedOptionalParameter", ParameterMapping.None, Optional = true)]
[AddParameterToCaller(typeof(Task<string>), "addedGenericOptionalParameter", ParameterMapping.None, Optional = true)]
[AddParameterToCaller(typeof(object), "renamed", ParameterMapping.None, Optional = true)]
public class ControllerWithSpecialGenerationSettings : ControllerBase
{
    public const int DefaultValue = 1;
    
    [HttpGet]
    public async Task<ActionResult<string>> SimpleGet()
    {
        return "return";
    }

    [HttpGet]
    public async Task<ActionResult<string>> TypeTransformation(
        TestController.ComplexArgument complexArgument,
        ILogger logger,
        float a)
    {
        return "return";
    }
    
    [HttpGet]
    public async Task<ActionResult<int?>> ActionWithOptionalParameter(
        string test2,
        float floatToBeMadeOptionalString,
        string test3,
        float floatToBeMadeOptionalString2,
        string[] test,
        string optionalWithoutTransformation = "asd",
        char optionalChar = 'z',
        TestEnum optionalEnum = TestEnum.Value,
        int optionalInt = 1,
        double optionalDouble = 2.3,
        int optionalWithFullDefault = default,
        int defaultConst = DefaultValue,
        DateTime defaultStruct = new(),
        int optional = default,
        [FromServices] object? fromServices = default,
        [FromServicesAttribute] object? fromServices2 = default)
    {
        return optional;
    }
}

public enum TestEnum
{
    Value = 0,
}




