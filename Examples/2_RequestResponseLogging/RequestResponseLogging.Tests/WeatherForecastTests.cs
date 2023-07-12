using Microsoft.AspNetCore.Mvc.Testing;
using SimpleExample.Controllers;
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
        await using var webApplicationFactory = new WebApplicationFactory<Program>().WithRidge(x =>
        {
            x.UseXunitLogWriter(_testOutputHelper);
        }); 
        var webApplicationClient = webApplicationFactory.CreateClient();
        WeatherForecastControllerClient client = new(webApplicationClient, webApplicationFactory.Services);

        var response = await client.Get();
        
        Assert.True(response.IsSuccessStatusCode);
        Assert.Single(response.Result);
        Assert.Equal("Cool", response.Result.First().Summary);
        Assert.Equal(1, response.Result.First().TemperatureC);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Now), response.Result.First().Date);
    }
}
