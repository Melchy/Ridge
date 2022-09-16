#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ridge;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestWebApplication.Controllers.Examples
{
    [GenerateStronglyTypedCallerForTesting]
    public class ExamplesController : Controller
    {
        // Example controller
        // Method must be virtual
        [HttpGet("ReturnGivenNumber")]
        public virtual ActionResult<int> ReturnGivenNumber(
            [FromQuery] int input)
        {
            return input;
        }

        [HttpGet("ThrowException")]
        public virtual ActionResult ThrowException()
        {
            
            throw new InvalidOperationException("Exception throw");
        }
        
        // notice that you don't have to use endpoint routing
        // route for this action is defined in startup in following way
        // endpoints.MapControllerRoute(name: "complexExample", "{controller}/{action}/{fromRoute}");
        public virtual ActionResult<ResponseObject> ComplexExample(
            [FromQuery] ComplexObject complexObjectFromQuery,
            [FromQuery] List<string> listOfSimpleTypesFromQuery,
            [FromBody] List<ComplexObject> complexObjectsFromBody,
            [FromRoute] int fromRoute,
            // From services arguments are ignored and injected correctly by ASP.Net
            [FromServices] ExamplesController examplesController)
        {
            return new ResponseObject
            {
                FromRoute = fromRoute,
                ComplexObjectFromQuery = complexObjectFromQuery,
                ComplexObjectsFromBody = complexObjectsFromBody,
                ListOfSimpleTypesFromQuery = listOfSimpleTypesFromQuery,
            };
        }

        [HttpGet("customModelBinder/{thisIsBoundedUsingCustomBinder}")]
        public virtual ActionResult<string> CustomModelBinderExample(
            [ModelBinder(typeof(CountryCodeBinder))] string boundFromCustomModelBinder)
        {
            return boundFromCustomModelBinder;
        }

        [HttpGet("CallThatNeedsHeaders")]
        public virtual ActionResult CallThatNeedsHeaders()
        {
            return Ok();
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
        public Task BindModelAsync(
            ModelBindingContext bindingContext)
        {
            var str = bindingContext.ActionContext.HttpContext
                .Request.RouteValues["thisIsBoundedUsingCustomBinder"]!.ToString();
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
    }
}
#nullable enable
