using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Ridge.Filters;

namespace Ridge.Setup
{
    public static class RidgeInstaller
    {
        /// <summary>
        /// This method registers class which fixes swagger return types.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddRidge(this IServiceCollection services)
        {
            return services.AddTransient<IActionResultTypeMapper, ControllerResultTypeMapper>();
        }
        /// <summary>
        /// Adds filter which transforms PageResults to IActionResults.
        /// </summary>
        /// <param name="pageConventionCollection"></param>
        /// <returns></returns>
        public static PageConventionCollection UseRidgePagesFilter(this PageConventionCollection pageConventionCollection)
        {
            return pageConventionCollection.ConfigureFilter(new PageResultFilter());
        }

        /// <summary>
        /// Adds middleware which saves exceptions when application is called from test using ridge.
        /// This middleware allows ridge to rethrow exceptions instead of returning 5xx code.
        /// </summary>
        /// <param name="pageConventionCollection"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseRidgeImprovedExceptions(this IApplicationBuilder pageConventionCollection)
        {
            return pageConventionCollection.UseMiddleware<ExceptionSavingMiddleware>();
        }
    }
}
