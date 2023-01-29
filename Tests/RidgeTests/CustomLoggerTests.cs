using FluentAssertions;
using NUnit.Framework;
using Ridge;
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
        var webAppFactory = new RidgeApplicationFactory<Program>();
        return new Application(
            webAppFactory
        );
    }

    internal sealed class Application : IDisposable
    {
        public RidgeApplicationFactory<Program> RidgeApplicationFactory { get; set; }
        public TestControllerCaller TestControllerCaller { get; }
        public CustomLogger CustomLogger = new();

        public Application(
            RidgeApplicationFactory<Program> ridgeApplicationFactory)
        {
            RidgeApplicationFactory = ridgeApplicationFactory.AddCustomLogger(CustomLogger);
            var ridgeHttpClient = ridgeApplicationFactory.CreateRidgeClient();
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
