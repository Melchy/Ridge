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
        var response = await application.TestControllerClient.CallMethodReturningBody();
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
        public TestControllerClient TestControllerClient { get; }
        public CustomLogger CustomLogger = new();

        public Application(
            WebApplicationFactory<Program> ridgeApplicationFactory)
        {
            RidgeApplicationFactory = ridgeApplicationFactory.WithRidge(x =>
            {
                x.LogWriter = CustomLogger;
                x.ThrowExceptionInsteadOfReturning500 = true;
            });
            var ridgeHttpClient = RidgeApplicationFactory.CreateClient();
            TestControllerClient = new TestControllerClient(ridgeHttpClient, RidgeApplicationFactory.Services);
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
