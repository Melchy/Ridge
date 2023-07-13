using AddtitionalParamters.Controllers;
using AlterRequestGeneration.Tests;
using Microsoft.AspNetCore.Mvc.Testing;
using Ridge.AspNetCore.Parameters;
using Ridge.Parameters.AdditionalParams;
using Xunit;

namespace CustomParameters.Tests;

public class WeatherForecastTests
{
    [Fact]
    public async Task GetWeatherForecast()
    {
        await using var webApplicationFactory = new WebApplicationFactory<Program>()
           .WithRidge(x =>
            {
                x.UseHttpRequestFactoryMiddleware(new TemperatureMiddleware());
            }); 
        var webApplicationClient = webApplicationFactory.CreateClient();
        WeatherForecastControllerClient client = new(webApplicationClient, webApplicationFactory.Services);

        // Parameter summary uses HttpHeaderParameter that is automatically mapped to header
        // Parameter temperatureC uses AdditionalParameter which must be mapped manually
        // (this is used only as demonstration. The result is the same as using HttpHeaderParameter)
        var response = await client.Get(new HttpHeaderParameter("summary", "Cool"), new AdditionalParameter("temperatureC", 1));
        
        Assert.True(response.IsSuccessStatusCode);
        Assert.Single(response.Result);
        Assert.Equal("Cool", response.Result.First().Summary);
        Assert.Equal(1, response.Result.First().TemperatureC);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Now), response.Result.First().Date);
    }
}

