using ApplicationWithDefaultSerialization.Controllers;
using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace RidgeTests;

public class EndpointsTests
{
    [Test]
    public async Task TestSingleEndpointTransfomration()
    {
        using var application = DefaultSerializationApp.CreateApplication();
        var response = await application.ApiClient.TransformEndpointAndMethodName();
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task TestSingleEndpointNoTransformation()
    {
        using var application = DefaultSerializationApp.CreateApplication();
        var response = await application.ApiClient.SomeRandomMethod();
        response.IsSuccessStatusCode.Should().BeTrue();
    }
}
