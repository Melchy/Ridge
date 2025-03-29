using System;
using System.Net.Http;

namespace Ridge.AspNetCore;

/// <summary>
/// This class is used as extension point for the source generator whe the option Ridge_GenerateEndpointCallsAsExtensionMethods is enabled.
/// The generator will generate extension methods for this class. You only have to instantiate it and pass in the
/// service provider and http client from WebApplicationFactory
/// </summary>
public class ApiClient
{
    /// <summary>
    /// Service provider
    /// </summary>
    public IServiceProvider ServiceProvider { get; }
    
    /// <summary>
    /// Http client
    /// </summary>
    public HttpClient HttpClient { get; }

    /// <summary>
    /// Creates instance of api client.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="httpClient"></param>
    public ApiClient(
        HttpClient httpClient,
        IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        HttpClient = httpClient;
    }
}
