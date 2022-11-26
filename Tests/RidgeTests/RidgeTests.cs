using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using NUnit.Framework;
using Ridge;
using Ridge.DelegationHandlers;
using Ridge.HttpRequestFactoryMiddlewares;
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
    public async Task FromFormIsSupported()
    {
        using var application = CreateApplication();
        var result = await application.TestControllerCaller.CallFromForm(new TestController.ComplexArgument(
            integer: 10,
            str: "test",
            dateTime: DateTime.UtcNow.Date,
            innerObject: new TestController.InnerObject(str: "InnerStr")
        ));
        result.Result.Integer.Should().Be(10);
        result.Result.Str.Should().Be("test");
        result.Result.DateTime.ToString("dd/MM/yyyy").Should().Be(DateTime.UtcNow.ToString("dd/MM/yyyy"));
        result.Result.InnerObject!.Str.Should().Be("InnerStr");
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


    [Test]
    public async Task ArrayOfComplexArgumentsInFromQueryIsNotSupported()
    {
        using var application = CreateApplication();
        Func<Task> sutCall = () => application.TestControllerCaller.CallArrayOfComplexArgumentsInFromQuery(new List<TestController.ComplexArgument>());
        await sutCall.Should().ThrowAsync<InvalidOperationException>().WithMessage("*complex type*");
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
    public async Task FromQueryAndFromRouteCanNotHaveSameName()
    {
        using var application = CreateApplication();
        Func<Task> sutCall = () => application.TestControllerCaller.CallFromRouteFromQuerySameName("asd", "asd");
        await sutCall.Should().ThrowAsync<InvalidOperationException>().WithMessage("*FromRoute*").WithMessage("*FromQuery*");
    }

    [Test]
    public async Task NullsCanNotBeInFromRouteArgument()
    {
        using var application = CreateApplication();
        Func<Task> sutCall = () => application.TestControllerCaller.CallNullsTest(1, new TestController.ComplexArgument(), DateTime.Now, null);
        await sutCall.Should().ThrowAsync<InvalidOperationException>().WithMessage("*route*");
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
            new HttpHeader("foo", "foo"),
            new HttpHeader("header1", "header1"),
            new HttpHeader("header2", "header2"));

        result.Result["foo"].First().Should().Be("foo");
        result.Result["header1"].First().Should().Be("header1");
        result.Result["header2"].First().Should().Be("header2");
    }

    [Test]
    public async Task RequestCanBeAlteredUsingGlobalBuilder()
    {
        using var application = CreateApplication();

        application.ApplicationCaller.AddHeader(
            new HttpHeader("foo", "foo"),
            new HttpHeader("header1", "header1"),
            new HttpHeader("header2", "header2")
        );

        var response = await application.TestControllerCaller.CallMethodReturningHeaders();
        response.Result["foo"].First().Should().Be("foo");
        response.Result["header1"].First().Should().Be("header1");
        response.Result["header2"].First().Should().Be("header2");
    }


    [Test]
    public async Task CallWithTwoSameFromHeaderAttributesValid()
    {
        using var application = CreateApplication();

        var result = await application.TestControllerCaller.CallMethodReturningHeaders(
            new HttpHeader("foo", "foo"),
            new HttpHeader("header1", "header1"),
            new HttpHeader("header2", "header2"));

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
        application.ApplicationCaller.AddDelegationHandler(new ListSeparatedByCommasDelegationHandler(new[] {1, 1, 1}));
        var result =
            await application.TestControllerCaller.CallCustomBinder(null!, ListSeparatedByCommasDelegationHandler.UseThisHandler());
        result.Result.Should().AllBeEquivalentTo(1);
    }

    [Test]
    public async Task PreModelBinderTest()
    {
        using var application = CreateApplication();
        application.ApplicationCaller.AddHttpRequestFactoryMiddleware(new TestObjectAddHttpRequestFactoryMiddleware());
        var result = await application.TestControllerCaller.CallCustomBinderFullObject(
            new TestController.CountryCodeBinded()
            {
                CountryCode = "cz",
            },
            TestObjectAddHttpRequestFactoryMiddleware.UseThisMiddleware());
        result.Result.Should().BeEquivalentTo("cz");
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
        var webAppFactory = new WebApplicationFactory<Program>();
        return new Application(
            webAppFactory
        );
    }


    public class ListSeparatedByCommasDelegationHandler : DelegatingHandler
    {
        public class UseThisHandlerClass
        {
        }

        public static UseThisHandlerClass UseThisHandler()
        {
            return new UseThisHandlerClass();
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage httpRequestMessage,
            CancellationToken cancellationToken)
        {
            httpRequestMessage.Options.TryGetValue(new HttpRequestOptionsKey<RequestDescription>(RequestDescription.OptionsKey), out var actionCallDescription);
            if (actionCallDescription!.CustomParametersProvider.GetCustomParametersByType<UseThisHandlerClass>().FirstOrDefault() == null)
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
        public class UseThisTransformerClass
        {
        }

        public static UseThisTransformerClass UseThisMiddleware()
        {
            return new UseThisTransformerClass();
        }

        public override Task<HttpRequestMessage> CreateHttpRequest(
            RequestFactoryContext requestFactoryContext)
        {
            if (requestFactoryContext.CustomParametersProvider.GetCustomParametersByType<UseThisTransformerClass>().FirstOrDefault() == null)
            {
                return base.CreateHttpRequest(requestFactoryContext);
            }

            var bindedObject = requestFactoryContext.Arguments.FirstOrDefault(x => x is TestController.CountryCodeBinded);
            if (bindedObject == null)
            {
                return base.CreateHttpRequest(requestFactoryContext);
            }

            requestFactoryContext.RouteParams["countryCode"] = ((TestController.CountryCodeBinded)bindedObject).CountryCode;
            return base.CreateHttpRequest(requestFactoryContext);
        }
    }
}

internal sealed class Application : IDisposable
{
    public WebApplicationFactory<Program> WebApplicationFactory { get; set; }
    public TestControllerCaller<Program> TestControllerCaller { get; }
    public ControllerInAreaCaller<Program> ControllerInAreaCaller { get; set; }
    public ControllerWithoutAttributeRoutingCaller<Program> ControllerWithoutAttributeRoutingCaller { get; set; }

    public ApplicationCaller<Program> ApplicationCaller { get; set; }

    public ControllerWithSpecialGenerationSettingsCaller<Program> ControllerWithSpecialGenerationSettingsCaller { get; set; }

    public Application(
        WebApplicationFactory<Program> webApplicationFactory)
    {
        WebApplicationFactory = webApplicationFactory;
        ApplicationCaller = new ApplicationCaller<Program>(WebApplicationFactory);
        TestControllerCaller = new TestControllerCaller<Program>(ApplicationCaller);
        ControllerInAreaCaller = new ControllerInAreaCaller<Program>(ApplicationCaller);
        ControllerWithoutAttributeRoutingCaller = new ControllerWithoutAttributeRoutingCaller<Program>(ApplicationCaller);
        ControllerWithSpecialGenerationSettingsCaller = new ControllerWithSpecialGenerationSettingsCaller<Program>(ApplicationCaller);
    }

    public void Dispose()
    {
        WebApplicationFactory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
