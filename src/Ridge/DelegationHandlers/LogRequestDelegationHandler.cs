using Ridge.LogWriter;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ridge.DelegationHandlers;

internal class LogRequestDelegationHandler : DelegatingHandler
{
    private readonly ILogWriter _logger;

    public LogRequestDelegationHandler(
        ILogWriter logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var requestBody = request.Content;
        var requestBodyAsString = requestBody == null ? null : await requestBody.ReadAsStringAsync();
        _logger.WriteLine("Request:");
        _logger.WriteLine($"Time when request was sent: {DateTime.Now.ToString("HH:mm:ss:fff")}");
        _logger.WriteLine($"{request}");
        _logger.WriteLine("Body:");
        _logger.WriteLine($"{requestBodyAsString}");
        _logger.WriteLine("");

        var response = await base.SendAsync(request, cancellationToken);

        var responseBodyAsString = await response.Content.ReadAsStringAsync();
        _logger.WriteLine("Response:");
        _logger.WriteLine($"Time when response was received: {DateTime.Now.ToString("HH:mm:ss:fff")}");
        _logger.WriteLine($"{response}");
        _logger.WriteLine("Body:");
        _logger.WriteLine($"{responseBodyAsString}");
        _logger.WriteLine("");

        return response;
    }
}
