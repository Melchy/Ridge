using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ridge.AspNetCore.GeneratorAttributes;

namespace AlterClientGeneration.Controllers;

[ApiController]
[Route("[controller]")]
[GenerateClient]
// Note that we have to change GeneratedParameterName because this name will be used as header key
[TransformActionParameter(typeof(CountryCode), typeof(string), ParameterMapping.None, GeneratedParameterName = "countryCode")]
public class TransformParameterWithManualMappingController : ControllerBase
{
    [HttpGet]
    public IEnumerable<WeatherForecast> Get(
        [ModelBinder(typeof(BindCountryCodeFromQueryOrHeader))] CountryCode country)
    {
        return new List<WeatherForecast>()
        {
            new(
                Date: DateOnly.FromDateTime(DateTime.Now),
                Summary: "Cool",
                TemperatureC: 1,
                Country: country.Value
            ),
        };
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
