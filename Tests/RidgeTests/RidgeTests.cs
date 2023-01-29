using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using NUnit.Framework;
using Ridge;
using Ridge.DelegationHandlers;
using Ridge.HttpRequestFactoryMiddlewares;
using Ridge.Parameters;
using Ridge.Parameters.CustomParams;
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
        var response = await application.TestControllerCaller.CallReturnSync();
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task SyncCallWithResult()
    {
        using var application = CreateApplication();
        var response = await application.TestControllerCaller.CallReturnSyncWithResult();
        response.IsSuccessStatusCode.Should().BeTrue();
        response.Result.Should().Be("ok");
    }

    [Test]
    public async Task SyncCallThrowingNotWrappedException()
    {
        using var application = CreateApplication();
        var result = async () => await application.TestControllerCaller.CallSyncThrow();
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
        var response = await application.TestControllerCaller.CallArgumentsWithoutAttributes(complexObject,
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
        var result = await application.TestControllerCaller.CallReturnAsync();
        result.Result.Should().Be(10);
    }

    [Test]
    public async Task AsyncCallWithoutResult()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerCaller.CallBadRequestAsync();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task AreasAreSupported()
    {
        using var application = CreateApplication();
        var result = await application.ControllerInAreaCaller.CallIndex();
        result.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task MethodOverloadingIsSupported()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerCaller.CallOverloadedAction();
        result.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task MethodOverloadingIsSupported2()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerCaller.CallOverloadedAction(1);
        result.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task SimpleArgumentsAreMapped()
    {
        using var application = CreateApplication();
        var response =
            await application.TestControllerCaller.CallSimpleArguments(1,
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
        var result = await application.TestControllerCaller.CallComplexBody(new TestController.ComplexArgument(
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
    public async Task FromQueryCanContainComplexObject()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerCaller.CallComplexFromQuery(new TestController.ComplexArgument(
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
        var response = await application.TestControllerCaller.CallFromHeaderWithArray(new[] {1, 2});
        response.Result.header1.Should().Be(1);
        response.Result.header2.Should().Be(2);
    }

    [Test]
    public async Task FromHeaderIsSupportedForSimpleArguments()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerCaller.CallFromHeaderSimple(1);
        result.Result.Should().Be(1);
    }

    [Test]
    public async Task NameInFromQueryAttributeIsSupportedForComplexArgument()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerCaller.CallFromQueryWithNameComplexArgument(new TestController.Test() {Foo = 1});
        result.Result.Foo.Should().Be(1);
    }

    [Test]
    public async Task NameInFromQueryAttributeIsSupportedSimpleArgument()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerCaller.CallFromQueryWithNameSimpleArgument(1);
        result.Result.Should().Be(1);
    }

    // This works in test but it would not work in real application
    // Test adds default values bud real app does not
    [Test]
    public async Task ObjectWithDefaultValuesInCtorDoesNotWorkWhenBindingUsingJsonNet()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerCaller.CallDefaultPropertiesInCtorTest(new ObjectWithDefaultProperties());
        result.Result.Str.Should().Be("test");
    }

    [Test]
    public async Task NullsCanBePlacedInFromQueryOrFromBodyOrFromHead()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerCaller.CallNullsTest(null, null, null, "asd");
        result.Result.Item1.Should().Be(null);
        result.Result.Item2.Should().Be(null);
        result.Result.Item3.Should().Be(null);
    }

    [Test]
    public async Task ArrayInFromQueryIsSupported()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerCaller.CallArrayInFromQuery(new List<int>() {1, 1, 1});
        result.Result.Should().AllBeEquivalentTo(1);
    }

    [Test]
    public async Task NullInFromRouteCausesNotFound()
    {
        using var application = CreateApplication();
        var response = await application.TestControllerCaller.CallNullsTest(1, new TestController.ComplexArgument(), DateTime.Now, null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task NameInFromRouteAttributeIsSupportedSimpleArgument()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerCaller.CallFromRouteWithNameSimpleArgument(1);
        result.Result.Should().Be(1);
    }

    [Test]
    public async Task ClassicalRoutingIsSupported()
    {
        using var application = CreateApplication();
        var result = await application.ControllerWithoutAttributeRoutingCaller.CallHttpGetWithoutBody();
        result.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task FromServicesIsIgnored()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerCaller.CallFromServices();
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
        var result = await application.TestControllerCaller.CallArrayInBody(data);
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

        var result = await application.TestControllerCaller.CallMethodReturningHeaders(
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

        application.RidgeApplicationFactory.AddHeader(
            new HttpHeaderParameter("foo", "foo"),
            new HttpHeaderParameter("header1", "header1"),
            new HttpHeaderParameter("header2", "header2")
        );

        var response = await new TestControllerCaller(application.RidgeApplicationFactory.CreateRidgeClient()).CallMethodReturningHeaders();
        response.Result["foo"].First().Should().Be("foo");
        response.Result["header1"].First().Should().Be("header1");
        response.Result["header2"].First().Should().Be("header2");
    }


    [Test]
    public async Task CallWithTwoSameFromHeaderAttributesValid()
    {
        using var application = CreateApplication();

        var result = await application.TestControllerCaller.CallMethodReturningHeaders(
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
        var result = await application.TestControllerCaller.CallHttpPostWithoutBody();
        result.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task HttpGetWithBody()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerCaller.CallHttpGetWithBody(5);
        result.Result.Should().Be(5);
    }

    [Test]
    public async Task ExceptionsAreCorrectlyRethrown()
    {
        using var application = CreateApplication();
        Func<Task> sutCall = () => application.TestControllerCaller.CallMethodThrowingInvalidOperationException();
        await sutCall.Should().ThrowAsync<InvalidOperationException>().WithMessage("Correct");
    }

    [Test]
    public async Task When500IsReturnedNoExceptionIsThrown()
    {
        using var application = CreateApplication();
        Func<Task> sutCall = () => application.TestControllerCaller.CallMethodReturning500();
        await sutCall.Should().NotThrowAsync();
    }

    [Test]
    public async Task WhenActionReturnsIncorrectTypeDeserializationFails()
    {
        using var application = CreateApplication();
        HttpCallResponse<int> callResponse = await application.TestControllerCaller.CallMethodReturningBadRequestWithTypedResult();
        callResponse.IsClientErrorStatusCode.Should().BeTrue();
        Action sutCall = () =>
        {
            _ = callResponse.Result;
        };
        sutCall.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public async Task CustomDelegationHandler()
    {
        using var application = CreateApplication();
        application.RidgeApplicationFactory.AddDelegationHandler(new ListSeparatedByCommasDelegationHandler(new[] {1, 1, 1}));

        var testCaller = new TestControllerCaller(application.RidgeApplicationFactory.CreateRidgeClient());
        var result =
            await testCaller.CallCustomBinder(null!, ListSeparatedByCommasDelegationHandler.UseThisHandler());
        result.Result.Should().AllBeEquivalentTo(1);
    }

    [Test]
    public async Task PreModelBinderTest()
    {
        using var application = CreateApplication();
        application.RidgeApplicationFactory.AddHttpRequestFactoryMiddleware(new TestObjectAddHttpRequestFactoryMiddleware());
        var testCaller = new TestControllerCaller(application.RidgeApplicationFactory.CreateRidgeClient());

        var result = await testCaller.CallCustomBinderFullObject(
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
        application.RidgeApplicationFactory.AddHttpRequestFactoryMiddleware(new TestObjectAddHttpRequestFactoryMiddleware());

        var testCaller = new TestControllerCaller(application.RidgeApplicationFactory.CreateRidgeClient());
        var result = await testCaller.CallTaskCancellationTokenIsRemoved();
        result.Result.Should().BeEquivalentTo("ok");
    }
    
    [Test]
    public async Task CallsWithHttpResponseMessagesAreSupported()
    {
        using var application = CreateApplication();
        var response = await application.ControllerWithSpecialGenerationSettingsCaller.CallSimpleGet(1);
        var result = await response.Content.ReadAsStringAsync();
        result.Should().Be("return");
    }

    [Test]
    public async Task CallsWithTypeTransformation()
    {
        using var application = CreateApplication();
        var task = application.ControllerWithSpecialGenerationSettingsCaller.CallTypeTransformation(1, "transformed");
    }

    [Test]
    public async Task CallActionWithOptionalParameter()
    {
        using var application = CreateApplication();
        var task = application.ControllerWithSpecialGenerationSettingsCaller
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
        var webAppFactory = new RidgeApplicationFactory<Program>();
        return new Application(
            webAppFactory
        );
    }


    public class ListSeparatedByCommasDelegationHandler : DelegatingHandler
    {
        public static CustomParameter UseThisHandler()
        {
            return new CustomParameter(nameof(ListSeparatedByCommasDelegationHandler), null);
        }

        private static bool ShouldThisTransformerBeUsed(
            ParameterProvider parameterProvider)
        {
            var useThiCallerParameter = parameterProvider.GetCustomParameters().GetParameterByNameOrDefault(nameof(ListSeparatedByCommasDelegationHandler));
            return useThiCallerParameter != null;
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

    public class TestObjectAddHttpRequestFactoryMiddleware : HttpRequestFactoryMiddleware
    {
        public static CustomParameter UseThisMiddleware()
        {
            return new CustomParameter(nameof(TestObjectAddHttpRequestFactoryMiddleware), null);
        }

        private static bool ShouldThisTransformerBeUsed(
            ParameterProvider parameterProvider)
        {
            var useThiCallerParameter = parameterProvider.GetCustomParameters().GetParameterByNameOrDefault(nameof(TestObjectAddHttpRequestFactoryMiddleware));
            return useThiCallerParameter != null;
        }
        
        public override Task<HttpRequestMessage> CreateHttpRequest(
            IRequestFactoryContext requestFactoryContext)
        {
            if (!ShouldThisTransformerBeUsed(requestFactoryContext.ParameterProvider))
            {
                return base.CreateHttpRequest(requestFactoryContext);
            }

            var boundValue = requestFactoryContext.ParameterProvider
               .GetCallerParameters()
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
    public RidgeApplicationFactory<Program> RidgeApplicationFactory { get; set; }
    public TestControllerCaller TestControllerCaller { get; }
    public ControllerInAreaCaller ControllerInAreaCaller { get; set; }
    public ControllerWithoutAttributeRoutingCaller ControllerWithoutAttributeRoutingCaller { get; set; }
    public ControllerWithSpecialGenerationSettingsCaller ControllerWithSpecialGenerationSettingsCaller { get; set; }
    public AddedParametersWithDefaultMappingControllerCaller AddedParametersWithDefaultMappingControllerCaller { get; }

    public TransformedParametersWithDefaultMappingControllerCaller TransformedParametersWithDefaultMappingControllerCaller { get; }
    
    public Application(
        RidgeApplicationFactory<Program> ridgeApplicationFactory)
    {
        RidgeApplicationFactory = ridgeApplicationFactory.AddNUnitLogger();
        var ridgeHttpClient = ridgeApplicationFactory.CreateRidgeClient();
        TestControllerCaller = new TestControllerCaller(ridgeHttpClient);
        ControllerInAreaCaller = new ControllerInAreaCaller(ridgeHttpClient);
        ControllerWithoutAttributeRoutingCaller = new ControllerWithoutAttributeRoutingCaller(ridgeHttpClient);
        ControllerWithSpecialGenerationSettingsCaller = new ControllerWithSpecialGenerationSettingsCaller(ridgeHttpClient);
        AddedParametersWithDefaultMappingControllerCaller = new AddedParametersWithDefaultMappingControllerCaller(ridgeHttpClient);
        TransformedParametersWithDefaultMappingControllerCaller = new TransformedParametersWithDefaultMappingControllerCaller(ridgeHttpClient);
    }

    public void Dispose()
    {
        RidgeApplicationFactory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
