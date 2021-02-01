using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Ridge.Filters;

namespace Ridge.Setup
{
    public static class RidgeInstaller
    {
        public static PageConventionCollection UseRidgePagesFilter(this PageConventionCollection pageConventionCollection)
        {
            return pageConventionCollection.ConfigureFilter(new PageResultFilter());
        }
        
        public static IApplicationBuilder UseRidgeMiddleware(this IApplicationBuilder pageConventionCollection)
        {
            return  pageConventionCollection.UseMiddleware<ExceptionSavingMiddleware>();
        }

        public static FilterCollection UseRidgeControllerFilter(this FilterCollection filterCollection)
        {
            filterCollection.Add(new ControllerResultFilter());
            return filterCollection;
        }
    }


}
