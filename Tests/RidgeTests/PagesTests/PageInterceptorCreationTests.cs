using FluentAssertions;
using NUnit.Framework;
using RidgeTests.PagesTests.Infrastructure;
using System;
using TestWebApplication.Pages;

namespace RidgeTests.PagesTests
{
    public class PageInterceptorCreationTests
    {
        [Test]
        public void InterceptorCanBeCreated()
        {
            using var application = ApplicationBuilder.CreateApplication();

            Action sutCall = () => application.RazorPageFactory.CreateRazorPage<PageWithRouteParameter>();
            
            sutCall.Should().NotThrow<Exception>();
        }
        
        [Test]
        public void PageWithNonVirtualActionCanNotBeIntercepted()
        {
            using var application = ApplicationBuilder.CreateApplication();

            Action sutCall = () => application.RazorPageFactory.CreateRazorPage<PageWithMultipleNonVirtualMethod>();
            
            sutCall.Should().Throw<InvalidOperationException>()
                .And.Message.Should().Contain("virtual")
                .And.Contain($"{nameof(PageWithMultipleNonVirtualMethod.OnPostNonVirtual)}")
                .And.Contain($"{nameof(PageWithMultipleNonVirtualMethod.OnGetNonVirtual)}")
                .And.NotContain($"{nameof(PageWithMultipleNonVirtualMethod.OnGetSomething)}");
        }

        [Test]
        public void GenericTypeOfCustomActionResultMustMatchPageType()
        {
            using var application = ApplicationBuilder.CreateApplication();

            Action sutCall = () => application.RazorPageFactory.CreateRazorPage<PageWithIncorrectReturnTypeInCustomActionResult>();

            sutCall.Should().Throw<InvalidOperationException>()
                .And.Message.Should().NotContain("virtual")
                .And.Contain($"{nameof(PageWithIncorrectReturnTypeInCustomActionResult.OnGetGenericTypeOfCustomActionResultIsWrong)}")
                .And.Contain($"{nameof(PageWithIncorrectReturnTypeInCustomActionResult.OnGetIncorrectToo)}");
        }
        
        [Test]
        public void AllActionsMustReturnCustomActionResult()
        {
            using var application = ApplicationBuilder.CreateApplication();

            Action sutCall = () => application.RazorPageFactory.CreateRazorPage<PageWithIncorrectReturnType>();
            

            sutCall.Should().Throw<InvalidOperationException>()
                .And.Message.Should().NotContain("virtual")
                .And.Contain($"{nameof(PageWithIncorrectReturnType.OnGetVoid)}")
                .And.Contain($"{nameof(PageWithIncorrectReturnType.OnGetInt)}");
        }
    }
}
