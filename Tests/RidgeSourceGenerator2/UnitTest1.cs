using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TestWebApplication;
using TestWebApplication.Controllers;
using TestWebApplication.Controllers.Examples;
using WeirdNamespace;

namespace RidgeSourceGenerator2;

public class Tests
{
    [Test]
    public async Task Testt()
    {
        using var application = CreateApplication();
        var a = new ControllerWithNonVirtualMethodsCaller(application.HttpClient, application.WebApplicationFactory.Services);
        var result = await a.Call_ReturnTypeInNestedClass(new ClassInDifferentNamespace.Nested());
    }

    [Test]
    public async Task Testt2()
    {
        using var application = CreateApplication();
        var a = new ExamplesControllerCaller(application.HttpClient, application.WebApplicationFactory.Services);
        a = new ExamplesControllerCaller(application.HttpClient, application.WebApplicationFactory.Services);

        var methodName = "Index";
        var controllerType = typeof(ControllerWithNonVirtualMethods);

        var methodInfo = controllerType.GetMethod(methodName);
        var result = await a.Call_ComplexExample(new ComplexObject()
            {
                Str = "asd",
            },
            new List<string>()
            {
                "asd",
                "asd",
            },
            new List<ComplexObject>()
            {
                new(),
            },
            1,
            null!);
    }

    public static Application CreateApplication()
    {
        var webAppFactory = new WebApplicationFactory<Startup>();
        var client = webAppFactory.CreateClient();
        return new Application(
            webAppFactory,
            client
        );
    }
}

public sealed class Application : IDisposable
{
    public WebApplicationFactory<Startup> WebApplicationFactory { get; set; }
    public HttpClient HttpClient { get; }

    public Application(
        WebApplicationFactory<Startup> webApplicationFactory,
        HttpClient httpClient)
    {
        WebApplicationFactory = webApplicationFactory;
        HttpClient = httpClient;
    }

    public void Dispose()
    {
        WebApplicationFactory?.Dispose();
        GC.SuppressFinalize(this);
    }
}

public class Test
{
    public async Task ASd()
    {
    }
}
