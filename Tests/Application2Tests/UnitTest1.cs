using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
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
            var complexObject = new ComplexObject()
            {
                Str = "foo",
                NestedComplexObject = new NestedComplexObject()
                {
                    Integer = 1,
                    Str = "br",
                },
            };
            var response = await application.TestControllerCaller.Call_ArgumentsWithoutAttributes(complexObject,
                1,
                2);
            response.Result.ComplexObject.Should().BeEquivalentTo(complexObject);
            response.Result.FromQuery.Should().Be(2);
            response.Result.FromRoute.Should().Be(1);
        }

        internal static Application CreateApplication()
        {
            var webAppFactory = new WebApplicationFactory<Startup>();
            return new Application(webAppFactory);
        }

        internal sealed class Application : IDisposable
        {
            public WebApplicationFactory<Startup> WebApplicationFactory { get; set; }
            public TestControllerCaller TestControllerCaller { get; set; }

            public Application(
                WebApplicationFactory<Startup> webApplicationFactory)
            {
                WebApplicationFactory = webApplicationFactory;
                TestControllerCaller = new TestControllerCaller(
                    WebApplicationFactory.CreateClient(),
                    WebApplicationFactory.Services,
                    new NunitProgressLogWriter());
            }

            public void Dispose()
            {
                WebApplicationFactory?.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}
