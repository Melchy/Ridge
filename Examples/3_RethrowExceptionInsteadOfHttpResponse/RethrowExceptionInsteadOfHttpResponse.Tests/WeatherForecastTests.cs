using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using RethrowExceptionInsteadOfHttpResponse.Mvc.Controllers;

namespace RethrowExceptionInsteadOfHttpResponse.Tests;

public class WeatherForecastTests
{
    [Fact]
    public async Task GetWeatherForecast()
    {
        await using var webApplicationFactory = new WebApplicationFactory<Program>().WithRidge(); 
        var webApplicationClient = webApplicationFactory.CreateClient();
        WeatherForecastControllerClient client = new(webApplicationClient, webApplicationFactory.Services);

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.Get());
    }
}
