using Microsoft.AspNetCore.Mvc.Testing;
using Ridge.Interceptor.InterceptorFactory;
using TestWebApplication;

namespace RidgeTests.PagesTests.Infrastructure
{
    public static class ApplicationBuilder
    {
        public static PagesApplication CreateApplication()
        {
            var webApp = new WebApplicationFactory<Startup>();
            var httpClient = webApp.CreateClient();
            var razorPagesFactory = new RazorPageFactory(webApp.CreateClient(), webApp.Services);
            return new PagesApplication(webApp, razorPagesFactory, httpClient);
        }
    }
}
