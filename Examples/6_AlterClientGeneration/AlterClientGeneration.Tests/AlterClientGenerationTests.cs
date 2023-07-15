using Microsoft.AspNetCore.Mvc.Testing;
using AlterClientGeneration.Mvc.Controllers;
using Asp.Versioning;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace AlterClientGeneration.Tests;

public class AlterClientGenerationTests
{
    private readonly ITestOutputHelper _outputHelper;

    public AlterClientGenerationTests(
        ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }
    
    [Fact]
    public async Task RemovedParameter()
    {
        await using var webApplicationFactory = new WebApplicationFactory<Program>()
           .WithRidge(x =>
            {
                x.UseXunitLogWriter(_outputHelper);
            }); 
        var webApplicationClient = webApplicationFactory.CreateClient();
        RemoveParameterInClientControllerClient clientController = new(webApplicationClient, webApplicationFactory.Services);

        // Parameter IServiceCollection is not present in the generated client
        // Note that parameters with attribute [FromServices] and CancellationToken are removed by default
        var response = await clientController.Get();
        
        Assert.True(response.IsSuccessStatusCode);
        Assert.Single(response.Result);
        Assert.Equal("Cool", response.Result.First().Summary);
        Assert.Equal(1, response.Result.First().TemperatureC);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Now), response.Result.First().Date);
    }
    
    [Fact]
    public async Task TransformedParameter()
    {
        await using var webApplicationFactory = new WebApplicationFactory<Program>()
           .WithRidge(x =>
            {
                x.UseXunitLogWriter(_outputHelper);
            }); 
        var webApplicationClient = webApplicationFactory.CreateClient();
        TransformParameterControllerClient clientController = new(webApplicationClient, webApplicationFactory.Services);

        var response = await clientController.Get("CZ");
        
        Assert.True(response.IsSuccessStatusCode);
        Assert.Single(response.Result);
        Assert.Equal("CZ", response.Result.First().Country);
    }
    
    [Fact]
    public async Task AddParameter()
    {
        await using var webApplicationFactory = new WebApplicationFactory<Program>()
           .WithRidge(x =>
            {
                x.UseXunitLogWriter(_outputHelper);
            }); 
        var webApplicationClient = webApplicationFactory.CreateClient();
        AddParameterControllerClient clientController = new(webApplicationClient, webApplicationFactory.Services);

        var response = await clientController.Get("CZ");
        
        Assert.True(response.IsSuccessStatusCode);
        Assert.Single(response.Result);
        Assert.Equal("CZ", response.Result.First().Country);
    }
    
    [Fact]
    public async Task TransformParameterWithManualMapping()
    {
        await using var webApplicationFactory = new WebApplicationFactory<Program>()
           .WithRidge(x =>
            {
                x.UseHttpRequestFactoryMiddleware(new CountryCodeMiddleware());
                x.UseXunitLogWriter(_outputHelper);
            });
        var webApplicationClient = webApplicationFactory.CreateClient();
        TransformParameterWithManualMappingControllerClient clientController = new(webApplicationClient, webApplicationFactory.Services);

        var response = await clientController.Get("CZ");
        
        Assert.True(response.IsSuccessStatusCode);
        Assert.Single(response.Result);
        Assert.Equal("CZ", response.Result.First().Country);
    }
}
