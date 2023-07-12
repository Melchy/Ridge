using Microsoft.AspNetCore.Mvc;
using Ridge.AspNetCore.GeneratorAttributes;

namespace SimpleExample.Controllers;

[ApiController]
[Route("[controller]")]
// Add attribute to generate client
[GenerateClient]
public class WeatherForecastController : ControllerBase
{
    [HttpGet(Name = "GetWeatherForecast")]
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
