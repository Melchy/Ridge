using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Ridge.AspNetCore.Parameters;
using Ridge.AspNetCore.Response;
using Ridge.HttpRequestFactoryMiddlewares;
using Ridge.Parameters;
using Ridge.Parameters.AdditionalParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TestWebApplication;
using TestWebApplication.Controllers;

namespace RidgeTests;

public class RidgeTests
{
    [Test]
    public async Task SyncCallWithoutResult()
    {
        using var application = CreateApplication();
        var response = await application.TestControllerClient.ReturnSync();
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task SyncCallWithResult()
    {
        using var application = CreateApplication();
        var response = await application.TestControllerClient.ReturnSyncWithResult();
        response.IsSuccessStatusCode.Should().BeTrue();
        response.Result.Should().Be("ok");
    }

    [Test]
    public async Task SyncCallThrowingNotWrappedException()
    {
        using var application = CreateApplication();
        var result = async () => await application.TestControllerClient.SyncThrow();
        await result.Should().ThrowAsync<InvalidOperationException>().WithMessage("Error");
    }

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
        var response = await application.TestControllerClient.ArgumentsWithoutAttributes(complexObject,
            1,
            2);
        response.Result.ComplexObject.Should().BeEquivalentTo(complexObject);
        response.Result.FromQuery.Should().Be(2);
        response.Result.FromRoute.Should().Be(1);
    }

    [Test]
    public async Task AsyncCallWithResult()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.ReturnAsync();
        result.Result.Should().Be(10);
    }

    [Test]
    public async Task AsyncCallWithoutResult()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.BadRequestAsync();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task AreasAreSupported()
    {
        using var application = CreateApplication();
        var result = await application.ControllerInAreaClient.Index();
        result.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task MethodOverloadingIsSupported()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.OverloadedAction();
        result.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task MethodOverloadingIsSupported2()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.OverloadedAction(1);
        result.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task SimpleArgumentsAreMapped()
    {
        using var application = CreateApplication();
        var response =
            await application.TestControllerClient.SimpleArguments(1,
                DateTime.Today,
                TestController.TestEnum.Zero,
                100,
                DateTime.Today);
        response.Result.FromRoute.Should().Be(1);
        response.Result.Body.Should().Be(DateTime.Today);
        response.Result.FromQuery.Should().Be(TestController.TestEnum.Zero);
        response.Result.FromRoute2.Should().Be(100);
        response.Result.FromQuery2.Should().Be(DateTime.Today);
    }

    [Test]
    public async Task BodyCanContainComplexObject()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.ComplexBody(new TestController.ComplexArgument(
            integer: 10,
            str: "test",
            dateTime: DateTime.Today,
            innerObject: new TestController.InnerObject(str: "InnerStr")
        ));
        result.Result.Integer.Should().Be(10);
        result.Result.Str.Should().Be("test");
        result.Result.DateTime.Should().Be(DateTime.Today);
        result.Result.InnerObject!.Str.Should().Be("InnerStr");
    }

    [Test]
    public async Task BodySetToNull()
    {
        using var application = CreateApplication();
        var response = await application.TestControllerClient.ComplexBody(null!);
        response.Result.Should().BeNull();
    }

    [Test]
    public async Task FromQueryCanContainComplexObject()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.ComplexFromQuery(new TestController.ComplexArgument(
            integer: 10,
            str: "test",
            dateTime: DateTime.Today,
            innerObject: new TestController.InnerObject("test")
            {
                List = new List<string>() {"a", "b"},
            }
        ));
        result.Result.Integer.Should().Be(10);
        result.Result.Str.Should().Be("test");
        result.Result.DateTime.Should().Be(DateTime.Today);
        result.Result.InnerObject!.Str.Should().Be("test");
        result.Result.InnerObject!.List.Should().ContainInOrder("a", "b");
    }

    [Test]
    public async Task FromHeaderSupportsArrays()
    {
        using var application = CreateApplication();
        var response = await application.TestControllerClient.FromHeaderWithArray(new[] {1, 2});
        response.Result.header1.Should().Be(1);
        response.Result.header2.Should().Be(2);
    }

    [Test]
    public async Task FromHeaderIsSupportedForSimpleArguments()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.FromHeaderSimple(1);
        result.Result.Should().Be(1);
    }

    [Test]
    public async Task HttpPatchWithBody()
    {
        using var application = CreateApplication();
        var response = await application.TestControllerClient.PatchWithBody(new ComplexObject() {Str = "test"});
        response.Result.Str.Should().Be("test");
    }
    
    [Test]
    public async Task NameInFromQueryAttributeIsSupportedForComplexArgument()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.FromQueryWithNameComplexArgument(new TestController.Test() {Foo = 1});
        result.Result.Foo.Should().Be(1);
    }

    [Test]
    public async Task NameInFromQueryAttributeIsSupportedSimpleArgument()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.FromQueryWithNameSimpleArgument(1);
        result.Result.Should().Be(1);
    }

    // This works in test but it would not work in real application
    // Test adds default values bud real app does not
    [Test]
    public async Task ObjectWithDefaultValuesInCtorDoesNotWorkWhenBindingUsingJsonNet()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.DefaultPropertiesInCtorTest(new ObjectWithDefaultProperties());
        result.Result.Str.Should().Be("test");
    }

    [Test]
    public async Task NullsCanBePlacedInFromQueryOrFromBodyOrFromHead()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.NullsTest(null, null, null, "asd");
        result.Result.Item1.Should().Be(null);
        result.Result.Item2.Should().Be(null);
        result.Result.Item3.Should().Be(null);
    }

    [Test]
    public async Task ArrayInFromQueryIsSupported()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.ArrayInFromQuery(new List<int>() {1, 1, 1});
        result.Result.Should().AllBeEquivalentTo(1);
    }

    [Test]
    public async Task NullInFromRouteCausesNotFound()
    {
        using var application = CreateApplication();
        var response = await application.TestControllerClient.NullsTest(1, new TestController.ComplexArgument(), DateTime.Now, null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task NameInFromRouteAttributeIsSupportedSimpleArgument()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.FromRouteWithNameSimpleArgument(1);
        result.Result.Should().Be(1);
    }

    [Test]
    public async Task ClassicalRoutingIsSupported()
    {
        using var application = CreateApplication();
        var result = await application.ControllerWithoutAttributeRoutingClient.HttpGetWithoutBody();
        result.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task FromServicesIsIgnored()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.FromServices();
        result.Result.Should().BeTrue();
    }

    [Test]
    public async Task ArrayInBodyIsSupported()
    {
        using var application = CreateApplication();
        var data = new List<TestController.ComplexArgument>
        {
            new(
                integer: 10,
                str: "test",
                dateTime: DateTime.Today
            ),
            new(
                integer: 100,
                str: "testt",
                dateTime: DateTime.Today
            ),
        };
        var result = await application.TestControllerClient.ArrayInBody(data);
        result.Result
           .Should()
           .SatisfyRespectively(x =>
                {
                    x.Integer.Should().Be(10);
                    x.Str.Should().Be("test");
                    x.DateTime.Should().Be(DateTime.Today);
                },
                x =>
                {
                    x.Integer.Should().Be(100);
                    x.Str.Should().Be("testt");
                    x.DateTime.Should().Be(DateTime.Today);
                });
    }


    [Test]
    public async Task HeadersCanBeAlteredUsingBuilder()
    {
        using var application = CreateApplication();

        var result = await application.TestControllerClient.MethodReturningHeaders(
            new HttpHeaderParameter("foo", "foo"),
            new HttpHeaderParameter("header1", "header1"),
            new HttpHeaderParameter("header2", "header2"));

        result.Result["foo"].First().Should().Be("foo");
        result.Result["header1"].First().Should().Be("header1");
        result.Result["header2"].First().Should().Be("header2");
    }

    [Test]
    public async Task RequestCanBeAlteredUsingGlobalBuilder()
    {
        using var application = CreateApplication();
        var client = application.WebApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add("foo", "foo");
        client.DefaultRequestHeaders.Add("header1", "header1");
        client.DefaultRequestHeaders.Add("header2", "header2");

        var response = await new TestControllerClient(client, application.WebApplicationFactory.Services).MethodReturningHeaders();
        response.Result["foo"].First().Should().Be("foo");
        response.Result["header1"].First().Should().Be("header1");
        response.Result["header2"].First().Should().Be("header2");
    }


    [Test]
    public async Task CallWithTwoSameFromHeaderAttributesValid()
    {
        using var application = CreateApplication();

        var result = await application.TestControllerClient.MethodReturningHeaders(
            new HttpHeaderParameter("foo", "foo"),
            new HttpHeaderParameter("header1", "header1"),
            new HttpHeaderParameter("header2", "header2"));

        result.Result["foo"].First().Should().Be("foo");
        result.Result["header1"].First().Should().Be("header1");
        result.Result["header2"].First().Should().Be("header2");
    }

    [Test]
    public async Task HttpPostWithoutBody()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.HttpPostWithoutBody();
        result.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task HttpGetWithBody()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.HttpGetWithBody(5);
        result.Result.Should().Be(5);
    }

    [Test]
    public async Task ExceptionsAreCorrectlyRethrown()
    {
        using var application = CreateApplication();
        Func<Task> sutCall = () => application.TestControllerClient.MethodThrowingInvalidOperationException();
        await sutCall.Should().ThrowAsync<InvalidOperationException>().WithMessage("Correct");
    }

    [Test]
    public async Task When500IsReturnedNoExceptionIsThrown()
    {
        using var application = CreateApplication();
        Func<Task> sutCall = () => application.TestControllerClient.MethodReturning500();
        await sutCall.Should().NotThrowAsync();
    }

    [Test]
    public async Task WhenActionReturnsIncorrectTypeDeserializationFails()
    {
        using var application = CreateApplication();
        HttpCallResponse<int> callResponse = await application.TestControllerClient.MethodReturningBadRequestWithTypedResult();
        callResponse.IsClientErrorStatusCode.Should().BeTrue();
        Action sutCall = () =>
        {
            _ = callResponse.Result;
        };
        sutCall.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public async Task PreModelBinderTest()
    {
        using var application = CreateApplication();
        var applicationFactory = application.WebApplicationFactory.WithRidge(x =>
        {
            x.UseHttpRequestFactoryMiddleware(new TestObjectAddHttpRequestFactoryMiddleware());
        });
        var testClient = new TestControllerClient(applicationFactory.CreateClient(), applicationFactory.Services);

        var result = await testClient.CustomBinderFullObject(
            new TestController.CountryCodeBinded()
            {
                CountryCode = "cz",
            },
            TestObjectAddHttpRequestFactoryMiddleware.UseThisMiddleware());
        result.Result.Should().BeEquivalentTo("cz");
    }

    [Test]
    public async Task TaskCancellationTokenIsRemoved()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.TaskCancellationTokenIsRemoved();
        result.Result.Should().BeEquivalentTo("ok");
    }

    [Test]
    public async Task OrderOfRequestFactoryMiddlewaresIsTheSame()
    {
        using var application = CreateApplication();
        var app = application.WebApplicationFactory.WithRidge(x =>
        {
            x.UseHttpRequestFactoryMiddleware(new HttpRequestFactoryMiddlewareOrder(1));
            x.UseHttpRequestFactoryMiddleware(new HttpRequestFactoryMiddlewareOrder(2));
            x.UseHttpRequestFactoryMiddleware(new HttpRequestFactoryMiddlewareOrder(3));
            x.UseHttpRequestFactoryMiddleware(new HttpRequestFactoryMiddlewareOrder(4));
        });
        var testClient = new TestControllerClient(app.CreateClient(), app.Services);
        var result = await testClient.ReturnsBody("");
        result.Result.Should().BeEquivalentTo("1234");
    }
    
    [Test]
    public async Task CallsWithHttpResponseMessagesAreSupported()
    {
        using var application = CreateApplication();
        var response = await application.ControllerWithSpecialGenerationSettingsClient.SimpleGet(1);
        var result = await response.Content.ReadAsStringAsync();
        result.Should().Be("return");
    }

    [Test]
    public async Task CallsWithTypeTransformation()
    {
        using var application = CreateApplication();
        var task = application.ControllerWithSpecialGenerationSettingsClient.TypeTransformation(1, "transformed");
    }

    [Test]
    public async Task CallActionWithOptionalParameter()
    {
        using var application = CreateApplication();
        var task = application.ControllerWithSpecialGenerationSettingsClient
           .ActionWithOptionalParameter("test",
                "test",
                new[]
                {
                    "test",
                },
                1);
    }
    
    [Test]
    public async Task ExceptionFilterTest()
    {
        using var application = CreateApplication();
        var response = await application.TestControllerClient
           .ExceptionToBeRethrown();
        
        response.IsServerErrorStatusCode.Should().BeTrue();
    }

    internal static Application CreateApplication()
    {
        var webAppFactory = new WebApplicationFactory<Program>();
        return new Application(
            webAppFactory
        );
    }

    public class HttpRequestFactoryMiddlewareOrder : HttpRequestFactoryMiddleware
    {
        private readonly int _order;

        public HttpRequestFactoryMiddlewareOrder(
            int order)
        {
            _order = order;
        }

        public override Task<HttpRequestMessage> CreateHttpRequest(
            IRequestFactoryContext requestFactoryContext)
        {
            requestFactoryContext.Body = ((string)requestFactoryContext.Body!) + _order;
            return base.CreateHttpRequest(requestFactoryContext);
        }
    }
    
    
    public class TestObjectAddHttpRequestFactoryMiddleware : HttpRequestFactoryMiddleware
    {
        public static AdditionalParameter UseThisMiddleware()
        {
            return new AdditionalParameter(nameof(TestObjectAddHttpRequestFactoryMiddleware), null);
        }

        private static bool ShouldThisTransformerBeUsed(
            ParameterProvider parameterProvider)
        {
            var useThiClientParameter = parameterProvider.GetAdditionalParameters().GetParameterByNameOrDefault(nameof(TestObjectAddHttpRequestFactoryMiddleware));
            return useThiClientParameter != null;
        }
        
        public override Task<HttpRequestMessage> CreateHttpRequest(
            IRequestFactoryContext requestFactoryContext)
        {
            if (!ShouldThisTransformerBeUsed(requestFactoryContext.ParameterProvider))
            {
                return base.CreateHttpRequest(requestFactoryContext);
            }

            var boundValue = requestFactoryContext.ParameterProvider
               .GetClientParameters()
               .GetFirstValueByTypeOrThrow<TestController.CountryCodeBinded>();
            if (boundValue == null)
            {
                return base.CreateHttpRequest(requestFactoryContext);
            }

            requestFactoryContext.UrlGenerationParameters["countryCode"] = boundValue.CountryCode;
            return base.CreateHttpRequest(requestFactoryContext);
        }
    }
}

internal sealed class Application : IDisposable
{
    public WebApplicationFactory<Program> WebApplicationFactory { get; set; }
    public TestControllerClient TestControllerClient { get; }
    public ControllerInAreaClient ControllerInAreaClient { get; set; }
    public ControllerWithoutAttributeRoutingClient ControllerWithoutAttributeRoutingClient { get; set; }
    public ControllerWithSpecialGenerationSettingsClient ControllerWithSpecialGenerationSettingsClient { get; set; }
    public AddedParametersWithDefaultMappingControllerClient AddedParametersWithDefaultMappingControllerClient { get; }

    public TransformedParametersWithDefaultMappingControllerClient TransformedParametersWithDefaultMappingControllerClient { get; }
    
    public Application(
        WebApplicationFactory<Program> webApplicationFactory)
    {
        WebApplicationFactory = webApplicationFactory.WithRidge(x =>
        {
            x.UseNunitLogWriter();
            x.UseExceptionRethrowFilter((e) => e is ExceptionToBeRethrown ? false : true);
        });
        var httpClient = WebApplicationFactory.CreateClient();
        TestControllerClient = new TestControllerClient(httpClient, WebApplicationFactory.Services);
        ControllerInAreaClient = new ControllerInAreaClient(httpClient, WebApplicationFactory.Services);
        ControllerWithoutAttributeRoutingClient = new ControllerWithoutAttributeRoutingClient(httpClient, WebApplicationFactory.Services);
        ControllerWithSpecialGenerationSettingsClient = new ControllerWithSpecialGenerationSettingsClient(httpClient, WebApplicationFactory.Services);
        AddedParametersWithDefaultMappingControllerClient = new AddedParametersWithDefaultMappingControllerClient(httpClient, WebApplicationFactory.Services);
        TransformedParametersWithDefaultMappingControllerClient = new TransformedParametersWithDefaultMappingControllerClient(httpClient, WebApplicationFactory.Services);
    }

    public void Dispose()
    {
        WebApplicationFactory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
