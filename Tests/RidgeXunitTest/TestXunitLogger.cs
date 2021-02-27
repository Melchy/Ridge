using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Ridge.Interceptor.InterceptorFactory;
using Ridge.LogWriter;
using System;
using System.Threading.Tasks;
using TestWebApplication;
using TestWebApplication.Controllers;
using Xunit;
using Xunit.Abstractions;

namespace RidgeXunitTest
{
    public class XunitLoggerTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public XunitLoggerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task TestXunitLogger()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<ControllerInArea>();
            var result = await testController.Index();
            result.IsSuccessStatusCode.Should().BeTrue();
        }

        public Application CreateApplication()
        {
            var webAppFactory = new WebApplicationFactory<Startup>();
            var client = webAppFactory.CreateClient();

            return new Application
            (
                webAppFactory,
                new ControllerFactory(client, webAppFactory.Services, new XunitLogWriter(_testOutputHelper))
            );
        }
    }

    public sealed class Application : IDisposable
    {
        public Application(
            WebApplicationFactory<Startup> webApplicationFactory,
            ControllerFactory controllerFactory)
        {
            WebApplicationFactory = webApplicationFactory;
            ControllerFactory = controllerFactory;
        }

        public WebApplicationFactory<Startup> WebApplicationFactory { get; set; }
        public ControllerFactory ControllerFactory { get; set; }

        public void Dispose()
        {
            WebApplicationFactory?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
