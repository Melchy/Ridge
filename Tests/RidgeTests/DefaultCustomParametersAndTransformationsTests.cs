using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
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
        var response = await application.AddedParametersWithDefaultMappingControllerClient.DefaultAction("routeParameter",
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
        var response = await application.TransformedParametersWithDefaultMappingControllerClient.DefaultAction(
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
