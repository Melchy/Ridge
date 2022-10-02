using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using NUnit.Framework;
using Ridge.ActionInfo;
using Ridge.Interceptor;
using Ridge.LogWriter;
using Ridge.Pipeline.Public;
using Ridge.Response;
using Ridge.Transformers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TestWebApplication;
using TestWebApplication.Controllers;
using TestWebApplication.Controllers.Examples;

namespace RidgeTests
{
    public class RidgeTesting
    {
        [Test]
        public async Task SyncCallWithoutResult()
        {
            using var application = CreateApplication();
            var response = await application.TestControllerCaller.Call_ReturnSync();
            response.IsSuccessStatusCode.Should().BeTrue();
        }

        [Test]
        public async Task SyncCallWithResult()
        {
            using var application = CreateApplication();
            var response = await application.TestControllerCaller.Call_ReturnSyncWithResult();
            response.IsSuccessStatusCode.Should().BeTrue();
            response.Result.Should().Be("ok");
        }

        [Test]
        public async Task SyncCallThrowingNotWrappedException()
        {
            using var application = CreateApplication();
            Func<Task> result = async () => await application.TestControllerCaller.Call_SyncThrow();
            result.Should().Throw<InvalidOperationException>().WithMessage("Error");
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
            var response = await application.TestControllerCaller.Call_ArgumentsWithoutAttributes(complexObject,
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
            var result = await application.TestControllerCaller.Call_ReturnAsync();
            result.Result.Should().Be(10);
        }

        [Test]
        public async Task AsyncCallWithoutResult()
        {
            using var application = CreateApplication();
            var result = await application.TestControllerCaller.Call_BadRequestAsync();
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task AreasAreSupported()
        {
            using var application = CreateApplication();
            var result = await application.ControllerInAreaCaller.Call_Index();
            result.IsSuccessStatusCode.Should().BeTrue();
        }

        [Test]
        public async Task MethodOverloadingIsSupported()
        {
            using var application = CreateApplication();
            var result = await application.TestControllerCaller.Call_OverloadedAction();
            result.IsSuccessStatusCode.Should().BeTrue();
        }

        [Test]
        public async Task MethodOverloadingIsSupported2()
        {
            using var application = CreateApplication();
            var result = await application.TestControllerCaller.Call_OverloadedAction(1);
            result.IsSuccessStatusCode.Should().BeTrue();
        }

        [Test]
        public async Task SimpleArgumentsAreMapped()
        {
            using var application = CreateApplication();
            var response =
                await application.TestControllerCaller.Call_SimpleArguments(1,
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
            var result = await application.TestControllerCaller.Call_ComplexBody(new TestController.ComplexArgument(
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
            var result = await application.TestControllerCaller.Call_ComplexFromQuery(new TestController.ComplexArgument(
                integer: 10,
                str: "test",
                dateTime: DateTime.Today,
                innerObject: new TestController.InnerObject("test")
                {
                    List = new List<string>() { "a", "b" },
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
            var result = await application.TestControllerCaller.Call_FromForm(new TestController.ComplexArgument(
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
            var response = await application.TestControllerCaller.Call_FromHeaderWithArray(new[] {1, 2});
            response.Result.header1.Should().Be(1);
            response.Result.header2.Should().Be(2);
        }

        [Test]
        public async Task FromHeaderIsSupportedForSimpleArguments()
        {
            using var application = CreateApplication();
            var result = await application.TestControllerCaller.Call_FromHeaderSimple(1);
            result.Result.Should().Be(1);
        }

        [Test]
        public async Task NameInFromQueryAttributeIsSupportedForComplexArgument()
        {
            using var application = CreateApplication();
            var result = await application.TestControllerCaller.Call_FromQueryWithNameComplexArgument(new TestController.Test() {Foo = 1});
            result.Result.Foo.Should().Be(1);
        }

        [Test]
        public async Task NameInFromQueryAttributeIsSupportedSimpleArgument()
        {
            using var application = CreateApplication();
            var result = await application.TestControllerCaller.Call_FromQueryWithNameSimpleArgument(1);
            result.Result.Should().Be(1);
        }


        [Test]
        public void ArrayOfComplexArgumentsInFromQueryIsNotSupported()
        {
            using var application = CreateApplication();
            Func<Task> sutCall = () => application.TestControllerCaller.Call_ArrayOfComplexArgumentsInFromQuery(new List<TestController.ComplexArgument>());
            sutCall.Should().Throw<InvalidOperationException>().WithMessage("*complex type*");
        }


        // This works in test but it would not work in real application
        // Test adds default values bud real app does not
        [Test]
        public async Task ObjectWithDefaultValuesInCtorDoesNotWorkWhenBindingUsingJsonNet()
        {
            using var application = CreateApplication();
            var result = await application.TestControllerCaller.Call_DefaultPropertiesInCtorTest(new ObjectWithDefaultProperties());
            result.Result.Str.Should().Be("test");
        }

        [Test]
        public async Task NullsCanBePlacedInFromQueryOrFromBodyOrFromHead()
        {
            using var application = CreateApplication();
            var result = await application.TestControllerCaller.Call_NullsTest(null, null, null, "asd");
            result.Result.Item1.Should().Be(null);
            result.Result.Item2.Should().Be(null);
            result.Result.Item3.Should().Be(null);
        }

        [Test]
        public async Task ArrayInFromQueryIsSupported()
        {
            using var application = CreateApplication();
            var result = await application.TestControllerCaller.Call_ArrayInFromQuery(new List<int>() {1, 1, 1});
            result.Result.Should().AllBeEquivalentTo(1);
        }


        [Test]
        public async Task FromQueryAndFromRouteCanNotHaveSameName()
        {
            using var application = CreateApplication();
            Func<Task> sutCall = () => application.TestControllerCaller.Call_FromRouteFromQuerySameName("asd", "asd");
            sutCall.Should().Throw<InvalidOperationException>().WithMessage("*FromRoute*").WithMessage("*FromQuery*");
        }

        [Test]
        public void NullsCanNotBeInFromRouteArgument()
        {
            using var application = CreateApplication();
            Func<Task> sutCall = () => application.TestControllerCaller.Call_NullsTest(1, new TestController.ComplexArgument(), DateTime.Now, null);
            sutCall.Should().Throw<InvalidOperationException>().WithMessage("*route*");
        }

        [Test]
        public async Task NameInFromRouteAttributeIsSupportedSimpleArgument()
        {
            using var application = CreateApplication();
            var result = await application.TestControllerCaller.Call_FromRouteWithNameSimpleArgument(1);
            result.Result.Should().Be(1);
        }

        [Test]
        public async Task ClassicalRoutingIsSupported()
        {
            using var application = CreateApplication();
            var result = await application.ControllerWithoutAttributeRoutingCaller.Call_HttpGetWithoutBody();
            result.IsSuccessStatusCode.Should().BeTrue();
        }

        [Test]
        public async Task FromServicesIsIgnored()
        {
            using var application = CreateApplication();
            var result = await application.TestControllerCaller.Call_FromServices(null);
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
            var result = await application.TestControllerCaller.Call_ArrayInBody(data);
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

            var result = await application.TestControllerCaller.Call_MethodReturningHeaders(
                headers: new List<(string, string?)>
                {
                    ("foo", "foo"),
                    ("header1", "header1"),
                    ("header2", "header2"),
                },
                authenticationHeaderValue: new AuthenticationHeaderValue("Bearer", "key"));

            result.Result["foo"].First().Should().Be("foo");
            result.Result["header1"].First().Should().Be("header1");
            result.Result["header2"].First().Should().Be("header2");
            result.Result["Authorization"].First().Should().Be("Bearer key");
        }

        [Test]
        public async Task RequestCanBeAlteredUsingGlobalBuilder()
        {
            using var application = CreateApplication();

            var testControllerCaller = new TestControllerCaller(
                application.WebApplicationFactory.CreateClient(),
                application.WebApplicationFactory.Services);

            testControllerCaller.AddHeaders(
                ("foo", "foo"),
                ("header1", "header1"),
                ("header2", "header2")
            );

            testControllerCaller.AddAuthenticationHeaderValue(new AuthenticationHeaderValue("Bearer", "key"));
            var response = await testControllerCaller.Call_MethodReturningHeaders();
            response.Result["foo"].First().Should().Be("foo");
            response.Result["header1"].First().Should().Be("header1");
            response.Result["header2"].First().Should().Be("header2");
            response.Result["Authorization"].First().Should().Be("Bearer key");
        }


        [Test]
        public async Task CallWithTwoSameFromHeaderAttributesValid()
        {
            using var application = CreateApplication();

            var result = await application.TestControllerCaller.Call_MethodReturningHeaders(
                headers: new List<(string, string?)>
                {
                    ("foo", "foo"),
                    ("header1", "header1"),
                    ("header2", "header2"),
                },
                authenticationHeaderValue: new AuthenticationHeaderValue("Bearer", "key"));

            result.Result["foo"].First().Should().Be("foo");
            result.Result["header1"].First().Should().Be("header1");
            result.Result["header2"].First().Should().Be("header2");
            result.Result["Authorization"].First().Should().Be("Bearer key");
        }
        
        
        [Test]
        public async Task HttpPostWithoutBody()
        {
            using var application = CreateApplication();
            var result = await application.TestControllerCaller.Call_HttpPostWithoutBody();
            result.IsSuccessStatusCode.Should().BeTrue();
        }

        [Test]
        public async Task HttpGetWithBody()
        {
            using var application = CreateApplication();
            var result = await application.TestControllerCaller.Call_HttpGetWithBody(5);
            result.Result.Should().Be(5);
        }

        [Test]
        public void ExceptionsAreCorrectlyRethrown()
        {
            using var application = CreateApplication();
            Func<Task> sutCall = () => application.TestControllerCaller.Call_MethodThrowingInvalidOperationException();
            sutCall.Should().Throw<InvalidOperationException>().WithMessage("Correct");
        }

        [Test]
        public void When500IsReturnedNoExceptionIsThrown()
        {
            using var application = CreateApplication();
            Func<Task> sutCall = () => application.TestControllerCaller.Call_MethodReturning500();
            sutCall.Should().NotThrow();
        }

        [Test]
        public async Task WhenActionReturnsIncorrectTypeDeserializationFails()
        {
            using var application = CreateApplication();
            HttpCallResponse<int> callResponse = await application.TestControllerCaller.Call_MethodReturningBadRequestWithTypedResult();
            callResponse.IsClientErrorStatusCode.Should().BeTrue();
            Action sutCall = () =>
            {
                _ = callResponse.Result;
            };
            sutCall.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public async Task ModelBinderIsSupported()
        {
            using var application = CreateApplication();
            var result = await application.TestControllerCaller.Call_CustomBinder(null!,
                httpRequestPipelineParts: new List<IHttpRequestPipelinePart>()
                {
                    new ListSeparatedByCommasPipelinePart(new List<int>() {1, 1, 1}),
                });
            result.Result.Should().AllBeEquivalentTo(1);
        }
        
        [Test]
        public async Task PreModelBinderTest()
        {
            using var application = CreateApplication();
            var result = await application.TestControllerCaller.Call_CustomBinderFullObject(
                new TestController.CountryCodeBinded() {CountryCode = "cz"},
                actionInfoTransformers: new List<IActionInfoTransformer>()
                {
                    new TestObjectActionInfoTransformer(),
                });
            result.Result.Should().BeEquivalentTo("cz");
        }

        [Test]
        public async Task CallsWithHttpResponseMessagesAreSupported()
        {
            using var application = CreateApplication();
            var response = await application.ControllerWithSpecialGenerationSettingsCaller.Call_SimpleGet();
            var result = await response.Content.ReadAsStringAsync();
            result.Should().Be("return");
        }

        public static Application CreateApplication()
        {
            var webAppFactory = new WebApplicationFactory<Startup>();
            return new Application(
                webAppFactory
            );
        }


        public class ListSeparatedByCommasPipelinePart : IHttpRequestPipelinePart
        {
            private readonly IEnumerable<int> _data;

            public ListSeparatedByCommasPipelinePart(
                IEnumerable<int> data)
            {
                _data = data;
            }

            public Task<HttpResponseMessage> InvokeAsync(
                Func<Task<HttpResponseMessage>> next,
                HttpRequestMessage httpRequestMessage,
                IReadOnlyActionInfo actionInfo,
                MethodInvocationInfo methodInvocationInfo)
            {
                httpRequestMessage.RequestUri = new Uri(
                    QueryHelpers.AddQueryString(httpRequestMessage.RequestUri!.ToString(), "properties", $"{string.Join(",", _data)}"),
                    UriKind.Relative);
                return next();
            }
        }

        public class TestObjectActionInfoTransformer : IActionInfoTransformer
        {
            public Task TransformAsync(
                IActionInfo actionInfo,
                MethodInvocationInfo methodInvocationInfo)
            {
                var bindedObject = methodInvocationInfo.Arguments.FirstOrDefault(x => x is TestController.CountryCodeBinded);
                if (bindedObject == null)
                {
                    return Task.CompletedTask;
                }

                actionInfo.RouteParams["countryCode"] = ((TestController.CountryCodeBinded)bindedObject).CountryCode;
                return Task.CompletedTask;
            }
        }
    }

    public sealed class Application : IDisposable
    {
        public WebApplicationFactory<Startup> WebApplicationFactory { get; set; }
        public TestControllerCaller TestControllerCaller { get; }
        public ControllerInAreaCaller ControllerInAreaCaller { get; set; }
        public ControllerWithoutAttributeRoutingCaller ControllerWithoutAttributeRoutingCaller { get; set; }

        public ControllerWithSpecialGenerationSettingsCaller ControllerWithSpecialGenerationSettingsCaller { get; set; }
        
        public Application(
            WebApplicationFactory<Startup> webApplicationFactory)
        {
            WebApplicationFactory = webApplicationFactory;
            TestControllerCaller = new TestControllerCaller(
                webApplicationFactory.CreateClient(),
                webApplicationFactory.Services,
                new NunitProgressLogWriter());
            ControllerInAreaCaller = new ControllerInAreaCaller(
                webApplicationFactory.CreateClient(),
                webApplicationFactory.Services,
                new NunitProgressLogWriter());
            ControllerWithoutAttributeRoutingCaller = new ControllerWithoutAttributeRoutingCaller(
                webApplicationFactory.CreateClient(),
                webApplicationFactory.Services,
                new NunitProgressLogWriter());
            ControllerWithSpecialGenerationSettingsCaller = new ControllerWithSpecialGenerationSettingsCaller(
                webApplicationFactory.CreateClient(),
                webApplicationFactory.Services,
                new NunitProgressLogWriter());
        }

        public void Dispose()
        {
            WebApplicationFactory?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
