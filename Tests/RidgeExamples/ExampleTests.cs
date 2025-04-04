﻿using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Ridge.AspNetCore.Parameters;
using Ridge.HttpRequestFactoryMiddlewares;
using Ridge.Parameters.AdditionalParams;
using System.Net.Http;
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
        // Client generated by ridge
        var examplesControllerClient = new ExamplesControllerClient(client, webApplicationFactory.Services); 

        var response = await examplesControllerClient.ReturnGivenNumber(10);
    
        Assert.True(response.IsSuccessStatusCode);
        Assert.AreEqual(10, response.Result); // Response is wrapped in HttpCallResponse<int>
    }

    // [Test]
    // public async Task ThrowExceptionTestWith500()
    // {
    //     using var webApplicationFactory = new WebApplicationFactory<Program>().WithRidge();
    //     var httpClient = webApplicationFactory.CreateClient();
    //     var examplesControllerClient = new ExamplesControllerClient(httpClient, webApplicationFactory.Services);
    //     
    //     var response = await examplesControllerClient.ActionWithError();
    //     
    //     Assert.True(response.IsSuccessStatusCode);
    // }
    
    
    [Test]
    public async Task ThrowExceptionTest()
    {
        using var webApplicationFactory = new WebApplicationFactory<Program>().WithRidge();
        var httpClient = webApplicationFactory.CreateClient();
        var examplesControllerClient = new ExamplesControllerClient(httpClient, webApplicationFactory.Services);

        var response = await examplesControllerClient.ActionWithError();

        Assert.True(response.IsSuccessStatusCode);
    }


    [Test]
    public async Task HttpRequestFactoryExample()
    {
        using var webApplicationFactory = new WebApplicationFactory<Program>().WithRidge(x =>
        {
            x.UseHttpRequestFactoryMiddleware(new AddEnglishLanguageHeader());
        });
        var ridgeHttpClient = webApplicationFactory.CreateClient();
        var examplesControllerClient = new ExamplesControllerClient(ridgeHttpClient, webApplicationFactory.Services);

        // endpoint which returns Accept-Language header 
        var response = await examplesControllerClient.ReturnLanguage();
        Assert.AreEqual("en-US", response.Result);
    }

    [Test]
    public async Task CustomModelBinderTest()
    {
        using var webApplicationFactory = new WebApplicationFactory<Program>().WithRidge();
        var ridgeHttpClient = webApplicationFactory.CreateClient();
        var examplesControllerClient = new ExamplesControllerClient(ridgeHttpClient, webApplicationFactory.Services);

        var response = await examplesControllerClient.ReturnCountryCode("cs-CZ");
        Assert.AreEqual("cs-CZ", response.Result);
    }
    
    [Test]
    public async Task ParameterAddedByRidge()
    {
        using var webApplicationFactory = new WebApplicationFactory<Program>().WithRidge();

        var httpClient = webApplicationFactory.CreateClient();
        var examplesControllerCaller = new ExamplesControllerClient(httpClient, webApplicationFactory.Services);
        
        // controller finds header by it's name and returns it's value
        var response = await examplesControllerCaller.ReadQueryParameterFromHttpContext(exampleParameter: "value");
        Assert.AreEqual("value", response.Result);
    }

    [Test]
    public async Task AdditionalParameter()
    {
        using var webApplicationFactory = new WebApplicationFactory<Program>().WithRidge(x =>
        {
            x.UseHttpRequestFactoryMiddleware(new SetAgeFromAdditionalParameter());
        });
        var ridgeHttpClient = webApplicationFactory.CreateClient();
        var examplesControllerClient = new ExamplesControllerClient(ridgeHttpClient, webApplicationFactory.Services);

        var response = await examplesControllerClient.ReadAgeFromHttpContext(additionalParameters: new AdditionalParameter("age", 10));

        Assert.AreEqual(10, response.Result);
    }

    [Test]
    public async Task PredefinedAdditionalParameters()
    {
        using var webApplicationFactory = new WebApplicationFactory<Program>().WithRidge();
        var ridgeHttpClient = webApplicationFactory.CreateClient();
        var examplesControllerClient = new ExamplesControllerClient(ridgeHttpClient, webApplicationFactory.Services);

        var response = await examplesControllerClient.ReadAgeFromHttpContext(additionalParameters: new QueryOrRouteParameter("age", 10));

        Assert.AreEqual(10, response.Result);
    }
}

public class SetAgeFromAdditionalParameter : HttpRequestFactoryMiddleware
{
    public override Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        var ageParameter = requestFactoryContext.ParameterProvider
           .GetAdditionalParameters()
           .GetParameterByNameOrThrow("age")
           .GetValueOrThrow<int>();


        requestFactoryContext.UrlGenerationParameters["age"] = ageParameter.ToString();
        
        return base.CreateHttpRequest(requestFactoryContext);
    }
}

public class AddEnglishLanguageHeader : HttpRequestFactoryMiddleware
{
    public override Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        requestFactoryContext.Headers.Add("Accept-Language", "en-US");
        return base.CreateHttpRequest(requestFactoryContext);
    }
}
