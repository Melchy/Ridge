using Ridge.HttpRequestFactoryMiddlewares;
using Ridge.LogWriter;
using Ridge.Setup;
using System;

// Namespace is correct
namespace Microsoft.AspNetCore.Mvc.Testing;

/// <summary>
/// Exception rethrow options.
/// </summary>
public static class RidgeOptionsHttpRequestMiddlewareExtensions
{
    /// <summary>
    /// Adds <see cref="HttpRequestFactoryMiddleware"/> which is used to alter HttpRequest creation.
    /// Middlewares are applied in order in which they are added.
    /// </summary>
    /// <param name="ridgeOptions">Options to edit.</param>
    /// <param name="httpRequestFactoryMiddleware"></param>
    /// <returns></returns>
    public static RidgeOptions UseHttpRequestFactoryMiddleware(
        this RidgeOptions ridgeOptions,
        HttpRequestFactoryMiddleware httpRequestFactoryMiddleware)
    {
        ridgeOptions.HttpRequestFactoryMiddlewares.Add(httpRequestFactoryMiddleware);
        return ridgeOptions;
    }
}
