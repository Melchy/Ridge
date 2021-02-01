using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using Ridge.CallData;
using RidgeTests.PagesTests.Infrastructure;
using System;
using System.Threading.Tasks;
using TestWebApplication.Pages;

namespace RidgeTests.PagesTests
{
    public class RazorPagesTests
    {
        [Test]
        public void GetPageSimpleCase()
        {
            using var application = ApplicationBuilder.CreateApplication();
            var page = application.RazorPageFactory.CreateRazorPage<GeneralPageModel>();

            var response = page.OnGet();
            
            response.Model.Test.ParamFromBody.Should().Be("asd");
            response.Response.Should().Contain("body");
        }
        
        [Test]
        public async Task GetSpecificPageHandler()
        {
            using var application = ApplicationBuilder.CreateApplication();
            var page = application.RazorPageFactory.CreateRazorPage<GeneralPageModel>();

            var response = await page.OnGetFooStatic();
            
            response.Model.Test.ParamFromBody.Should().Be("Foo");
            response.Response.Should().Contain("body");
        }
        
        
        [Test]
        public void ExceptionIsPropagatedToTest()
        {
            using var application = ApplicationBuilder.CreateApplication();
            var page = application.RazorPageFactory.CreateRazorPage<GeneralPageModel>();

            Func<Task> call = () => page.OnGetThrowsInvalidOperationException();

            call.Should().Throw<InvalidOperationException>().WithMessage("Error");
        }
        
        [Test]
        public void FromBodyQueryRouteIsSupported()
        {
            using var application = ApplicationBuilder.CreateApplication();

            var page = application.RazorPageFactory.CreateRazorPage<GeneralPageModel>();
            
            var response = page.OnGetFoo2("returnValue", "testQuery");
            response.Model.Test.ParamFromBody.Should().Be("returnValue");
            response.Model.Test.ParamFromQuery.Should().Be("testQuery");
            response.Response.Should().Contain("body");
        }
        
        
        [Test]
        public async Task ResultsWithResponseCode4xxAreSupported()
        {
            using var application = ApplicationBuilder.CreateApplication();

            var page = application.RazorPageFactory.CreateRazorPage<GeneralPageModel>();

            var response = await page.OnGetBadRequest();
            response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            response.Response.Should().Be("Bad request error");
        }
        
        
        [Test]
        public async Task ResponsesWith3xxCodeAreSupported()
        {
            using var application = ApplicationBuilder.CreateApplication();
            
            var page = application.RazorPageFactory.CreateRazorPage<GeneralPageModel>();

            var response = await page.OnGetRedirect();
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Response.Should().Be("Redirected!!");
        }
        
        
        [Test]
        public async Task RouteParamsAreSupported()
        {
            using var application = ApplicationBuilder.CreateApplication();

            var page = application.RazorPageFactory.CreateRazorPage<PageWithRouteParameter>();
            
            var response = await page.OnGet("test");
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Response.Should().Be("test");
        }
        
        [Test]
        public async Task RequestReturning5xxButNotThrowingIsSupported()
        {
            using var application = ApplicationBuilder.CreateApplication();

            var page = application.RazorPageFactory.CreateRazorPage<GeneralPageModel>();
            
            var response = await page.OnGetReturn500();
            response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }
        
        [Test]
        [Parallelizable(ParallelScope.None)]
        public async Task WhenControllerCallerIsTurnedOffEverythingWorksNormally()
        {
            CallDataDictionary.Clear();
            using var application = ApplicationBuilder.CreateApplication();

            var response = await application.HttpClient.GetAsync($"{nameof(GeneralPageModel)}");

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("body");
            CallDataDictionary.IsEmpty().Should().BeTrue();
        }
        
        [Test]
        [Parallelizable(ParallelScope.None)]
        public async Task WhenControllerCallerIsTurnedOffMiddlewareIsNotHit()
        {
            CallDataDictionary.Clear();
            using var application = ApplicationBuilder.CreateApplication();

            var response = await application.HttpClient.GetAsync($"{nameof(GeneralPageModel)}?handler=ThrowsInvalidOperationException");

            response.IsSuccessStatusCode.Should().BeFalse();
            CallDataDictionary.IsEmpty().Should().BeTrue();
        }
        
    }
}

