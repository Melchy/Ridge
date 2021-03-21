using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ridge.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace TestWebApplication.Controllers.Examples
{
    public class ExamplesController
    {
        // Example controller
        // Notice the ControllerResult instead of standard ActionResult
        [HttpGet("ReturnGivenNumber")]
        public virtual async Task<ControllerResult<int>> ReturnGivenNumber(
            [FromQuery] int input)
        {
            return input;
        }

        // notice that you do not have to use endpoint routing
        // route for this action is defined in startup in following way
        // endpoints.MapControllerRoute(name: "complexExample", "{controller}/{action}/{fromRoute}/{boundFromCustomModelBinder}");
        public virtual async Task<ControllerResult<ResponseObject>> ComplexExample(
            [FromQuery] ComplexObject complexObjectFromQuery,
            [FromQuery] List<string> listOfSimpleTypesFromQuery,
            [FromBody] List<ComplexObject> complexObjectsFromBody,
            [FromRoute] int fromRoute,
            // custom model binder are supported by using request transformers
            [ModelBinder(BinderType = typeof(CountryCodeBinder))] string customModelBinder,
            // From services arguments are ignored and injected correctly by ASP.Net
            [FromServices] ExamplesController examplesController
            )
        {
            return new ResponseObject
            {
                FromRoute = fromRoute,
                CustomModelBinder = customModelBinder,
                ComplexObjectFromQuery = complexObjectFromQuery,
                ComplexObjectsFromBody = complexObjectsFromBody,
                ListOfSimpleTypesFromQuery = listOfSimpleTypesFromQuery
            };
        }
    }

    public class ComplexObject
    {
        public string Str { get; set; }
        public NestedComplexObject NestedComplexObject { get; set; }

    }

    public class NestedComplexObject
    {
        public string Str { get; set; }
        public int Integer { get; set; }
    }

    public class CountryCodeBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var str = bindingContext.ActionContext.HttpContext
                .Request.RouteValues["boundFromCustomModelBinder"]!.ToString();
            bindingContext.Result = ModelBindingResult.Success(str);
            return Task.CompletedTask;
        }
    }

    public class ResponseObject
    {
        public ComplexObject ComplexObjectFromQuery { get; set; }
        public List<string> ListOfSimpleTypesFromQuery { get; set; }
        public List<ComplexObject> ComplexObjectsFromBody { get; set; }
        public int FromRoute { get; set; }
        public string CustomModelBinder { get; set; }
    }
}
#nullable enable
