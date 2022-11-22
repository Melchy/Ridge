using ApplicationWithDefaultSerialization;
using ApplicationWithDefaultSerialization.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Ridge.LogWriter;
using System;
using System.Threading.Tasks;

namespace RidgeTests;

public class TestApplicationWithDefaultSerialization
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
        var response = await application.TestControllerCaller.CallArgumentsWithoutAttributes(complexObject,
            1,
            2);
        response.Result.ComplexObject.Should().BeEquivalentTo(complexObject);
        response.Result.FromQuery.Should().Be(2);
        response.Result.FromRoute.Should().Be(1);
    }

    internal static Application CreateApplication()
    {
        var webAppFactory = new WebApplicationFactory<Program>();
        return new Application(webAppFactory);
    }

    internal sealed class Application : IDisposable
    {
        public WebApplicationFactory<Program> WebApplicationFactory { get; set; }
        public TestControllerCaller<Program> TestControllerCaller { get; set; }

        public Application(
            WebApplicationFactory<Program> webApplicationFactory)
        {
            WebApplicationFactory = webApplicationFactory;
            TestControllerCaller = new TestControllerCaller<Program>(
                WebApplicationFactory,
                new NunitLogWriter());
        }

        public void Dispose()
        {
            WebApplicationFactory?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
