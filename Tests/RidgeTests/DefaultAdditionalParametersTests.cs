using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Ridge.Parameters.AdditionalParams;
using System.Linq;
using System.Threading.Tasks;
using TestWebApplication;

namespace RidgeTests;

public class DefaultAdditionalParametersTests
{
    [Test]
    public async Task HeaderParametersAreCorrectlyMapped()
    {
        using var application = CreateApplication();

        var response = await application.TestControllerClient.MethodReturningHeaders(
            new HttpHeaderParameter("key", "value"),
            new HttpHeaderParameter("key2", "value2"));

        response.Result.Should().ContainKey("key").WhoseValue.First().Should().Be("value");
        response.Result.Should().ContainKey("key2").WhoseValue.First().Should().Be("value2");
    }

    [Test]
    public async Task BodyParameterIsCorrectlyMapped()
    {
        using var application = CreateApplication();

        var response = await application.TestControllerClient.MethodReturningBody(new BodyParameter("test"));

        // aditional " are added because json body contains " when sending string
        response.Result.Should().Be("\"test\"");
    }

    [Test]
    public async Task RouteAndQueryParametersAreCorrectlyMapped()
    {
        using var application = CreateApplication();

        var response = await application.TestControllerClient.RouteAndQueryParameters(
            new QueryOrRouteParameter("queryParameter", "query"),
            new QueryOrRouteParameter("routeParameter", "route"));

        // aditional " are added because json body contains " when sending string
        response.Result.queryParameter.Should().Be("query");
        response.Result.routeParameter.Should().Be("route");
    }

    internal static Application CreateApplication()
    {
        var webAppFactory = new WebApplicationFactory<Program>();
        return new Application(
            webAppFactory
        );
    }
}
