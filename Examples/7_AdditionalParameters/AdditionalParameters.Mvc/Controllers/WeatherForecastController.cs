using Microsoft.AspNetCore.Mvc;
using Ridge.AspNetCore.GeneratorAttributes;

namespace AddtitionalParamters.Controllers;

[ApiController]
[Route("[controller]")]
// Add attribute to generate client
[GenerateClient]
public class WeatherForecastController : ControllerBase
{
    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        var summary = HttpContext.Request.Headers["summary"];
        var temperatureC = HttpContext.Request.Headers["temperatureC"];
        
        return new List<WeatherForecast>()
        {
            new(
                Date: DateOnly.FromDateTime(DateTime.Now),
                Summary: summary,
                TemperatureC: int.Parse(temperatureC!)
            ),
        };
    }
}

public record WeatherForecast(
    DateOnly Date,
    int TemperatureC,
    string? Summary);
