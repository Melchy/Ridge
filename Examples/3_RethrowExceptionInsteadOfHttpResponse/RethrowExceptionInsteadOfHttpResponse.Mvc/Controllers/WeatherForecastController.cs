using Microsoft.AspNetCore.Mvc;
using Ridge.AspNetCore.GeneratorAttributes;

namespace RethrowExceptionInsteadOfHttpResponse.Mvc.Controllers;

[ApiController]
[Route("[controller]")]
// Add attribute to generate client
[GenerateClient]
public class WeatherForecastController : ControllerBase
{
    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        throw new InvalidOperationException("Very sophisticated exception");
    }
}

public record WeatherForecast(
    DateOnly Date,
    int TemperatureC,
    string? Summary);
