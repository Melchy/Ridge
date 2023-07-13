using Microsoft.AspNetCore.Mvc;
using Ridge.AspNetCore.GeneratorAttributes;

namespace AlterClientGeneration.Controllers;

[ApiController]
[Route("[controller]")]
[GenerateClient]
[TransformActionParameter(typeof(IServiceProvider), typeof(void), ParameterMapping.None)]
public class RemoveParameterInClientController : ControllerBase
{
    [HttpGet]
    public IEnumerable<WeatherForecast> Get(
        IServiceProvider serviceProvider)
    {
        return new List<WeatherForecast>()
        {
            new(
                Date: DateOnly.FromDateTime(DateTime.Now),
                Summary: "Cool",
                TemperatureC: 1
            ),
        };
    }
}
