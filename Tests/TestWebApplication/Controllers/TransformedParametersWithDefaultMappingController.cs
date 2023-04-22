using Microsoft.AspNetCore.Mvc;
using Ridge.GeneratorAttributes;
using System.Threading.Tasks;

namespace TestWebApplication.Controllers;

[GenerateClient]
[TransformActionParameter(typeof(string), typeof(string), ParameterMapping.MapToQueryOrRouteParameter)]
[TransformActionParameter(typeof(TestEnum), typeof(TestEnum), ParameterMapping.MapToQueryOrRouteParameter)]
[TransformActionParameter(typeof(int), typeof(int), ParameterMapping.MapToHeader)]
[TransformActionParameter(typeof(double), typeof(double), ParameterMapping.MapToBody)]
public class TransformedParametersWithDefaultMappingController : ControllerBase
{
    [HttpGet("transformation/{route}")]
    public Task<ActionResult<(string fromRouteParameter, TestEnum fromQueryParameter, double body, int fromHeaderParameter)>> DefaultAction(
        [FromQuery] TestEnum query,
        [FromRoute] string route,
        [FromHeader] int header,
        [FromBody] double body)
    {
        return Task.FromResult<ActionResult<(string fromRouteParameter, TestEnum fromQueryParameter, double body, int fromHeaderParameter)>>((route, query, body, header));
    }

    public enum TestEnum
    {
        None = 0,
    }
}
