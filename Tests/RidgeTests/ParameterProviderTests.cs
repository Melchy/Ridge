using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Ridge.GeneratorAttributes;
using Ridge.HttpRequestFactoryMiddlewares;
using Ridge.Parameters;
using Ridge.Parameters.ActionParams;
using Ridge.Parameters.CallerParams;
using Ridge.Parameters.CustomParams;
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
        var applicationFactory = application.WebApplicationFactory.AddHttpRequestFactoryMiddleware(testRequestFactoryMiddleware);
        var response = await new ControllerWithSpecialGenerationSettingsCaller(applicationFactory.CreateRidgeClient())
           .CallActionWithOptionalParameter("test",
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
    public async Task ParameterProviderCallerParametersAreCorrectlySetUp()
    {
        using var application = CreateApplication();
        var testRequestFactoryMiddleware = new ParameterProviderFactoryMiddleware();
        var applicationFactory = application.WebApplicationFactory.AddHttpRequestFactoryMiddleware(testRequestFactoryMiddleware);
        applicationFactory.AddHttpRequestFactoryMiddleware(testRequestFactoryMiddleware);
        var response = await new ControllerWithSpecialGenerationSettingsCaller(applicationFactory.CreateRidgeClient())
           .CallActionWithOptionalParameter("test",
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

        testRequestFactoryMiddleware.ParameterProvider!.GetCallerParameters().Should().HaveCount(18);
        testRequestFactoryMiddleware.ParameterProvider!
           .GetCallerParameters()
           .OrderBy(x => x.Name)
           .Should()
           .BeEquivalentTo(new[]
            {
                new CallerParameter("test2", typeof(string), "test", ParameterMapping.None),
                new CallerParameter("test3", typeof(string), "test", ParameterMapping.None),
                new CallerParameter("test",
                    typeof(string[]),
                    new[]
                    {
                        "test",
                    },
                    ParameterMapping.None),
                new CallerParameter("addedParameter", typeof(int?), 1, ParameterMapping.None),
                new CallerParameter("renamed", typeof(string), "renamedParamter", ParameterMapping.None),
                new CallerParameter("renamed1", typeof(string), "renamedParamter1", ParameterMapping.None),
                new CallerParameter("optionalWithoutTransformation", typeof(string), "optionalWithoutTransformation", ParameterMapping.None),
                new CallerParameter("optionalChar", typeof(char), 'z', ParameterMapping.None),
                new CallerParameter("optionalEnum", typeof(TestEnum), TestEnum.Value, ParameterMapping.None),
                new CallerParameter("optionalInt", typeof(int), 1, ParameterMapping.None),
                new CallerParameter("optionalDouble", typeof(double), 2.3, ParameterMapping.None),
                new CallerParameter("optionalWithFullDefault", typeof(int), 0, ParameterMapping.None),
                new CallerParameter("defaultConst", typeof(int), 1, ParameterMapping.None),
                new CallerParameter("defaultStruct", typeof(DateTime), default(DateTime), ParameterMapping.None),
                new CallerParameter("optional", typeof(int), 0, ParameterMapping.None),
                new CallerParameter("addedOptionalParameter", typeof(string), "addedOptionalParameter", ParameterMapping.None),
                new CallerParameter("addedGenericOptionalParameter", typeof(Task<string>), null, ParameterMapping.None),
                new CallerParameter("renamed2", typeof(object), null, ParameterMapping.None),
            }.OrderBy(x => x.Name));
    }

    [Test]
    public async Task CustomParameterProviderIsSetUpCorrectly()
    {
        using var application = CreateApplication();
        var testRequestFactoryMiddleware = new ParameterProviderFactoryMiddleware();
        var applicationFactory = application.WebApplicationFactory.AddHttpRequestFactoryMiddleware(testRequestFactoryMiddleware);
        applicationFactory.AddHttpRequestFactoryMiddleware(testRequestFactoryMiddleware);
        var customParameters = new[]
        {
            new CustomParameter("optionalParameterString", "OptionalParameter"),
            new CustomParameter("optionalParameterInt", 2),
        };
        var response = await new ControllerWithSpecialGenerationSettingsCaller(applicationFactory.CreateRidgeClient())
           .CallSimpleGet(null,
                customParameters: customParameters);

        testRequestFactoryMiddleware.ParameterProvider!.GetCustomParameters().Should().HaveCount(2);
        testRequestFactoryMiddleware.ParameterProvider!.GetCustomParameters()
           .Should()
           .Equal(customParameters,
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
