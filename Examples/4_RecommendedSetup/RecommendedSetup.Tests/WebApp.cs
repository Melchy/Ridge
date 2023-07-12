using Microsoft.AspNetCore.Mvc.Testing;
using RecommendedSetup.Controllers;
using Xunit.Abstractions;

namespace SimpleExample.Tests;

public sealed class WebApp : IAsyncDisposable
{
    private readonly WebApplicationFactory<Program> _webApplicationFactory;

    public readonly WeatherForecastControllerClient WeatherForecastControllerClient; 
        
    public WebApp(
        ITestOutputHelper testOutputHelper)
    {
        _webApplicationFactory = new WebApplicationFactory<Program>().WithRidge(x =>
        {
            x.UseXunitLogWriter(testOutputHelper);
        }); 
        var webApplicationClient = _webApplicationFactory.CreateClient();
        WeatherForecastControllerClient = new WeatherForecastControllerClient(webApplicationClient, _webApplicationFactory.Services);
    }
    
    public ValueTask DisposeAsync()
    {
        return _webApplicationFactory.DisposeAsync();
    }
}
