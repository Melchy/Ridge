using Microsoft.Extensions.Options;
using Ridge.Setup;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.LogWriter.Internal;

internal class RidgeLogger
{
    private readonly CompositeLogWriter? _logger;

    public RidgeLogger(
        CompositeLogWriter? compositeLogWriter)
    {
        _logger = compositeLogWriter;
    }

    public async Task LogRequest(
        HttpRequestMessage request)
    {
        if (_logger == null)
        {
            return;
        }
        
        var requestBody = request.Content;
        var requestBodyAsString = requestBody == null ? null : await requestBody.ReadAsStringAsync();
        _logger.WriteLine("Request:");
        _logger.WriteLine($"Time when request was sent: {DateTime.Now.ToString("HH:mm:ss:fff")}");
        _logger.WriteLine($"{request}");
        _logger.WriteLine("Body:");
        _logger.WriteLine($"{requestBodyAsString}");
        _logger.WriteLine("");
    }

    public async Task LogResponse(
        HttpResponseMessage response)
    {
        if (_logger == null)
        {
            return;
        }
        
        var responseBodyAsString = await response.Content.ReadAsStringAsync();
        _logger.WriteLine("Response:");
        _logger.WriteLine($"Time when response was received: {DateTime.Now.ToString("HH:mm:ss:fff")}");
        _logger.WriteLine($"{response}");
        _logger.WriteLine("Body:");
        _logger.WriteLine($"{responseBodyAsString}");
        _logger.WriteLine("");
    }
}
