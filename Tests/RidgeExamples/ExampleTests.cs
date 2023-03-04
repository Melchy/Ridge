using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Ridge.HttpRequestFactoryMiddlewares;
using Ridge.Parameters.AdditionalParams;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TestWebApplication;
using TestWebApplication.Controllers;

namespace RidgeExamples;

public class ExampleTests
{
    // ------------------------------------------Test.cs----------------------------------------------------------------
    [Test]
    public async Task CallControllerUsingRidge()
    {
        using var webApplicationFactory =
            new WebApplicationFactory<Program>() // WebApplicationFactory - https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests
               .WithRidge(); // add ridge dependencies to WebApplicationFactory
        var client = webApplicationFactory.CreateClient(); // create http client
        var examplesControllerClient = new ExamplesControllerClient(client, webApplicationFactory.Services); // create strongly typed client

        var response = await examplesControllerClient.ReturnGivenNumber(10);
    
        Assert.True(response.IsSuccessStatusCode);
        Assert.AreEqual(10, response.Result); // Response is wrapped in HttpCallResponse<int>
    }

    // Equivalent code without using Ridge 
    [Test]
    public async Task CallControllerWithoutRidge()
    {
        using var webApplicationFactory =
            new WebApplicationFactory<Program>(); // WebApplicationFactory - https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests
        var client = webApplicationFactory.CreateClient(); // create http client
    
        var response = await client.GetAsync("/returnGivenNumber/?input=10");
    
        Assert.True(response.IsSuccessStatusCode);
        var responseAsString = await response.Content.ReadAsStringAsync();
        Assert.AreEqual(10, JsonSerializer.Deserialize<int>(responseAsString));
    }


    [Test]
    public async Task ThrowExceptionTest()
    {
        using var webApplicationFactory = new WebApplicationFactory<Program>().WithRidge();
        // notice use of AddExceptionCatching()
        var ridgeHttpClient = webApplicationFactory.CreateClient();
        var examplesControllerClient = new ExamplesControllerClient(ridgeHttpClient, webApplicationFactory.Services);

        try
        {
            _ = await examplesControllerClient.ThrowException();
        }
        catch (InvalidOperationException e)
        {
            Assert.AreEqual("Exception throw", e.Message);
        }
    }


    [Test]
    public async Task HttpRequestFactoryExample()
    {
        using var webApplicationFactory = new WebApplicationFactory<Program>().WithRidge(x =>
        {
            x.HttpRequestFactoryMiddlewares.Add(new AddHeaderHttpRequestFactoryMiddleware("exampleHeader", "exampleHeaderValue"));
        });
        var ridgeHttpClient = webApplicationFactory.CreateClient();
        var examplesControllerClient = new ExamplesControllerClient(ridgeHttpClient, webApplicationFactory.Services);

        // controller finds header by it's name and returns it's value
        var response = await examplesControllerClient.ReturnHeader(headerName: "exampleHeader");
        Assert.AreEqual("exampleHeaderValue", response.Result);
    }

    [Test]
    public async Task ParameterAddedByRidge()
    {
        using var webApplicationFactory = new WebApplicationFactory<Program>().WithRidge();

        var ridgeHttpClient = webApplicationFactory.CreateClient();
        var examplesControllerClient = new ExamplesControllerClient(ridgeHttpClient, webApplicationFactory.Services);

        // controller finds header by it's name and returns it's value
        var response = await examplesControllerClient.ReadQueryParameterFromHttpContext(GeneratedParameter: "queryParameterValue");
        Assert.AreEqual("queryParameterValue", response.Result);
    }

    [Test]
    public async Task CustomModelBinderTest()
    {
        using var webApplicationFactory = new WebApplicationFactory<Program>().WithRidge();

        var ridgeHttpClient = webApplicationFactory.CreateClient();
        var examplesControllerClient = new ExamplesControllerClient(ridgeHttpClient, webApplicationFactory.Services);

        // controller finds header by it's name and returns it's value
        var response = await examplesControllerClient.WithCustomModelBinder("cs-CZ");
        Assert.AreEqual("cs-CZ", response.Result);
    }

    [Test]
    public async Task AdditionalParameter()
    {
        using var webApplicationFactory = new WebApplicationFactory<Program>().WithRidge(x =>
        {
            x.HttpRequestFactoryMiddlewares.Add(new AddHeaderFromAdditionalParameters());
        });

        var ridgeHttpClient = webApplicationFactory.CreateClient();
        var examplesControllerClient = new ExamplesControllerClient(ridgeHttpClient, webApplicationFactory.Services);

        // action returns all passed headers
        var response = await examplesControllerClient.ReturnAllHeaders(additionalParameters: new AdditionalParameter("exampleHeader", "exampleHeaderValue"));

        Assert.AreEqual("exampleHeaderValue", response.Result.First(x => x.key == "exampleHeader").value);
    }
}

public class AddHeaderFromAdditionalParameters : HttpRequestFactoryMiddleware
{
    public override Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        var additionalParameters = requestFactoryContext.ParameterProvider.GetAdditionalParameters();

        foreach (var additionalParameter in additionalParameters)
        {
            requestFactoryContext.Headers.Add(additionalParameter.Name, additionalParameter.Value?.ToString());
        }

        return base.CreateHttpRequest(requestFactoryContext);
    }
}

public class CountryCodeHttpRequestFactoryMiddleware : HttpRequestFactoryMiddleware
{
    public override Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        var parameterValue = requestFactoryContext.ParameterProvider
           .GetClientParameters()
           .GetValueByNameOrDefault<string>("countryCode");
        if (string.IsNullOrEmpty(parameterValue))
        {
            return base.CreateHttpRequest(requestFactoryContext);
        }

        requestFactoryContext.UrlGenerationParameters["countryCode"] = parameterValue;
        return base.CreateHttpRequest(requestFactoryContext);
    }
}

public class MapQueryParameterHttpRequestFactoryMiddleware : HttpRequestFactoryMiddleware
{
    public override Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        var parameterValue = requestFactoryContext.ParameterProvider
           .GetClientParameters()
           .GetValueByNameOrDefault<string>("GeneratedParameter");
        if (string.IsNullOrEmpty(parameterValue))
        {
            return base.CreateHttpRequest(requestFactoryContext);
        }

        requestFactoryContext.UrlGenerationParameters["ExampleQueryParameter"] = parameterValue;
        return base.CreateHttpRequest(requestFactoryContext);
    }
}

public class AddHeaderHttpRequestFactoryMiddleware : HttpRequestFactoryMiddleware
{
    private readonly string _headerName;
    private readonly string _headerValue;

    public AddHeaderHttpRequestFactoryMiddleware(
        string headerName,
        string headerValue)
    {
        _headerName = headerName;
        _headerValue = headerValue;
    }

    public override Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        requestFactoryContext.Headers.Add(_headerName, _headerValue);
        return base.CreateHttpRequest(requestFactoryContext);
    }
}
