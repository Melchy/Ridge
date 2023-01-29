using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Ridge.GeneratorAttributes;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApplication.Controllers;

[GenerateCaller]
[Route("[controller]")]
[AddParameterToCaller(typeof(string), "parameterToRoute", ParameterMapping.MapToQueryOrRouteParameter)]
[AddParameterToCaller(typeof(string), "parameterToQuery", ParameterMapping.MapToQueryOrRouteParameter)]
[AddParameterToCaller(typeof(ComplexObject), "parameterToBody", ParameterMapping.MapToBody)]
[AddParameterToCaller(typeof(string), "parameterToHeader", ParameterMapping.MapToHeader)]
public class AddedParametersWithDefaultMappingController : ControllerBase
{
    [HttpGet("GetWithBody/{parameterToRoute}")]
    public async Task<ActionResult<(string fromRouteParameter, string fromQueryParameter, ComplexObject body, string fromHeaderParameter)>> DefaultAction()
    {
        var parameterAddedToRoute = HttpContext.Request.RouteValues["parameterToRoute"]!.ToString()!;
        var parameterAddedToQuery = HttpContext.Request.Query["parameterToQuery"].First()!;
        var parameterAddedToBody = JsonConvert.DeserializeObject<ComplexObject>(await new StreamReader(HttpContext.Request.Body).ReadToEndAsync());
        var parameterAddedToHeader = HttpContext.Request.Headers["parameterToHeader"].FirstOrDefault()!;
        return (parameterAddedToRoute, parameterAddedToQuery, parameterAddedToBody, parameterAddedToHeader);
    }
}
