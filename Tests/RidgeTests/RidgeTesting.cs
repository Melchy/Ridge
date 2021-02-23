using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Ridge.Interceptor;
using Ridge.Interceptor.InterceptorFactory;
using Ridge.Middlewares.Public;
using Ridge.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TestWebApplication;
using TestWebApplication.Controllers;

namespace RidgeTests
{
    public class RidgeTesting
    {
        [Test]
        public void SyncCallWithoutResult()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = testController.BadRequestAction();
            result.IsSuccessStatusCode.Should().BeFalse();
        }

        [Test]
        public void SyncCallWithResult()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = testController.Return10();
            result.Result.Should().Be(10);
        }

        [Test]
        public async Task AsyncCallWithResult()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = await testController.ReturnAsync();
            result.Result.Should().Be(10);
        }

        [Test]
        public async Task AsyncCallWithoutResult()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = await testController.BadRequestAsync();
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public void AreasAreSupported()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<ControllerInArea>();
            var result = testController.Index();
            result.IsSuccessStatusCode.Should().BeTrue();
        }
        
        [Test]
        public void ControllerMustHaveAllMethodsMarkedAsVirtual()
        {
            using var application = CreateApplication();
            Action call = () => application.ControllerFactory.CreateController<ControllerWithNonVirtualMethods>();
            call.Should().Throw<InvalidOperationException>()
                .Where(x=> x.Message.Contains(nameof(ControllerWithNonVirtualMethods)))
                .Where(x=> x.Message.Contains(nameof(ControllerWithNonVirtualMethods.NonVirtual)))
                .Where(x=> x.Message.Contains(nameof(ControllerWithNonVirtualMethods.NonVirtual2)))
                .Where(x=> !(x.Message.Contains(nameof(ControllerWithNonVirtualMethods.Index))));
        }
        
        [Test]
        public void MethodOverloadingIsSupported()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = testController.OverloadedAction();
            result.IsSuccessStatusCode.Should().BeTrue();
        }
        
        [Test]
        public void SimpleArgumentsAreMapped()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = testController.SimpleArguments(1,DateTime.Today, TestController.TestEnum.Zero, 100, DateTime.Today);
            result.Result.FromRoute.Should().Be(1);
            result.Result.Body.Should().Be(DateTime.Today);
            result.Result.FromQuery.Should().Be(TestController.TestEnum.Zero);
            result.Result.FromRoute2.Should().Be(100);
            result.Result.FromQuery2.Should().Be(DateTime.Today);
        }
        
        [Test]
        public void BodyCanContainComplexObject()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = testController.ComplexBody(new TestController.ComplexArgument
            (
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
        public void FromQueryCanContainComplexObject()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = testController.ComplexFromQuery(new TestController.ComplexArgument
            (
                integer: 10,
                str: "test",
                dateTime: DateTime.Today,
                innerObject: new TestController.InnerObject("test")
                {
                    List = new List<string>(){"a", "b"}
                }
            ));
            result.Result.Integer.Should().Be(10);
            result.Result.Str.Should().Be("test");
            result.Result.DateTime.Should().Be(DateTime.Today);
            result.Result.InnerObject!.Str.Should().Be("test");
            result.Result.InnerObject.List.Should().ContainInOrder("a", "b");
        }
        
        [Test]
        public void FromFormIsSupported()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = testController.FromForm(new TestController.ComplexArgument
            (
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
        public void FromHeaderWithComplexArgumentsIsNotSupported()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            Action sutCall = () =>  testController.FromHeader(new TestController.ComplexArgument(

                integer: 10,
                str: "test",
                dateTime: DateTime.Today,
                innerObject: null!
            ));
            sutCall.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void FromHeaderIsSupportedForSimpleArguments()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = testController.FromHeaderSimple(1);
            result.Result.Should().Be(1);
        }

        [Test]
        public void NameInFromQueryAttributeIsSupportedForComplexArgument()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = testController.FromQueryWithNameComplexArgument(new TestController.Test(){Foo = 1});
            result.Result.Foo.Should().Be(1);
        }

        [Test]
        public void NameInFromQueryAttributeIsSupportedSimpleArgument()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = testController.FromQueryWithNameSimpleArgument(1);
            result.Result.Should().Be(1);
        }


        [Test]
        public void ArrayOfComplexArgumentsInFromQueryIsNotSupported()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            Action sutCall = () => testController.ArrayOfComplexArgumentsInFromQuery(new List<TestController.ComplexArgument>());
            sutCall.Should().Throw<InvalidOperationException>().WithMessage("*complex type*");
        }


        // This works in test but it would not work in real application
        // Test adds default values bud real app does not
        [Test]
        public void ObjectWithDefaultValuesInCtorDoesNotWorkWhenBindingUsingJsonNet()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = testController.DefaultPropertiesInCtorTest(new ObjectWithDefaultProperties());
            result.Result.Str.Should().Be("test");
        }

        [Test]
        public void NullsCanBePlacedInFromQueryOrFromBodyOrFromHead()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = testController.NullsTest(null, null, null, "asd");
            result.Result.Item1.Should().Be(null);
            result.Result.Item2.Should().Be(null);
            result.Result.Item3.Should().Be(null);
        }

        [Test]
        public void ArrayInFromQueryIsSupported()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = testController.ArrayInFromQuery(new List<int>(){1,1,1});
            result.Result.Should().AllBeEquivalentTo(1);
        }


        [Test]
        public void FromQueryAndFromRouteCanNotHaveSameName()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            Action sutCall = () => testController.FromRouteFromQuerySameName("asd", "asd");
            sutCall.Should().Throw<InvalidOperationException>().WithMessage("*FromRoute*").WithMessage("*FromQuery*");
        }

        [Test]
        public void NullsCanNotBeInFromRouteArgument()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            Action sutCall = () => testController.NullsTest(1, new TestController.ComplexArgument(), DateTime.Now, null);
            sutCall.Should().Throw<InvalidOperationException>().WithMessage("*route*");
        }

        [Test]
        public void NameInFromRouteAttributeIsSupportedSimpleArgument()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = testController.FromRouteWithNameSimpleArgument(1);
            result.Result.Should().Be(1);
        }

        [Test]
        public async Task ClassicalRoutingIsSupported()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<ControllerWithoutAttributeRouting>();
            var result = await testController.HttpGetWithoutBody();
            result.IsSuccessStatusCode.Should().BeTrue();
        }
        
        [Test]
        public void FromServicesIsIgnored()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = testController.FromServices(null);
            result.Result.Should().BeTrue();
        }
        
        [Test]
        public void ArrayInBodyIsSupported()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var data = new List<TestController.ComplexArgument>
            {
                new TestController.ComplexArgument
                (
                    integer: 10,
                    str: "test",
                    dateTime: DateTime.Today
                ),
                new TestController.ComplexArgument
                (
                    integer: 100,
                    str: "testt",
                    dateTime: DateTime.Today
                ),
            };
            var result = testController.ArrayInBody(data);
            result.Result.Should().SatisfyRespectively(x =>
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
        public void HeadersCanBeAlteredUsingBuilder()
        {
            using var application = CreateApplication();
            application.ControllerFactory.AddHeader("foo", "foo");
            application.ControllerFactory.AddAuthorization(new AuthenticationHeaderValue("Bearer","key"));
            application.ControllerFactory.AddHeaders(new Dictionary<string, string>()
            {
                ["header1"] = "header",
                ["header2"] = "header2",
            });
            var testController = application.ControllerFactory.CreateController<TestController>();
            
            var result = testController.MethodReturningHeaders();

            result.Result["foo"].Should().Be("foo");
            result.Result["header1"].Should().Be("header");
            result.Result["header2"].Should().Be("header2");
            result.Result["Authorization"].Should().Be("Bearer key");
        }

        [Test]
        public async Task HttpPostWithoutBody()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = await testController.HttpPostWithoutBody();
            result.IsSuccessStatusCode.Should().BeTrue();
        }
        
        [Test]
        public async Task HttpGetWithBody()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = await testController.HttpGetWithBody(5);
            result.Result.Should().Be(5);
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
        public void WhenActionReturnsIncorrectTypeDeserializationFails()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            ControllerResult<int> response = testController.MethodReturningBadRequestWithTypedResult();
            response.IsClientErrorStatusCode.Should().BeTrue();
            Action sutCall = () =>
            {
                var foo = response.Result;
            };
            sutCall.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void WhenActionReturnsIncorrectTypeDefaultValueIsUsed()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            ControllerResult<string> response = testController.MehodReturningControllerCallWithoutType();
            response.IsSuccessStatusCode.Should().BeTrue();
            response.Result.Should().Be(null);
        }

        [Test]
        public void MethodMustReturnControllerResult()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            Action sutCall = () =>  testController.MethodNotReturningControllerResult();
            sutCall.Should().Throw<InvalidOperationException>().WithMessage("*ControllerResult*");
        }


        [Test]
        public void ModelBinderIsSupported()
        {
            using var application = CreateApplication();
            application.ControllerFactory.AddCallMiddleware(new ListSeparatedByCommasMiddleware(new List<int>(){1,1,1}));
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = testController.CustomBinder(null!);
            result.Result.Should().AllBeEquivalentTo(1);
        }
        
        [Test]
        public void PreModelBinderTest()
        {
            using var application = CreateApplication();
            application.ControllerFactory.AddPreCallMiddleware(new TestObjectMiddleware());
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = testController.CustomBinderFullObject(new TestController.CountryCodeBinded(){CountryCode = "cz"});
            result.Result.Should().BeEquivalentTo("cz");
        }

        [Test]
        public void MethodsReturningVoidAreNotSupported()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            Action actionResult = () => testController.MethodReturningVoid();
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


        public class ListSeparatedByCommasMiddleware : CallMiddleware
        {
            private readonly IEnumerable<int> _data;

            public ListSeparatedByCommasMiddleware(IEnumerable<int> data)
            {
                _data = data;
            }

            public override Task<HttpResponseMessage> Invoke(
                CallMiddlewareDelegate next,
                HttpRequestMessage httpRequestMessage,
                IReadOnlyInvocationInformation invocationInformation)
            {
                httpRequestMessage.RequestUri = new Uri(
                    QueryHelpers.AddQueryString(httpRequestMessage.RequestUri.ToString(), "properties", $"{string.Join(",", _data)}"),
                    UriKind.Relative);
                return next();
            }
        }
        
        public class TestObjectMiddleware : PreCallMiddleware
        {
            public override Task Invoke(
                PreCallMiddlewareDelegate next,
                IInvocationInformation invocationInformation)
            {
                var bindedObject = invocationInformation.Arguments.FirstOrDefault(x => x is TestController.CountryCodeBinded);
                if (bindedObject == null)
                {
                    return next();
                }

                invocationInformation.RouteParams["countryCode"] = ((TestController.CountryCodeBinded)bindedObject).CountryCode;
                return Task.CompletedTask;
            }
        }

        public static Application CreateApplication()
        {
            var webAppFactory = new WebApplicationFactory<Startup>();
            var client = webAppFactory.CreateClient();

            return new Application
            (
                webAppFactory,
                new ControllerFactory(client, webAppFactory.Services)
            );
        }
    }

    public sealed class Application : IDisposable
    {
        public Application(
            WebApplicationFactory<Startup> webApplicationFactory,
            ControllerFactory controllerFactory)
        {
            WebApplicationFactory = webApplicationFactory;
            ControllerFactory = controllerFactory;
        }

        public WebApplicationFactory<Startup> WebApplicationFactory { get; set; }
        public ControllerFactory ControllerFactory { get; set; }
        
        public void Dispose()
        {
            WebApplicationFactory?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
