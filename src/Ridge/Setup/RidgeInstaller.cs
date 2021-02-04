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
        public static IServiceCollection AddRidge(this IServiceCollection services)
        {
            return services.AddTransient<IActionResultTypeMapper, ControllerResultTypeMapper>();
        }
        public static PageConventionCollection UseRidgePagesFilter(this PageConventionCollection pageConventionCollection)
        {
            return pageConventionCollection.ConfigureFilter(new PageResultFilter());
        }
        
        public static IApplicationBuilder UseRidgeImprovedExceptions(this IApplicationBuilder pageConventionCollection)
        {
            return  pageConventionCollection.UseMiddleware<ExceptionSavingMiddleware>();
        }
    }
}
