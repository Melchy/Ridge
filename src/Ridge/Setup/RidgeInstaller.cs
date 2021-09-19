using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using Ridge.Filters;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TestWebApplication")]

namespace Ridge.Setup
{
    /// <summary>
    ///     Ridge installer.
    /// </summary>
    public static class RidgeInstaller
    {
        /// <summary>
        ///     Adds filter which transforms PageResults to IActionResults.
        /// </summary>
        /// <param name="pageConventionCollection"></param>
        /// <returns></returns>
        internal static PageConventionCollection UseRidgePagesFilter(
            this PageConventionCollection pageConventionCollection)
        {
            return pageConventionCollection.ConfigureFilter(new PageResultFilter());
        }

        /// <summary>
        ///     Adds middleware which saves exceptions when application is called from test using ridge.
        ///     This middleware allows ridge to rethrow exceptions instead of returning 5xx code.
        /// </summary>
        /// <param name="pageConventionCollection"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseRidgeImprovedExceptions(
            this IApplicationBuilder pageConventionCollection)
        {
            return pageConventionCollection.UseMiddleware<ExceptionSavingMiddleware>();
        }
    }
}
