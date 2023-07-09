using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Ridge.AspNetCore.GeneratorAttributes;
using Ridge.AspNetCore.Parameters;
using Ridge.HttpRequestFactoryMiddlewares;
using Ridge.Parameters;
using Ridge.Parameters.ActionParams;
using Ridge.Parameters.AdditionalParams;
using Ridge.Parameters.ClientParams;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TestWebApplication;
using TestWebApplication.Controllers;

namespace RidgeTests;

public class ParameterProviderTests
{
    [Test]
    public async Task ParameterProviderActionParametersAreCorrectlySetUp()
    {
        using var application = CreateApplication();
        var testRequestFactoryMiddleware = new ParameterProviderFactoryMiddleware();
        using var applicationFactory = application.WebApplicationFactory.WithRidge(x =>
        {
            x.UseHttpRequestFactoryMiddleware(testRequestFactoryMiddleware);
        });

        var response = await new ControllerWithSpecialGenerationSettingsClient(applicationFactory.CreateClient(), applicationFactory.Services)
           .ActionWithOptionalParameter("test",
                "test",
                new[]
                {
                    "test",
                },
                1,
                "renamedParamter",
                "renamedParamter1",
                optionalWithoutTransformation: "optionalWithoutTransformation",
                addedOptionalParameter: "addedOptionalParameter",
                defaultStruct: DateTime.MinValue);

        testRequestFactoryMiddleware.ParameterProvider!.GetaActionParameters().Should().HaveCount(16);
        testRequestFactoryMiddleware.ParameterProvider!
           .GetaActionParameters()
           .Should()
           .Equal(new[]
                {
                    new ActionParameter(null!, "test2", typeof(string)),
                    new ActionParameter(null!, "floatToBeMadeOptionalString", typeof(float)),
                    new ActionParameter(null!, "test3", typeof(string)),
                    new ActionParameter(null!, "floatToBeMadeOptionalString2", typeof(float)),
                    new ActionParameter(null!, "test", typeof(string[])),
                    new ActionParameter(null!, "optionalWithoutTransformation", typeof(string)),
                    new ActionParameter(null!, "optionalChar", typeof(char)),
                    new ActionParameter(null!, "optionalEnum", typeof(TestEnum)),
                    new ActionParameter(null!, "optionalInt", typeof(int)),
                    new ActionParameter(null!, "optionalDouble", typeof(double)),
                    new ActionParameter(null!, "optionalWithFullDefault", typeof(int)),
                    new ActionParameter(null!, "defaultConst", typeof(int)),
                    new ActionParameter(null!, "defaultStruct", typeof(DateTime)),
                    new ActionParameter(null!, "optional", typeof(int)),
                    new ActionParameter(null!, "fromServices", typeof(object)),
                    new ActionParameter(null!, "fromServices2", typeof(object)),
                },
                (
                    x,
                    y) => x.Name == y.Name && x.Type == y.Type);
    }


    [Test]
    public async Task ParameterProviderClientParametersAreCorrectlySetUp()
    {
        using var application = CreateApplication();
        var testRequestFactoryMiddleware = new ParameterProviderFactoryMiddleware();
        using var applicationFactory = application.WebApplicationFactory.WithRidge(x =>
        {
            x.UseHttpRequestFactoryMiddleware(testRequestFactoryMiddleware);
        });

        var response = await new ControllerWithSpecialGenerationSettingsClient(applicationFactory.CreateClient(), applicationFactory.Services)
           .ActionWithOptionalParameter("test",
                "test",
                new[]
                {
                    "test",
                },
                1,
                "renamedParamter",
                "renamedParamter1",
                optionalWithoutTransformation: "optionalWithoutTransformation",
                addedOptionalParameter: "addedOptionalParameter",
                defaultStruct: DateTime.MinValue);

        testRequestFactoryMiddleware.ParameterProvider!.GetClientParameters().Should().HaveCount(18);
        testRequestFactoryMiddleware.ParameterProvider!
           .GetClientParameters()
           .OrderBy(x => x.Name)
           .Should()
           .BeEquivalentTo(new[]
            {
                new ClientParameter("test2", typeof(string), "test", ParameterMapping.None),
                new ClientParameter("test3", typeof(string), "test", ParameterMapping.None),
                new ClientParameter("test",
                    typeof(string[]),
                    new[]
                    {
                        "test",
                    },
                    ParameterMapping.None),
                new ClientParameter("addedParameter", typeof(int?), 1, ParameterMapping.None),
                new ClientParameter("renamed", typeof(string), "renamedParamter", ParameterMapping.None),
                new ClientParameter("renamed1", typeof(string), "renamedParamter1", ParameterMapping.None),
                new ClientParameter("optionalWithoutTransformation", typeof(string), "optionalWithoutTransformation", ParameterMapping.None),
                new ClientParameter("optionalChar", typeof(char), 'z', ParameterMapping.None),
                new ClientParameter("optionalEnum", typeof(TestEnum), TestEnum.Value, ParameterMapping.None),
                new ClientParameter("optionalInt", typeof(int), 1, ParameterMapping.None),
                new ClientParameter("optionalDouble", typeof(double), 2.3, ParameterMapping.None),
                new ClientParameter("optionalWithFullDefault", typeof(int), 0, ParameterMapping.None),
                new ClientParameter("defaultConst", typeof(int), 1, ParameterMapping.None),
                new ClientParameter("defaultStruct", typeof(DateTime), default(DateTime), ParameterMapping.None),
                new ClientParameter("optional", typeof(int), 0, ParameterMapping.None),
                new ClientParameter("addedOptionalParameter", typeof(string), "addedOptionalParameter", ParameterMapping.None),
                new ClientParameter("addedGenericOptionalParameter", typeof(Task<string>), null, ParameterMapping.None),
                new ClientParameter("renamed2", typeof(object), null, ParameterMapping.None),
            }.OrderBy(x => x.Name));
    }

    [Test]
    public async Task AdditionalParameterProviderIsSetUpCorrectly()
    {
        using var application = CreateApplication();
        var testRequestFactoryMiddleware = new ParameterProviderFactoryMiddleware();
        using var applicationFactory = application.WebApplicationFactory.WithRidge(x =>
        {
            x.UseHttpRequestFactoryMiddleware(testRequestFactoryMiddleware);
        });
        var additionalParameters = new[]
        {
            new AdditionalParameter("optionalParameterString", "OptionalParameter"),
            new AdditionalParameter("optionalParameterInt", 2),
        };
        var response = await new ControllerWithSpecialGenerationSettingsClient(applicationFactory.CreateClient(), applicationFactory.Services)
           .SimpleGet(null,
                additionalParameters: additionalParameters);

        testRequestFactoryMiddleware.ParameterProvider!.GetAdditionalParameters().Should().HaveCount(2);
        testRequestFactoryMiddleware.ParameterProvider!.GetAdditionalParameters()
           .Should()
           .Equal(additionalParameters,
                (
                    x,
                    y) => x.Name == y.Name && x.Value == y.Value);
    }

    internal static Application CreateApplication()
    {
        var webAppFactory = new WebApplicationFactory<Program>();
        return new Application(
            webAppFactory
        );
    }
}

public class ParameterProviderFactoryMiddleware : HttpRequestFactoryMiddleware
{
    public override Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        ParameterProvider = requestFactoryContext.ParameterProvider;
        return base.CreateHttpRequest(requestFactoryContext);
    }

    public ParameterProvider? ParameterProvider { get; private set; }
}
