using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ridge.AspNetCore.GeneratorAttributes;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApplication.Controllers;

[GenerateClient]
[TransformActionParameter(typeof(string), typeof(string), ParameterMapping.MapToQueryOrRouteParameter)]
[TransformActionParameter(typeof(TestEnum), typeof(TestEnum), ParameterMapping.MapToQueryOrRouteParameter)]
[TransformActionParameter(typeof(int), typeof(int), ParameterMapping.MapToHeader)]
[TransformActionParameter(typeof(CountryCode), typeof(string), ParameterMapping.MapToHeader, GeneratedParameterName = "countryCode")]
[TransformActionParameter(typeof(double), typeof(double), ParameterMapping.MapToBody)]
public class TransformedParametersWithDefaultMappingController : ControllerBase
{
    [HttpGet("transformation/{route}")]
    public Task<ActionResult<(string fromRouteParameter, TestEnum fromQueryParameter, double body, int fromHeaderParameter, string parameterWithNameFromTransformer)>> DefaultAction(
        [FromQuery] TestEnum query,
        [FromRoute] string route,
        [FromHeader] int header,
        [FromBody] double body, 
        [ModelBinder(typeof(BindCountryCodeFromQueryOrHeader))] CountryCode country)
    {
        return Task.FromResult<ActionResult<(string fromRouteParameter, TestEnum fromQueryParameter, double body, int fromHeaderParameter, string parameterWithNameFromTransformer)>>((route, query, body, header, country.Value));
    }

    public enum TestEnum
    {
        None = 0,
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
