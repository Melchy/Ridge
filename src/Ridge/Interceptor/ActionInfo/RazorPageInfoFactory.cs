using CommonExtensionMethods;
using FluentReflections;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Ridge.Interceptor.ActionInfo.Dtos;
using Ridge.Interceptor.InterceptorFactory;
using Ridge.Middlewares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("RidgeTests")]

namespace Ridge.Interceptor.ActionInfo
{
    internal class RazorPageInfoFactory : IGetInfo
    {
        private readonly IServiceProvider _serviceProvider;

        public RazorPageInfoFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public async Task<ActionInfoDto> GetInfo<TPage>(
            IEnumerable<object> arguments,
            MethodInfo methodInfo,
            PreCallMiddlewareCaller preCallMiddlewareCaller)
        {
            var dictionaryOfUrlsAndTuplesContainingRelativePathAndArea = GetDictionaryOfPagePathsAndTuplesContainingRelativePathAndArea();
            var viewDescriptor = GetViewDescriptor<TPage>();
            var pagePathAndArea = dictionaryOfUrlsAndTuplesContainingRelativePathAndArea[viewDescriptor.RelativePath];
            var pageHandlerModel = GetPageHandlerModel<TPage>(methodInfo);
            var actionArgumentInfo = ActionArgumentsInfo.CreateActionInfo(arguments, methodInfo);
            if (pagePathAndArea.area != null)
            {
                actionArgumentInfo.AddArea(pagePathAndArea.area);
            }

            await preCallMiddlewareCaller.Call(actionArgumentInfo);
            var linkToPage = GetLinkToPage(pagePathAndArea, actionArgumentInfo, pageHandlerModel);
            return new ActionInfoDto(linkToPage,
                    pageHandlerModel.HttpMethod,
                    actionArgumentInfo);
        }

        private string GetLinkToPage(
            (string pagePath, string? area) pagePathAndArea, ActionArgumentsInfo actionArgumentsInfo, PageHandlerModel pageHandlerModel)
        {
            var linkGenerator = _serviceProvider.GetService<LinkGenerator>();
            var linkToPage = linkGenerator.GetPathByPage(pagePathAndArea.pagePath,
                values: actionArgumentsInfo.RouteParams,
                handler: pageHandlerModel.HandlerName);
            if (linkToPage == null)
            {
                throw new InvalidOperationException($"Could not create link to page. Tested values: {Environment.NewLine}" +
                                                    $"Page path: {pagePathAndArea.pagePath}, RouteParams: {JsonConvert.SerializeObject(actionArgumentsInfo.RouteParams)}," +
                                                    $"handler name {pageHandlerModel.HandlerName}");
            }

            return linkToPage;
        }

        private PageHandlerModel GetPageHandlerModel<TPage>(
            MethodInfo methodInfo)
        {
            var pageApplicationModelPartsProvider = _serviceProvider.GetService<IPageApplicationModelPartsProvider>();
            var pageHandlerModel = pageApplicationModelPartsProvider.CreateHandlerModel(methodInfo);
            if (pageHandlerModel == null)
            {
                throw new InvalidOperationException($"Method {methodInfo.Name} in class {typeof(TPage)} is not valid handler.");
            }
            return pageHandlerModel;
        }

        private CompiledViewDescriptor GetViewDescriptor<TPage>()
        {
            var viewsFeature = GetViewFeature(_serviceProvider);
            var modelTypesWithViewDescriptors = viewsFeature.ViewDescriptors.Select(compiledViewDescriptor =>
            {
                var modelType = compiledViewDescriptor.Type.Reflection()
                    .GetPropertyOrField("Model").GetType();
                return (modelType, compiledViewDescriptor);
            });
            var pageType = typeof(TPage);
            var viewDescriptor = modelTypesWithViewDescriptors
                .First(x => x.modelType.Type == pageType)
                .compiledViewDescriptor;
            return viewDescriptor;
        }

        private Dictionary<string, (string pagePath, string? area)> GetDictionaryOfPagePathsAndTuplesContainingRelativePathAndArea()
        {
            var routeModelProviders = _serviceProvider.GetService<IEnumerable<IPageRouteModelProvider>>().ToArray();
            var context = new PageRouteModelProviderContext();
            for (var i = 0; i < routeModelProviders.Length; i++)
            {
                routeModelProviders[i].OnProvidersExecuting(context);
            }
            var result =
                context.RouteModels.ToDictionary(x => x.RelativePath, x => GetUrlFromRouteValues(x.RouteValues));
            return result;
        }
        
        private static (string pagePath, string? area) GetUrlFromRouteValues(IDictionary<string, string> routerValues)
        {
            var resultUrl = routerValues["page"];
            var area = routerValues.GetOrDefault("area");
            return (resultUrl, area);
        }

        private static ViewsFeature GetViewFeature(IServiceProvider serviceProvider)
        {
            var applicationPartManager = serviceProvider.GetService<ApplicationPartManager>();
            var viewsFeature = new ViewsFeature();
            applicationPartManager.PopulateFeature(viewsFeature);
            return viewsFeature;
        }
    }
}
