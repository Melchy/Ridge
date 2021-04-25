using Microsoft.AspNetCore.Mvc.Testing;
using Ridge.Interceptor.InterceptorFactory;
using System;
using System.Diagnostics;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using TestWebApplication;
using TestWebApplication.Controllers;
using Xunit;
using Xunit.Abstractions;

namespace RidgeXunitTest
{
    public class ManyTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ManyTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task CallAsync()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            await testController.ReturnAsync();
        }

        [Fact]
        public async Task CallAsync2()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            await testController.ReturnAsync();
        }

        [Fact]
        public async Task CallAsync3()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            await testController.ReturnAsync();
        }

        [Fact]
        public async Task CallAsync4()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            await testController.ReturnAsync();
        }

        [Fact]
        public void Test1()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test2()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test3()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test4()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test5()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test6()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test7()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test8()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test9()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test10()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test11()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test12()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test13()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test14()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test15()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test16()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test17()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test18()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test19()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test20()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test21()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test22()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test23()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test24()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test25()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test26()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test27()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test28()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test29()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test30()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test31()
        {
            CallSyncMethod();
        }

        [Fact]
        public void Test32()
        {
            CallSyncMethod();
        }

        private void CallSyncMethod()
        {
            using var application = CreateApplication();
            var testController = application.ControllerFactory.CreateController<TestController>();
            var result = testController.ReturnSyncWithResult();
        }

        public Task DisposeAsync() => Task.FromResult(0);

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
}
