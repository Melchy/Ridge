using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Ridge.Parameters.CustomParams;
using System.Linq;
using System.Threading.Tasks;
using TestWebApplication;
using TestWebApplication.Controllers;

namespace RidgeTests;

public class ParametersAddedOrTransformedAutoMappingTests
{
    [Test]
    public async Task TestAddedParameters()
    {
        using var application = CreateApplication();
        var response = await application.AddedParametersWithDefaultMappingControllerCaller.CallDefaultAction("routeParameter",
            "queryParameter",
            new ComplexObject()
            {
                Str = "body",
            },
            "headerParameter");

        response.Result.fromRouteParameter.Should().Be("routeParameter");
        response.Result.fromQueryParameter.Should().Be("queryParameter");
        response.Result.body.Str.Should().Be("body");
        response.Result.fromHeaderParameter.Should().Be("headerParameter");
    }

    [Test]
    public async Task TestTransformedParameters()
    {
        using var application = CreateApplication();
        var response = await application.TransformedParametersWithDefaultMappingControllerCaller.CallDefaultAction(
            TransformedParametersWithDefaultMappingController.TestEnum.None,
            "route",
            10,
            2.3);

        response.Result.fromRouteParameter.Should().Be("route");
        response.Result.fromQueryParameter.Should().Be(TransformedParametersWithDefaultMappingController.TestEnum.None);
        response.Result.body.Should().Be(2.3);
        response.Result.fromHeaderParameter.Should().Be(10);
    }

    internal static Application CreateApplication()
    {
        var webAppFactory = new WebApplicationFactory<Program>();
        return new Application(
            webAppFactory
        );
    }
}

public class DefaultCustomParametersTests
{
    [Test]
    public async Task HeaderParametersAreCorrectlyMapped()
    {
        using var application = CreateApplication();

        var response = await application.TestControllerCaller.CallMethodReturningHeaders(
            new HttpHeaderParameter("key", "value"),
            new HttpHeaderParameter("key2", "value2"));

        response.Result.Should().ContainKey("key").WhoseValue.First().Should().Be("value");
        response.Result.Should().ContainKey("key2").WhoseValue.First().Should().Be("value2");
    }

    [Test]
    public async Task BodyParameterIsCorrectlyMapped()
    {
        using var application = CreateApplication();

        var response = await application.TestControllerCaller.CallMethodReturningBody(new BodyParameter("test"));

        // aditional " are added because json body contains " when sending string
        response.Result.Should().Be("\"test\"");
    }

    [Test]
    public async Task RouteAndQueryParametersAreCorrectlyMapped()
    {
        using var application = CreateApplication();

        var response = await application.TestControllerCaller.CallRouteAndQueryParameters(
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
