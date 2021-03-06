﻿using Newtonsoft.Json;
using Ridge.Interceptor;
using Ridge.LogWriter;
using Ridge.Pipeline.Public;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Pipeline.Infrastructure
{
    internal class CallWebAppPipelinePart : IHttpRequestPipelinePart
    {
        private readonly HttpClient _httpClient;
        private readonly ILogWriter? _logger;

        public CallWebAppPipelinePart(
            HttpClient httpClient,
            ILogWriter? logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<HttpResponseMessage> InvokeAsync(
            Func<Task<HttpResponseMessage>> next,
            HttpRequestMessage httpRequestMessage,
            IReadOnlyActionInfo actionInfo,
            InvocationInfo invocationInfo)
        {
            if (_logger != null)
            {
                var body = httpRequestMessage.Content;
                var bodyAsString = body == null ? null : await httpRequestMessage.Content.ReadAsStringAsync();
                _logger.WriteLine("Request:");
                _logger.WriteLine($"{httpRequestMessage}");
                _logger.WriteLine("Body:");
                _logger.WriteLine($"{bodyAsString}");
                _logger.WriteLine("");
            }

            var response = await _httpClient.SendAsync(httpRequestMessage);

            if (_logger != null)
            {
                var body = response.Content;
                var bodyAsString = body == null ? null : await response.Content.ReadAsStringAsync();
                _logger.WriteLine("Response:");
                _logger.WriteLine($"{response}");
                _logger.WriteLine("Body:");
                _logger.WriteLine($"{bodyAsString}");
                _logger.WriteLine("");
            }

            return response;
        }
    }
}
