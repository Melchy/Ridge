using Microsoft.AspNetCore.Mvc;
using Ridge.GeneratorAttributes;
using System.Threading.Tasks;

namespace TestWebApplication.Controllers;

[GenerateCaller]
[TransformParameterInCaller(typeof(string), typeof(string), ParameterMapping.MapToQueryOrRouteParameter)]
[TransformParameterInCaller(typeof(TestEnum), typeof(TestEnum), ParameterMapping.MapToQueryOrRouteParameter)]
[TransformParameterInCaller(typeof(int), typeof(int), ParameterMapping.MapToHeader)]
[TransformParameterInCaller(typeof(double), typeof(double), ParameterMapping.MapToBody)]
public class TransformedParametersWithDefaultMappingController : ControllerBase
{
    [HttpGet("transformation/{route}")]
    public async Task<ActionResult<(string fromRouteParameter, TestEnum fromQueryParameter, double body, int fromHeaderParameter)>> DefaultAction(
        [FromQuery] TestEnum query,
        [FromRoute] string route,
        [FromHeader] int header,
        [FromBody] double body)
    {
        return (route, query, body, header);
    }

    public enum TestEnum
    {
        None = 0,
    }
}
