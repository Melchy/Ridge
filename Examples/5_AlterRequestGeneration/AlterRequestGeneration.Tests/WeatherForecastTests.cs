using Microsoft.AspNetCore.Mvc.Testing;
using AlterRequestGeneration.Controllers;
using Asp.Versioning;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace AlterRequestGeneration.Tests;

public class WeatherForecastTests
{
    private readonly ITestOutputHelper _outputHelper;

    public WeatherForecastTests(
        ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }
    
    [Fact]
    public async Task GetWeatherForecast()
    {
        await using var webApplicationFactory = new WebApplicationFactory<Program>()
           .WithRidge(x =>
            {
                x.UseXunitLogWriter(_outputHelper);
                x.UseHttpRequestFactoryMiddleware(new ApiVersionMiddleware());
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
