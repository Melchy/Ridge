using ApplicationWithDefaultSerialization;
using ApplicationWithDefaultSerialization.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Ridge.AspNetCore;
using Ridge.Extensions.Nunit;
using Ridge.LogWriter;
using System;
using System.Threading.Tasks;

namespace RidgeTests;

public class TestApplicationWithDefaultSerialization
{
    [Test]
    public async Task ArgumentsWithoutAttributesAreSupported()
    {
        using var application = DefaultSerializationApp.CreateApplication();
        var complexObject = new ComplexObject()
        {
            Str = "foo",
            NestedComplexObject = new NestedComplexObject()
            {
                Integer = 1,
                Str = "br",
            },
        };
        var response = await application.ApiClient.ArgumentsWithoutAttributes(complexObject,
            1,
            2);
        response.Result.ComplexObject.Should().BeEquivalentTo(complexObject);
        response.Result.FromQuery.Should().Be(2);
        response.Result.FromRoute.Should().Be(1);
    }
}

internal sealed class DefaultSerializationApp : IDisposable
{
    public WebApplicationFactory<Program> WebApplicationFactory { get; set; }
    public ApiClient ApiClient { get; set; }
    
    internal static DefaultSerializationApp CreateApplication()
    {
        return new DefaultSerializationApp(new WebApplicationFactory<Program>());
    }

    private DefaultSerializationApp(
        WebApplicationFactory<Program> webApplicationFactory)
    {
        WebApplicationFactory = webApplicationFactory.WithRidge(x =>
        {
            x.UseNunitLogWriter();
        });
        ApiClient = new ApiClient(WebApplicationFactory.CreateClient(), WebApplicationFactory.Services);
    }

    public void Dispose()
    {
        WebApplicationFactory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
