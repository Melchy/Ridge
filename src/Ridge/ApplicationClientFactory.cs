using Ridge.AspNetCore;
using System;
using System.Net.Http;

namespace Ridge;

internal class ApplicationClientFactory : IApplicationClientFactory
{
    public IApplicationClient CreateClient(
        IServiceProvider serviceProvider, 
        HttpClient httpClient)
    {
        return new ApplicationClient(httpClient, serviceProvider);
    }
}
