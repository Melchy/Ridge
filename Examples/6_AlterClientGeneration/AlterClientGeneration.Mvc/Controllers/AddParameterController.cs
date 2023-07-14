using Microsoft.AspNetCore.Mvc;
using Ridge.AspNetCore.GeneratorAttributes;

namespace AlterClientGeneration.Mvc.Controllers;

[ApiController]
[Route("[controller]")]
[GenerateClient]
[AddParameterToClient(typeof(string), "country", ParameterMapping.MapToQueryOrRouteParameter)]
public class AddParameterController : ControllerBase
{
    [HttpGet("{country}")]
    public IEnumerable<WeatherForecast> Get()
    {
        var country = (string?)HttpContext.Request.RouteValues["country"];
        return new List<WeatherForecast>()
        {
            new(
                Date: DateOnly.FromDateTime(DateTime.Now),
                Summary: "Cool",
                TemperatureC: 1,
                Country: country
            ),
        };
    }
}
