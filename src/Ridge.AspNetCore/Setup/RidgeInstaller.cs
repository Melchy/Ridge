using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ridge.AspNetCore.ExceptionHandling;

// namespace is correct
namespace Microsoft.AspNetCore.Builder;

/// <summary>
///     Ridge installer.
/// </summary>
public static class RidgeInstaller
{
    /// <summary>
    ///     Adds middleware which saves exceptions.
    ///     This middleware allows ridge to rethrow exceptions instead of returning 5xx code.
    /// </summary>
    /// <param name="applicationBuilder"></param>
    /// <returns></returns>
    public static IApplicationBuilder RethrowExceptionInsteadOfReturningHttpResponse(
        this IApplicationBuilder applicationBuilder)
    {
        return applicationBuilder.UseMiddleware<ExceptionSavingMiddleware>();
    }

    /// <summary>
    ///     Check if application was created in test using
    ///     RidgeClient.
    /// </summary>
    /// <param name="app">Application</param>
    /// <returns>True if the app was created started from ridge client.</returns>
    public static bool WasApplicationCreatedFromTest(
        this IApplicationBuilder app)
    {
        var configuration = app.ApplicationServices.GetService<IConfiguration>();
        if (configuration?["Ridge:IsTestCall"] == "true")
        {
            return true;
        }

        return false;
    }
}
