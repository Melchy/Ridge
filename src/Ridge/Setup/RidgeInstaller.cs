using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ridge.ExceptionHandling;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TestWebApplication")]
[assembly: InternalsVisibleTo("RidgeTests")]

namespace Ridge.Setup;

/// <summary>
///     Ridge installer.
/// </summary>
public static class RidgeInstaller
{
    internal static bool WasInstallerUsed { get; set; }
    
    /// <summary>
    ///     Adds middleware which saves exceptions when application is called from test using ridge.
    ///     This middleware allows ridge to rethrow exceptions instead of returning 5xx code.
    /// </summary>
    /// <param name="applicationBuilder"></param>
    /// <returns></returns>
    public static IApplicationBuilder ThrowExceptionInsteadOfReturning500(
        this IApplicationBuilder applicationBuilder)
    {
        WasInstallerUsed = true;
        return applicationBuilder.UseMiddleware<ExceptionSavingMiddleware>();
    }

    /// <summary>
    ///     Check if application was created in test using
    ///     RidgeClient.
    /// </summary>
    /// <param name="app">Application</param>
    /// <returns>True if the app was created started from ridge client.</returns>
    public static bool WasApplicationCalledFromTestClient(
        this IApplicationBuilder app)
    {
        WasInstallerUsed = true;
        var configuration = app.ApplicationServices.GetService<IConfiguration>();
        if (configuration?["Ridge:IsTestCall"] == "true")
        {
            return true;
        }

        return false;
    }
}
