using Xunit;
using Xunit.Abstractions;

namespace SimpleExample.Tests;

public class WeatherForecastTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public WeatherForecastTests(
        ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public async Task GetWeatherForecast()
    {
        await using var webApp = new WebApp(_testOutputHelper);

        var response = await webApp.WeatherForecastControllerClient.Get();
        
        Assert.True(response.IsSuccessStatusCode);
        Assert.Single(response.Result);
        Assert.Equal("Cool", response.Result.First().Summary);
        Assert.Equal(1, response.Result.First().TemperatureC);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Now), response.Result.First().Date);
    }
}
