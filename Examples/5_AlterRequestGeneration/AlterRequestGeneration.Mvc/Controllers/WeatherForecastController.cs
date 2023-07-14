using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Ridge.AspNetCore.GeneratorAttributes;

namespace AlterRequestGeneration.Mvc.Controllers;

[ApiController]
[Route("[controller]")]
[GenerateClient]
[ApiVersion("3.0")]
public class WeatherForecastController : ControllerBase
{
    [HttpGet()]
    public IEnumerable<WeatherForecast> Get()
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

public record WeatherForecast(
    DateOnly Date,
    int TemperatureC,
    string? Summary);
