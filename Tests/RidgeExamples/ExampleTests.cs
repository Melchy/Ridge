using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Ridge.HttpRequestFactoryMiddlewares;
using Ridge.Parameters.CustomParams;
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
    // Test file
    [Test]
    public async Task CallControllerUsingRidge()
    {
        // Create WebApplicationFactory - https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-5.0#basic-tests-with-the-default-webapplicationfactory
        using var webApplicationFactory = new WebApplicationFactory<Program>();
        // create http client for ridge caller
        var client = webApplicationFactory.AddExceptionCatching().CreateRidgeClient();
        var examplesControllerCaller = new ExamplesControllerCaller(client);

        // Ridge wraps the HttpResponseMessage in a convenient wrapper class
        var response = await examplesControllerCaller.CallReturnGivenNumber(10);
        Assert.True(response.IsSuccessStatusCode);
        Assert.AreEqual(10, response.Result);

        // Access the http response directly
        Assert.True(response.HttpResponseMessage.IsSuccessStatusCode);
    }

    // Equivalent code without using ridge 
    [Test]
    public async Task CallControllerWithoutRidge()
    {
        // Create WebApplicationFactory - https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-5.0#basic-tests-with-the-default-webapplicationfactory
        using var webApplicationFactory = new WebApplicationFactory<Program>();
        // create http client for ridge caller
        var client = webApplicationFactory.CreateClient();

        var response = await client.GetAsync("/returnGivenNumber/?input=10");
        Assert.True(response.IsSuccessStatusCode);
        var responseAsString = await response.Content.ReadAsStringAsync();
        Assert.AreEqual(10, JsonSerializer.Deserialize<int>(responseAsString));
    }


    [Test]
    public async Task ThrowExceptionTest()
    {
        using var webApplicationFactory = new WebApplicationFactory<Program>();
        // notice use of AddExceptionCatching()
        var ridgeHttpClient = webApplicationFactory.AddExceptionCatching().AddExceptionCatching().CreateRidgeClient();
        var examplesControllerCaller = new ExamplesControllerCaller(ridgeHttpClient);

        try
        {
            _ = await examplesControllerCaller.CallThrowException();
        }
        catch (InvalidOperationException e)
        {
            Assert.AreEqual("Exception throw", e.Message);
        }
    }


    [Test]
    public async Task HttpRequestFactoryExample()
    {
        using var webApplicationFactory = new WebApplicationFactory<Program>()
           .AddExceptionCatching()
           .AddHttpRequestFactoryMiddleware(new AddHeaderHttpRequestFactoryMiddleware("exampleHeader", "exampleHeaderValue"));
        var ridgeHttpClient = webApplicationFactory.CreateRidgeClient();
        var examplesControllerCaller = new ExamplesControllerCaller(ridgeHttpClient);

        // controller finds header by it's name and returns it's value
        var response = await examplesControllerCaller.CallReturnHeader(headerName: "exampleHeader");
        Assert.AreEqual("exampleHeaderValue", response.Result);
    }

    [Test]
    public async Task AddHeaderSimple()
    {
        using var webApplicationFactory = new WebApplicationFactory<Program>()
           .AddExceptionCatching()
           .AddHeader(new HttpHeaderParameter("exampleHeader", "exampleHeaderValue"));
        var ridgeHttpClient = webApplicationFactory.CreateRidgeClient();
        var examplesControllerCaller = new ExamplesControllerCaller(ridgeHttpClient);

        // controller finds header by it's name and returns it's value
        var response = await examplesControllerCaller.CallReturnHeader(headerName: "exampleHeader");
        Assert.AreEqual("exampleHeaderValue", response.Result);
    }

    [Test]
    public async Task ParameterAddedByRidge()
    {
        using var webApplicationFactory = new WebApplicationFactory<Program>().AddExceptionCatching();

        var ridgeHttpClient = webApplicationFactory.CreateRidgeClient();
        var examplesControllerCaller = new ExamplesControllerCaller(ridgeHttpClient);

        // controller finds header by it's name and returns it's value
        var response = await examplesControllerCaller.CallReadQueryParameterFromHttpContext(GeneratedParameter: "queryParameterValue");
        Assert.AreEqual("queryParameterValue", response.Result);
    }

    [Test]
    public async Task CustomModelBinderTest()
    {
        using var webApplicationFactory = new WebApplicationFactory<Program>().AddExceptionCatching();

        var ridgeHttpClient = webApplicationFactory.CreateRidgeClient();
        var examplesControllerCaller = new ExamplesControllerCaller(ridgeHttpClient);

        // controller finds header by it's name and returns it's value
        var response = await examplesControllerCaller.CallWithCustomModelBinder("cs-CZ");
        Assert.AreEqual("cs-CZ", response.Result);
    }

    [Test]
    public async Task CustomParameter()
    {
        using var webApplicationFactory = new WebApplicationFactory<Program>()
           .AddHttpRequestFactoryMiddleware(new AddHeaderFromCustomParameters())
           .AddExceptionCatching();

        var ridgeHttpClient = webApplicationFactory.CreateRidgeClient();
        var examplesControllerCaller = new ExamplesControllerCaller(ridgeHttpClient);

        // action returns all passed headers
        var response = await examplesControllerCaller.CallReturnAllHeaders(customParameters: new CustomParameter("exampleHeader", "exampleHeaderValue"));

        Assert.AreEqual("exampleHeaderValue", response.Result.First(x => x.key == "exampleHeader").value);
    }
}

public class AddHeaderFromCustomParameters : HttpRequestFactoryMiddleware
{
    public override Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        var customParameters = requestFactoryContext.ParameterProvider.GetCustomParameters();

        foreach (var customParameter in customParameters)
        {
            requestFactoryContext.Headers.Add(customParameter.Name, customParameter.Value?.ToString());
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
           .GetCallerParameters()
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
           .GetCallerParameters()
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
