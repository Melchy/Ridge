using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Ridge.LogWriter;
using System;
using System.Threading.Tasks;
using TestWebApplication;
using TestWebApplication.Controllers;

namespace RidgeTests;

public class CustomLoggerTests
{
    [Test]
    public async Task CustomLogger()
    {
        using var application = CreateApplication();
        var response = await application.TestControllerCaller.CallMethodReturningBody();
        application.CustomLogger.LoggedMessage.Should().Contain("Request").And.Contain("Response");
    }

    internal static Application CreateApplication()
    {
        var webAppFactory = new WebApplicationFactory<Program>();
        return new Application(
            webAppFactory
        );
    }

    internal sealed class Application : IDisposable
    {
        public WebApplicationFactory<Program> RidgeApplicationFactory { get; set; }
        public TestControllerCaller TestControllerCaller { get; }
        public CustomLogger CustomLogger = new();

        public Application(
            WebApplicationFactory<Program> ridgeApplicationFactory)
        {
            RidgeApplicationFactory = ridgeApplicationFactory.AddCustomLogger(CustomLogger).AddExceptionCatching();
            var ridgeHttpClient = RidgeApplicationFactory.CreateRidgeClient();
            TestControllerCaller = new TestControllerCaller(ridgeHttpClient);
        }

        public void Dispose()
        {
            RidgeApplicationFactory?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

public class CustomLogger : ILogWriter
{
    public string LoggedMessage { get; private set; } = "";

    public void WriteLine(
        string text)
    {
        LoggedMessage += text;
    }
}
