using Ridge.HttpRequestFactoryMiddlewares;

namespace AlterRequestGeneration.Tests;

public class ApiVersionMiddleware : HttpRequestFactoryMiddleware
{
    public override async Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        // add parameter as query parameter
        requestFactoryContext.UrlGenerationParameters["api-version"] = "3";
        // or you can read the version from controller attribute
        // var controllerType = requestFactoryContext.CalledControllerMethodInfo.DeclaringType;
        // var apiVersion = controllerType?.GetCustomAttribute<ApiVersionAttribute>()?.Versions.FirstOrDefault();
        // if (apiVersion == null)
        // {
        //     throw new InvalidOperationException($"Api not found on controller '{controllerType?.FullName}' is missing.");
        // }
        //
        // requestFactoryContext.UrlGenerationParameters["api-version"] = apiVersion.ToString();
        
        return await base.CreateHttpRequest(requestFactoryContext);
    }
}
