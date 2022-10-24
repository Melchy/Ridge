using Microsoft.AspNetCore.Builder;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TestWebApplication")]
[assembly: InternalsVisibleTo("RidgeTests")]

namespace Ridge.Setup
{
    /// <summary>
    ///     Ridge installer.
    /// </summary>
    public static class RidgeInstaller
    {
        /// <summary>
        ///     Adds middleware which saves exceptions when application is called from test using ridge.
        ///     This middleware allows ridge to rethrow exceptions instead of returning 5xx code.
        /// </summary>
        /// <param name="applicationBuilder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseRidgeImprovedExceptions(
            this IApplicationBuilder applicationBuilder)
        {
            return applicationBuilder.UseMiddleware<ExceptionSavingMiddleware>();
        }
    }
}
