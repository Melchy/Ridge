using FluentAssertions;
using Ridge;
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
        var webAppFactory = new RidgeApplicationFactory<Program>().AddXUnitLogger(_testOutputHelper);

        return new Application(
            webAppFactory
        );
    }
}

internal sealed class Application : IDisposable
{
    public RidgeApplicationFactory<Program> RidgeApplicationFactory { get; set; }

    public ControllerInAreaCaller ControllerInAreaCaller { get; set; }

    public Application(
        RidgeApplicationFactory<Program> ridgeApplicationFactory)
    {
        RidgeApplicationFactory = ridgeApplicationFactory;
        ControllerInAreaCaller = new ControllerInAreaCaller(RidgeApplicationFactory.CreateRidgeClient());
    }

    public void Dispose()
    {
        RidgeApplicationFactory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
