using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Threading.Tasks;
using TestWebApplication;
using TestWebApplication.Controllers;
using Xunit;
using Xunit.Abstractions;

namespace RidgeXunitTest;

public class XunitLoggerTests
{
    public XunitLoggerTests(
        ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private readonly ITestOutputHelper _testOutputHelper;

    [Fact]
    public async Task TestXunitLogger()
    {
        using var application = CreateApplication();
        var response = await application.ControllerInAreaCaller.CallIndex();
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    internal Application CreateApplication()
    {
        var webAppFactory = new WebApplicationFactory<Program>().AddXUnitLogger(_testOutputHelper);

        return new Application(
            webAppFactory
        );
    }
}

internal sealed class Application : IDisposable
{
    public WebApplicationFactory<Program> WebApplicationFactory { get; set; }

    public ControllerInAreaCaller ControllerInAreaCaller { get; set; }

    public Application(
        WebApplicationFactory<Program> webApplicationFactory)
    {
        WebApplicationFactory = webApplicationFactory;
        ControllerInAreaCaller = new ControllerInAreaCaller(WebApplicationFactory.CreateRidgeClient());
    }

    public void Dispose()
    {
        WebApplicationFactory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
