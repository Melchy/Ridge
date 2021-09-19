using Microsoft.AspNetCore.Mvc.Testing;
using Ridge.Interceptor.InterceptorFactory;
using System;
using System.Net.Http;
using TestWebApplication;

namespace RidgeTests.PagesTests.Infrastructure
{
    public sealed class PagesApplication : IDisposable
    {
        public WebApplicationFactory<Startup> WebApplicationFactory { get; }
        public RazorPageFactory RazorPageFactory { get; }
        public HttpClient HttpClient { get; }

        public PagesApplication(
            WebApplicationFactory<Startup> webApplicationFactory,
            RazorPageFactory razorPageFactory,
            HttpClient httpClient)
        {
            WebApplicationFactory = webApplicationFactory;
            RazorPageFactory = razorPageFactory;
            HttpClient = httpClient;
        }

        public void Dispose()
        {
            WebApplicationFactory?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
