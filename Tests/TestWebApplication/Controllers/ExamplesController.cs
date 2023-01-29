#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ridge.GeneratorAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApplication.Controllers;

[GenerateCaller]
[AddParameterToCaller(typeof(string), "GeneratedParameter", ParameterMapping.MapToQueryOrRouteParameter, Optional = true)]
[TransformParameterInCaller(fromType: typeof(CountryCode), toType: typeof(string), ParameterMapping.MapToQueryOrRouteParameter)]
public class ExamplesController : Controller
{
    [HttpGet("ReadQueryParameterFromHttpContext")]
    public async Task<string> ReadQueryParameterFromHttpContext()
    {
        return HttpContext.Request.Query["GeneratedParameter"];
    }


    private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

    public ExamplesController(
        IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
    {
        _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
    }

    [HttpGet("ReturnGivenNumber")]
    public ActionResult<int> ReturnGivenNumber(
        [FromQuery] int input)
    {
        return input;
    }

    [HttpGet("ReturnAllHeaders")]
    public ActionResult<IEnumerable<(string key, string value)>> ReturnAllHeaders()
    {
        return new ActionResult<IEnumerable<(string key, string value)>>(HttpContext.Request.Headers.Select(x => (x.Key, x.Value.FirstOrDefault())));
    }

    [HttpGet("ThrowException")]
    public ActionResult ThrowException()
    {
        throw new InvalidOperationException("Exception throw");
    }

    // notice that you don't have to use endpoint routing
    // route for this action is defined in startup in following way
    // endpoints.MapControllerRoute(name: "complexExample", "{controller}/{action}/{fromRoute}");
    public ActionResult<ResponseObject> ComplexExample(
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

    [HttpGet("ReturnHeader")]
    public async Task<string> ReturnHeader(
        string headerName)
    {
        return HttpContext.Request.Headers[headerName];
    }
    
    [HttpGet("CallThatNeedsHeaders")]
    public ActionResult CallThatNeedsHeaders()
    {
        return Ok();
    }

    [HttpGet("CallWithCustomModelBinder")]
    public ActionResult<string> WithCustomModelBinder(
        [ModelBinder(typeof(BindCountryCodeFromQueryOrHeader))] CountryCode countryCode)
    {
        return countryCode.Value;
    }

    public class BindCountryCodeFromQueryOrHeader : IModelBinder
    {
        public Task BindModelAsync(
            ModelBindingContext bindingContext)
        {
            // Get value from query or header
            var str = bindingContext.ActionContext.HttpContext.Request.Headers["countryCode"].FirstOrDefault() ??
                      bindingContext.ActionContext.HttpContext.Request.Query["countryCode"].FirstOrDefault();

            bindingContext.Result = ModelBindingResult.Success(str == null ? null : new CountryCode(str));
            return Task.CompletedTask;
        }
    }

    public class CountryCode
    {
        public string Value { get; }

        public CountryCode(
            string value)
        {
            Value = value;
        }
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

public class ResponseObject
{
    public ComplexObject ComplexObjectFromQuery { get; set; }
    public List<string> ListOfSimpleTypesFromQuery { get; set; }
    public List<ComplexObject> ComplexObjectsFromBody { get; set; }
    public int FromRoute { get; set; }
}
#nullable enable
