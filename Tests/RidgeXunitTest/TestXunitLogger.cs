using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
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
            var response = await application.ControllerInAreaCaller.Call_Index();
            response.IsSuccessStatusCode.Should().BeTrue();
        }

        public Application CreateApplication()
        {
            var webAppFactory = new WebApplicationFactory<Startup>();

            return new Application(
                webAppFactory,
                _testOutputHelper
            );
        }
    }

    public sealed class Application : IDisposable
    {
        public WebApplicationFactory<Startup> WebApplicationFactory { get; set; }

        public ControllerInAreaCaller ControllerInAreaCaller { get; set; }
        
        public Application(
            WebApplicationFactory<Startup> webApplicationFactory,
            ITestOutputHelper testOutputHelper)
        {
            WebApplicationFactory = webApplicationFactory;
            ControllerInAreaCaller = new ControllerInAreaCaller(
                WebApplicationFactory.CreateClient(),
                WebApplicationFactory.Services,
                new XunitLogWriter(testOutputHelper));
        }

        public void Dispose()
        {
            WebApplicationFactory?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
