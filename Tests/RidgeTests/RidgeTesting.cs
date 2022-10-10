using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using NUnit.Framework;
using Ridge.CallResult.Controller.Extensions;
using Ridge.Interceptor;
using Ridge.Interceptor.InterceptorFactory;
using Ridge.LogWriter;
using Ridge.Pipeline.Public;
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
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = testController.ReturnSync();
            result.IsSuccessStatusCode().Should().BeTrue();
        }

        [Test]
        public async Task SyncCallWithResult()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = testController.ReturnSyncWithResult();
            result.IsSuccessStatusCode().Should().BeTrue();
            result.GetResult().Should().Be("ok");
        }

        [Test]
        public async Task SyncCallThrowingNotWrappedException()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            Action result = () => testController.SyncThrow();
            result.Should().Throw<InvalidOperationException>().WithMessage("Error");
        }

        [Test]
        public async Task SyncMethodWithIncorrectReturnType()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            Action sutCall = () => testController.SyncNotReturningActionResult();
            sutCall.Should().Throw<InvalidOperationException>().WithMessage("*ActionResult*");
        }

        [Test]
        public async Task ArgumentsWithoutAttributesAreSupported()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var complexObject = new ComplexObject()
            {
                Str = "foo",
                NestedComplexObject = new NestedComplexObject()
                {
                    Integer = 1,
                    Str = "br",
                },
            };
            var response = await testController.ArgumentsWithoutAttributes(complexObject,
                1,
                2);
            response.GetResult().ComplexObject.Should().BeEquivalentTo(complexObject);
            response.GetResult().FromQuery.Should().Be(2);
            response.GetResult().FromRoute.Should().Be(1);
        }

        [Test]
        public async Task AsyncCallWithResult()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = await testController.ReturnAsync();
            result.GetResult().Should().Be(10);
        }

        [Test]
        public async Task AsyncCallWithoutResult()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = await testController.BadRequestAsync();
            result.StatusCode().Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task AreasAreSupported()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<ControllerInArea>();
            var result = await testController.Index();
            result.IsSuccessStatusCode().Should().BeTrue();
        }

        [Test]
        public void ControllerMustHaveAllMethodsMarkedAsVirtual()
        {
            using var application = CreateApplication();
            Action call = () => application.ControllerFactory.CreateController<ControllerWithNonVirtualMethods>();
            call.Should()
                .Throw<InvalidOperationException>()
                .Where(x => x.Message.Contains(nameof(ControllerWithNonVirtualMethods)))
                .Where(x => x.Message.Contains(nameof(ControllerWithNonVirtualMethods.NonVirtual)))
                .Where(x => x.Message.Contains(nameof(ControllerWithNonVirtualMethods.NonVirtual2)))
                .Where(x => !x.Message.Contains(nameof(ControllerWithNonVirtualMethods.Index)));
        }

        [Test]
        public async Task MethodOverloadingIsSupported()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = await testController.OverloadedAction();
            result.IsSuccessStatusCode().Should().BeTrue();
        }

        [Test]
        public async Task SimpleArgumentsAreMapped()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = await testController.SimpleArguments(1, DateTime.Today, TestController.TestEnum.Zero, 100, DateTime.Today);
            result.GetResult().FromRoute.Should().Be(1);
            result.GetResult().Body.Should().Be(DateTime.Today);
            result.GetResult().FromQuery.Should().Be(TestController.TestEnum.Zero);
            result.GetResult().FromRoute2.Should().Be(100);
            result.GetResult().FromQuery2.Should().Be(DateTime.Today);
        }

        [Test]
        public async Task BodyCanContainComplexObject()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = await testController.ComplexBody(new TestController.ComplexArgument(
                integer: 10,
                str: "test",
                dateTime: DateTime.Today,
                innerObject: new TestController.InnerObject(str: "InnerStr")
            ));
            result.GetResult().Integer.Should().Be(10);
            result.GetResult().Str.Should().Be("test");
            result.GetResult().DateTime.Should().Be(DateTime.Today);
            result.GetResult().InnerObject!.Str.Should().Be("InnerStr");
        }

        [Test]
        public async Task FromQueryCanContainComplexObject()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = await testController.ComplexFromQuery(new TestController.ComplexArgument(
                integer: 10,
                str: "test",
                dateTime: DateTime.Today,
                innerObject: new TestController.InnerObject("test")
                {
                    List = new List<string>() { "a", "b" },
                }
            ));
            result.GetResult().Integer.Should().Be(10);
            result.GetResult().Str.Should().Be("test");
            result.GetResult().DateTime.Should().Be(DateTime.Today);
            result.GetResult().InnerObject!.Str.Should().Be("test");
            result.GetResult().InnerObject!.List.Should().ContainInOrder("a", "b");
        }

        [Test]
        public async Task FromFormIsSupported()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = await testController.FromForm(new TestController.ComplexArgument(
                integer: 10,
                str: "test",
                dateTime: DateTime.UtcNow.Date,
                innerObject: new TestController.InnerObject(str: "InnerStr")
            ));
            result.GetResult().Integer.Should().Be(10);
            result.GetResult().Str.Should().Be("test");
            result.GetResult().DateTime.ToString("dd/MM/yyyy").Should().Be(DateTime.UtcNow.ToString("dd/MM/yyyy"));
            result.GetResult().InnerObject!.Str.Should().Be("InnerStr");
        }

        [Test]
        public void FromHeaderWithComplexArgumentsIsNotSupported()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            Func<Task> sutCall = () => testController.FromHeader(new TestController.ComplexArgument(
                integer: 10,
                str: "test",
                dateTime: DateTime.Today,
                innerObject: null!
            ));
            sutCall.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public async Task FromHeaderIsSupportedForSimpleArguments()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = await testController.FromHeaderSimple(1);
            result.GetResult().Should().Be(1);
        }

        [Test]
        public async Task NameInFromQueryAttributeIsSupportedForComplexArgument()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = await testController.FromQueryWithNameComplexArgument(new TestController.Test() { Foo = 1 });
            result.GetResult().Foo.Should().Be(1);
        }

        [Test]
        public async Task NameInFromQueryAttributeIsSupportedSimpleArgument()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = await testController.FromQueryWithNameSimpleArgument(1);
            result.GetResult().Should().Be(1);
        }


        [Test]
        public void ArrayOfComplexArgumentsInFromQueryIsNotSupported()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            Func<Task> sutCall = () => testController.ArrayOfComplexArgumentsInFromQuery(new List<TestController.ComplexArgument>());
            sutCall.Should().Throw<InvalidOperationException>().WithMessage("*complex type*");
        }


        // This works in test but it would not work in real application
        // Test adds default values bud real app does not
        [Test]
        public async Task ObjectWithDefaultValuesInCtorDoesNotWorkWhenBindingUsingJsonNet()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = await testController.DefaultPropertiesInCtorTest(new ObjectWithDefaultProperties());
            result.GetResult().Str.Should().Be("test");
        }

        [Test]
        public async Task NullsCanBePlacedInFromQueryOrFromBodyOrFromHead()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = await testController.NullsTest(null, null, null, "asd");
            result.GetResult().Item1.Should().Be(null);
            result.GetResult().Item2.Should().Be(null);
            result.GetResult().Item3.Should().Be(null);
        }

        [Test]
        public async Task ArrayInFromQueryIsSupported()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = await testController.ArrayInFromQuery(new List<int>() { 1, 1, 1 });
            result.GetResult().Should().AllBeEquivalentTo(1);
        }


        [Test]
        public async Task FromQueryAndFromRouteCanNotHaveSameName()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            Func<Task> sutCall = () => testController.FromRouteFromQuerySameName("asd", "asd");
            sutCall.Should().Throw<InvalidOperationException>().WithMessage("*FromRoute*").WithMessage("*FromQuery*");
        }

        [Test]
        public void NullsCanNotBeInFromRouteArgument()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            Func<Task> sutCall = () => testController.NullsTest(1, new TestController.ComplexArgument(), DateTime.Now, null);
            sutCall.Should().Throw<InvalidOperationException>().WithMessage("*route*");
        }

        [Test]
        public async Task NameInFromRouteAttributeIsSupportedSimpleArgument()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = await testController.FromRouteWithNameSimpleArgument(1);
            result.GetResult().Should().Be(1);
        }

        [Test]
        public async Task ClassicalRoutingIsSupported()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<ControllerWithoutAttributeRouting>();
            var result = await testController.HttpGetWithoutBody();
            result.IsSuccessStatusCode().Should().BeTrue();
        }

        [Test]
        public async Task FromServicesIsIgnored()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = await testController.FromServices(null);
            result.GetResult().Should().BeTrue();
        }

        [Test]
        public async Task ArrayInBodyIsSupported()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
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
            var result = await testController.ArrayInBody(data);
            result.GetResult()
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
            application.ControllerFactory.AddHeader("foo", "foo");
            application.ControllerFactory.AddAuthorization(new AuthenticationHeaderValue("Bearer", "key"));
            application.ControllerFactory.AddHeaders(new Dictionary<string, string?>()
            {
                ["header1"] = "header",
                ["header2"] = "header2",
            });
            var testController = application.ControllerFactory.CreateController<TestController>();

            var result = await testController.MethodReturningHeaders();

            result.GetResult()["foo"].First().Should().Be("foo");
            result.GetResult()["header1"].First().Should().Be("header");
            result.GetResult()["header2"].First().Should().Be("header2");
            result.GetResult()["Authorization"].First().Should().Be("Bearer key");
        }

        [Test]
        public async Task HttpPostWithoutBody()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = await testController.HttpPostWithoutBody();
            result.IsSuccessStatusCode().Should().BeTrue();
        }

        [Test]
        public async Task HttpGetWithBody()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = await testController.HttpGetWithBody(5);
            result.GetResult().Should().Be(5);
        }

        [Test]
        public void ExceptionsAreCorrectlyRethrown()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            Func<Task> sutCall = () => testController.MethodThrowingInvalidOperationException();
            sutCall.Should().Throw<InvalidOperationException>().WithMessage("Correct");
        }

        [Test]
        public void When500IsReturnedNoExceptionIsThrown()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            Func<Task> sutCall = () => testController.MethodReturning500();
            sutCall.Should().NotThrow();
        }

        [Test]
        public async Task WhenActionReturnsIncorrectTypeDeserializationFails()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            ActionResult<int> response = await testController.MethodReturningBadRequestWithTypedResult();
            response.IsClientErrorStatusCode().Should().BeTrue();
            Action sutCall = () =>
            {
                _ = response.GetResult();
            };
            sutCall.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public async Task WhenActionReturnsIncorrectTypeDefaultValueIsUsed()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            ActionResult<string> response = await testController.MehodReturningControllerCallWithoutType();
            response.IsSuccessStatusCode().Should().BeTrue();
            response.GetResult().Should().Be(null);
        }


        [Test]
        public async Task ModelBinderIsSupported()
        {
            using var application = CreateApplication();
            application.ControllerFactory.AddHttpRequestPipelinePart(new ListSeparatedByCommasPipelinePart(new List<int>() { 1, 1, 1 }));
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = await testController.CustomBinder(null!);
            result.GetResult().Should().AllBeEquivalentTo(1);
        }

        [Test]
        public async Task PreModelBinderTest()
        {
            using var application = CreateApplication();
            application.ControllerFactory.AddActionInfoTransformer(new TestObjectActionInfoTransformer());
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = await testController.CustomBinderFullObject(new TestController.CountryCodeBinded() { CountryCode = "cz" });
            result.GetResult().Should().BeEquivalentTo("cz");
        }

        [Test]
        public void MethodsReturningVoidAreNotSupported()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            Func<Task> actionResult = () => testController.MethodReturningVoid();
            actionResult.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void AsyncMethodsReturningTaskAreNotSupported()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            Func<Task> actionResult = () => testController.MethodReturningTask();
            actionResult.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void MethodsReturningTaskAreNotSupported()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            Func<Task> actionResult = () => testController.MethodReturningTaskNotAsync();
            actionResult.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void MethodsReturningTaskWithIncorrectGenericType()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            Func<Task> actionResult = () => testController.MethodReturningIncorrectTypeInTask();
            actionResult.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public async Task HttpPatchWithBody()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var response = await testController.PatchWithBody(new ComplexObject() {Str = "test"});
            response.GetResult().Str.Should().Be("test");
        }

        public static Application CreateApplication()
        {
            var webAppFactory = new WebApplicationFactory<Startup>();
            var client = webAppFactory.CreateClient();
            return new Application(
                webAppFactory,
                new ControllerFactory(client, webAppFactory.Services, new NunitLogWriter())
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
                InvocationInfo invocationInfo)
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
                InvocationInfo invocationInfo)
            {
                var bindedObject = invocationInfo.Arguments.FirstOrDefault(x => x is TestController.CountryCodeBinded);
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
        public ControllerFactory ControllerFactory { get; set; }

        public Application(
            WebApplicationFactory<Startup> webApplicationFactory,
            ControllerFactory controllerFactory)
        {
            WebApplicationFactory = webApplicationFactory;
            ControllerFactory = controllerFactory;
        }

        public void Dispose()
        {
            WebApplicationFactory?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
