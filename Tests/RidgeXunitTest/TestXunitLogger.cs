using Microsoft.AspNetCore.Mvc.Testing;
using System;
using TestWebApplication;
using Xunit.Abstractions;

namespace RidgeXunitTest
{
    public class XunitLoggerTests
    {
        public XunitLoggerTests(
            ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        private readonly ITestOutputHelper _testOutputHelper;

        // TODO dodelat
        // [Fact]
        // public async Task TestXunitLogger()
        // {
        //     using var application = CreateApplication();
        //     var testController = application.ControllerFactory.CreateController<ControllerInArea>();
        //     var result = await testController.Index();
        //     result.IsSuccessStatusCode().Should().BeTrue();
        // }
        //
        // public Application CreateApplication()
        // {
        //     var webAppFactory = new WebApplicationFactory<Startup>();
        //     var client = webAppFactory.CreateClient();
        //
        //     return new Application(
        //         webAppFactory,
        //         new ControllerFactory(client, webAppFactory.Services, new XunitLogWriter(_testOutputHelper))
        //     );
        // }
    }

    public sealed class Application : IDisposable
    {
        public WebApplicationFactory<Startup> WebApplicationFactory { get; set; }

        public Application(
            WebApplicationFactory<Startup> webApplicationFactory)
        {
            WebApplicationFactory = webApplicationFactory;
        }

        public void Dispose()
        {
            WebApplicationFactory?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
