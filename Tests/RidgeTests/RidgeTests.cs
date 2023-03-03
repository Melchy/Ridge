using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using NUnit.Framework;
using Ridge.DelegationHandlers;
using Ridge.HttpRequestFactoryMiddlewares;
using Ridge.LogWriter;
using Ridge.Parameters;
using Ridge.Parameters.AdditionalParams;
using Ridge.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
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
        var response = await application.TestControllerClient.CallReturnSync();
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task SyncCallWithResult()
    {
        using var application = CreateApplication();
        var response = await application.TestControllerClient.CallReturnSyncWithResult();
        response.IsSuccessStatusCode.Should().BeTrue();
        response.Result.Should().Be("ok");
    }

    [Test]
    public async Task SyncCallThrowingNotWrappedException()
    {
        using var application = CreateApplication();
        var result = async () => await application.TestControllerClient.CallSyncThrow();
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
        var response = await application.TestControllerClient.CallArgumentsWithoutAttributes(complexObject,
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
        var result = await application.TestControllerClient.CallReturnAsync();
        result.Result.Should().Be(10);
    }

    [Test]
    public async Task AsyncCallWithoutResult()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.CallBadRequestAsync();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task AreasAreSupported()
    {
        using var application = CreateApplication();
        var result = await application.ControllerInAreaClient.CallIndex();
        result.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task MethodOverloadingIsSupported()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.CallOverloadedAction();
        result.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task MethodOverloadingIsSupported2()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.CallOverloadedAction(1);
        result.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task SimpleArgumentsAreMapped()
    {
        using var application = CreateApplication();
        var response =
            await application.TestControllerClient.CallSimpleArguments(1,
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
        var result = await application.TestControllerClient.CallComplexBody(new TestController.ComplexArgument(
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
        var response = await application.TestControllerClient.CallComplexBody(null!);
        response.Result.Should().BeNull();
    }

    [Test]
    public async Task FromQueryCanContainComplexObject()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.CallComplexFromQuery(new TestController.ComplexArgument(
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
        var response = await application.TestControllerClient.CallFromHeaderWithArray(new[] {1, 2});
        response.Result.header1.Should().Be(1);
        response.Result.header2.Should().Be(2);
    }

    [Test]
    public async Task FromHeaderIsSupportedForSimpleArguments()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.CallFromHeaderSimple(1);
        result.Result.Should().Be(1);
    }

    [Test]
    public async Task NameInFromQueryAttributeIsSupportedForComplexArgument()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.CallFromQueryWithNameComplexArgument(new TestController.Test() {Foo = 1});
        result.Result.Foo.Should().Be(1);
    }

    [Test]
    public async Task NameInFromQueryAttributeIsSupportedSimpleArgument()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.CallFromQueryWithNameSimpleArgument(1);
        result.Result.Should().Be(1);
    }

    // This works in test but it would not work in real application
    // Test adds default values bud real app does not
    [Test]
    public async Task ObjectWithDefaultValuesInCtorDoesNotWorkWhenBindingUsingJsonNet()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.CallDefaultPropertiesInCtorTest(new ObjectWithDefaultProperties());
        result.Result.Str.Should().Be("test");
    }

    [Test]
    public async Task NullsCanBePlacedInFromQueryOrFromBodyOrFromHead()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.CallNullsTest(null, null, null, "asd");
        result.Result.Item1.Should().Be(null);
        result.Result.Item2.Should().Be(null);
        result.Result.Item3.Should().Be(null);
    }

    [Test]
    public async Task ArrayInFromQueryIsSupported()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.CallArrayInFromQuery(new List<int>() {1, 1, 1});
        result.Result.Should().AllBeEquivalentTo(1);
    }

    [Test]
    public async Task NullInFromRouteCausesNotFound()
    {
        using var application = CreateApplication();
        var response = await application.TestControllerClient.CallNullsTest(1, new TestController.ComplexArgument(), DateTime.Now, null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task NameInFromRouteAttributeIsSupportedSimpleArgument()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.CallFromRouteWithNameSimpleArgument(1);
        result.Result.Should().Be(1);
    }

    [Test]
    public async Task ClassicalRoutingIsSupported()
    {
        using var application = CreateApplication();
        var result = await application.ControllerWithoutAttributeRoutingClient.CallHttpGetWithoutBody();
        result.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task FromServicesIsIgnored()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.CallFromServices();
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
        var result = await application.TestControllerClient.CallArrayInBody(data);
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

        var result = await application.TestControllerClient.CallMethodReturningHeaders(
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

        var response = await new TestControllerClient(client, application.WebApplicationFactory.Services).CallMethodReturningHeaders();
        response.Result["foo"].First().Should().Be("foo");
        response.Result["header1"].First().Should().Be("header1");
        response.Result["header2"].First().Should().Be("header2");
    }


    [Test]
    public async Task CallWithTwoSameFromHeaderAttributesValid()
    {
        using var application = CreateApplication();

        var result = await application.TestControllerClient.CallMethodReturningHeaders(
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
        var result = await application.TestControllerClient.CallHttpPostWithoutBody();
        result.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task HttpGetWithBody()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerClient.CallHttpGetWithBody(5);
        result.Result.Should().Be(5);
    }

    [Test]
    public async Task ExceptionsAreCorrectlyRethrown()
    {
        using var application = CreateApplication();
        Func<Task> sutCall = () => application.TestControllerClient.CallMethodThrowingInvalidOperationException();
        await sutCall.Should().ThrowAsync<InvalidOperationException>().WithMessage("Correct");
    }

    [Test]
    public async Task When500IsReturnedNoExceptionIsThrown()
    {
        using var application = CreateApplication();
        Func<Task> sutCall = () => application.TestControllerClient.CallMethodReturning500();
        await sutCall.Should().NotThrowAsync();
    }

    [Test]
    public async Task WhenActionReturnsIncorrectTypeDeserializationFails()
    {
        using var application = CreateApplication();
        HttpCallResponse<int> callResponse = await application.TestControllerClient.CallMethodReturningBadRequestWithTypedResult();
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
            x.HttpRequestFactoryMiddlewares.Add(new TestObjectAddHttpRequestFactoryMiddleware());
        });
        var testClient = new TestControllerClient(applicationFactory.CreateClient(), applicationFactory.Services);

        var result = await testClient.CallCustomBinderFullObject(
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
        var result = await application.TestControllerClient.CallTaskCancellationTokenIsRemoved();
        result.Result.Should().BeEquivalentTo("ok");
    }

    [Test]
    public async Task OrderOfRequestFactoryMiddlewaresIsTheSame()
    {
        using var application = CreateApplication();
        var app = application.WebApplicationFactory.WithRidge(x =>
        {
            x.HttpRequestFactoryMiddlewares.Add(new HttpRequestFactoryMiddlewareOrder(1));
            x.HttpRequestFactoryMiddlewares.Add(new HttpRequestFactoryMiddlewareOrder(2));
            x.HttpRequestFactoryMiddlewares.Add(new HttpRequestFactoryMiddlewareOrder(3));
            x.HttpRequestFactoryMiddlewares.Add(new HttpRequestFactoryMiddlewareOrder(4));
        });
        var testClient = new TestControllerClient(app.CreateClient(), app.Services);
        var result = await testClient.CallReturnsBody("");
        result.Result.Should().BeEquivalentTo("1234");
    }
    
    [Test]
    public async Task CallsWithHttpResponseMessagesAreSupported()
    {
        using var application = CreateApplication();
        var response = await application.ControllerWithSpecialGenerationSettingsClient.CallSimpleGet(1);
        var result = await response.Content.ReadAsStringAsync();
        result.Should().Be("return");
    }

    [Test]
    public async Task CallsWithTypeTransformation()
    {
        using var application = CreateApplication();
        var task = application.ControllerWithSpecialGenerationSettingsClient.CallTypeTransformation(1, "transformed");
    }

    [Test]
    public async Task CallActionWithOptionalParameter()
    {
        using var application = CreateApplication();
        var task = application.ControllerWithSpecialGenerationSettingsClient
           .CallActionWithOptionalParameter("test",
                "test",
                new[]
                {
                    "test",
                },
                1);
    }

    internal static Application CreateApplication()
    {
        var webAppFactory = new WebApplicationFactory<Program>();
        return new Application(
            webAppFactory
        );
    }


    public class ListSeparatedByCommasDelegationHandler : DelegatingHandler
    {
        public static AdditionalParameter UseThisHandler()
        {
            return new AdditionalParameter(nameof(ListSeparatedByCommasDelegationHandler), null);
        }

        private static bool ShouldThisTransformerBeUsed(
            ParameterProvider parameterProvider)
        {
            var useThiClientParameter = parameterProvider.GetAdditionalParameters().GetParameterByNameOrDefault(nameof(ListSeparatedByCommasDelegationHandler));
            return useThiClientParameter != null;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage httpRequestMessage,
            CancellationToken cancellationToken)
        {
            var requestDescription = httpRequestMessage.GetRequestDescription();
            if (!ShouldThisTransformerBeUsed(requestDescription.ParameterProvider))
            {
                return await base.SendAsync(httpRequestMessage, cancellationToken);
            }

            httpRequestMessage.RequestUri = new Uri(
                QueryHelpers.AddQueryString(httpRequestMessage.RequestUri!.ToString(), "properties", $"{string.Join(",", _data)}"),
                UriKind.Absolute);
            return await base.SendAsync(httpRequestMessage, cancellationToken);
        }


        private readonly IEnumerable<int> _data;

        public ListSeparatedByCommasDelegationHandler(
            IEnumerable<int> data)
        {
            _data = data;
        }
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
            x.LogWriter = new NunitLogWriter();
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
