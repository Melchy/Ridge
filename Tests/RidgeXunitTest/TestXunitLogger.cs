using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Ridge;
using Ridge.LogWriter;
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
        var webAppFactory = new WebApplicationFactory<Program>();

        return new Application(
            webAppFactory,
            _testOutputHelper
        );
    }
}

internal sealed class Application : IDisposable
{
    public WebApplicationFactory<Program> WebApplicationFactory { get; set; }

    public ControllerInAreaCaller<Program> ControllerInAreaCaller { get; set; }

    public ApplicationCaller<Program> ApplicationCaller { get; set; }
    
    public Application(
        WebApplicationFactory<Program> webApplicationFactory,
        ITestOutputHelper testOutputHelper)
    {
        WebApplicationFactory = webApplicationFactory;
        ApplicationCaller = new ApplicationCaller<Program>(WebApplicationFactory, new XunitLogWriter(testOutputHelper));
        ControllerInAreaCaller = new ControllerInAreaCaller<Program>(ApplicationCaller);
    }

    public void Dispose()
    {
        WebApplicationFactory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
