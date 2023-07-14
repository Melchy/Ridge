namespace AlterClientGeneration.Mvc.Controllers;

public record WeatherForecast(
    DateOnly Date,
    int TemperatureC,
    string? Summary,
    string? Country = null);
