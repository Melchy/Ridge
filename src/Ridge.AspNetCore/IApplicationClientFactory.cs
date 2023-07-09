using System;
using System.Net.Http;

namespace Ridge.AspNetCore;

/// <summary>
///    Factory used to create ApplicationClient.
/// </summary>
public interface IApplicationClientFactory
{
    /// <summary>
    /// Creates instance of IApplicationClient.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="httpClient"></param>
    /// <returns></returns>
    IApplicationClient CreateClient(
        IServiceProvider serviceProvider, 
        HttpClient httpClient);
}
