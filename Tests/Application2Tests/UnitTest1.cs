using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Ridge.CallResult.Controller.Extensions;
using Ridge.Interceptor.InterceptorFactory;
using Ridge.LogWriter;
using System;
using System.Threading.Tasks;
using TestWebAplication2;
using TestWebAplication2.Controllers;

namespace Application2Tests
{
    public class Tests
    {
        [Test]
        public async Task ArgumentsWithoutAttributesAreSupported()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var complexObject = new ComplexObject()
            {
                Str = "foo",
                NestedComplexObject = new NestedComplexObject()
                {
                    Integer = 1,
                    Str = "br",
                },
            };
            var response = await testController.ArgumentsWithoutAttributes(complexObject,
                1,
                2);
            response.GetResult().ComplexObject.Should().BeEquivalentTo(complexObject);
            response.GetResult().FromQuery.Should().Be(2);
            response.GetResult().FromRoute.Should().Be(1);
        }

        public static Application CreateApplication()
        {
            var webAppFactory = new WebApplicationFactory<Startup>();
            var client = webAppFactory.CreateClient();
            return new Application(
                webAppFactory,
                new ControllerFactory(client, webAppFactory.Services, new NunitLogWriter())
            );
        }

        public sealed class Application : IDisposable
        {
            public WebApplicationFactory<Startup> WebApplicationFactory { get; set; }
            public ControllerFactory ControllerFactory { get; set; }

            public Application(
                WebApplicationFactory<Startup> webApplicationFactory,
                ControllerFactory controllerFactory)
            {
                WebApplicationFactory = webApplicationFactory;
                ControllerFactory = controllerFactory;
            }

            public void Dispose()
            {
                WebApplicationFactory?.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}
