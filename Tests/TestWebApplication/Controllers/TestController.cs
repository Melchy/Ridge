using Castle.Core.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Ridge.Results;
using System.ComponentModel;

namespace TestWebApplication.Controllers
{
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }
        [HttpGet("GetWithBody")]
        public virtual async Task<ControllerResult<int>> HttpGetWithBody([FromBody] int foo)
        {
            await Task.CompletedTask;
            return Ok(foo);
        }
        
        [HttpPost("PostWithoutBody")]
        public virtual async Task<ControllerResult> HttpPostWithoutBody()
        {
            return await Task.FromResult(Ok());
        }

        [HttpGet]
        public virtual async Task<ControllerResult> BadRequestAsync()
        {
            return await Task.FromResult(BadRequest());
        }
        
        [HttpPost]
        public virtual async Task<ControllerResult> BadRequestAction()
        {
            return BadRequest();
        }
        
        [HttpPut]
        public virtual async Task<ControllerResult<int>> Return10()
        {
            return 10;
        }
        
        [HttpDelete]
        public virtual async Task<ControllerResult<int>> ReturnAsync()
        {
            return await Task.FromResult(10);
        }
        
        [HttpPatch]
        public virtual async Task<ControllerResult> IActionResultAsync()
        {
            return await Task.FromResult(Ok());
        }
        
        [HttpOptions]
        public virtual async Task<ControllerResult> IActionResult()
        {
            return Ok();
        }
        
        [HttpPost("{fromRoute}/{fromRoute2}")]
        public virtual async Task<ControllerResult<SimpleArgumentResult>> SimpleArguments(
            [FromRoute] int fromRoute,
            [FromBody] DateTime body,
            [FromQuery] TestEnum fromQuery,
            [FromRoute] long fromRoute2,
            [FromQuery] DateTime fromQuery2)
        {
            return Ok(new SimpleArgumentResult(fromRoute, body, fromQuery, fromRoute2, fromQuery2));
        }

        public class SimpleArgumentResult
        {
            public int FromRoute { get; }
            public DateTime Body { get; }
            public TestEnum FromQuery { get; }
            public long FromRoute2 { get; }
            public DateTime FromQuery2 { get; }

            public SimpleArgumentResult(int fromRoute, DateTime body, TestEnum fromQuery, long fromRoute2, DateTime fromQuery2)
            {
                FromRoute = fromRoute;
                Body = body;
                FromQuery = fromQuery;
                FromRoute2 = fromRoute2;
                FromQuery2 = fromQuery2;
            }
        }
        
        [HttpPost("complexBody")]
        public virtual async Task<ControllerResult<ComplexArgument>> ComplexBody(
            [FromBody] ComplexArgument body)
        {
            return body;
        }

        [HttpHead("complexFromQuery")]
        public virtual async Task<ControllerResult<ComplexArgument>> ComplexFromQuery(
            [FromQuery(Name = "foo")] ComplexArgument fromQuery)
        {
            return fromQuery;
        }
        
        [HttpPost("ComplexFromForm")]
        public virtual async Task<ControllerResult<ComplexArgument>> FromForm(
            [FromForm] ComplexArgument fromRoute)
        {
            return fromRoute;
        }

        [HttpGet("FromHeader")]
        public virtual async Task<ControllerResult<ComplexArgument>> FromHeader(
            [FromHeader] ComplexArgument fromHeader)
        {
            return new ControllerResult<ComplexArgument>(fromHeader);
        }

        [HttpPost("FromHeaderSimple")]
        public virtual async Task<ControllerResult<int>> FromHeaderSimple(
            [FromHeader] int fromHeader)
        {
            return new ControllerResult<int>(fromHeader);
        }


        [HttpGet("Foo/{asd}")]
        public virtual async Task<ControllerResult<int>> Foo(
            [FromQuery(Name = "asd")] int foo2, [FromRoute(Name = "asd")] int foo)
        {
            var result = RedirectToAction("Foo", new Dictionary<string, string>()
            {
                ["asd"] = "1",
            });
            return result;
        }

        public class FooTest
        {
            public FooTest()
            {
                Test2 = null!;
            }

            public string Test2 { get; set; }
        }


        [HttpGet("FromQueryWithNameComplexArgument")]
        public virtual async Task<ControllerResult<Test>> FromQueryWithNameComplexArgument(
            [FromQuery(Name = "Something")] Test fromQuery)
        {
            return new Test()
            {
                Foo = fromQuery.Foo
            };
        }

        public class Test
        {
            public int Foo { get; set; }
        }

        [HttpGet("FromQueryWithNameSimpleArgument")]
        public virtual async Task<ControllerResult<int>> FromQueryWithNameSimpleArgument(
            [FromQuery(Name = "Something")] int fromQuery)
        {
            return fromQuery;
        }


        [HttpGet("NullsTest/{route}")]
        public virtual async Task<ControllerResult<(int?, ComplexArgument?, DateTime?, string?)>> NullsTest(
            int? query, [FromBody] ComplexArgument? body, [FromHeader] DateTime? header, [FromRoute] string? route)
        {
            return (query, body, header, route);
        }


        [HttpGet("FromRouteFromQuerySame/{route}")]
        public virtual async Task<ControllerResult> FromRouteFromQuerySameName(
            [FromQuery(Name = "foo")] string foo, [FromRoute(Name = "foo")] string foo2)
        {
            return Ok();
        }

        [HttpGet("FromRouteWithNameSimpleArgument")]
        public virtual async Task<ControllerResult<int>> FromRouteWithNameSimpleArgument(
            [FromQuery(Name = "Something")] int fromRoute)
        {
            return fromRoute;
        }

        [HttpPost("FromServices")]
        public virtual async Task<ControllerResult<bool>> FromServices(
            [FromServices] ControllerInArea? fromServices)
        {
            return fromServices != null;
        }
        
        [HttpPost("ArrayInBody")]
        public virtual async Task<ControllerResult<IEnumerable<ComplexArgument>>> ArrayInBody(
            [FromBody] IEnumerable<ComplexArgument> complexArguments)
        {
            await Task.CompletedTask;
            return complexArguments.ToList();
        }

        [HttpPost("ArrayInFromQuery")]
        public virtual async Task<ControllerResult<IEnumerable<int>>> ArrayInFromQuery(
            [FromQuery(Name = "foo")] IEnumerable<int> array)
        {
            await Task.CompletedTask;
            return array.ToList();
        }

        [HttpPost("MethodReturningHeaders")]
        public virtual async Task<ControllerResult<Dictionary<string,List<string>>>> MethodReturningHeaders()
        {
            return HttpContext.Request.Headers.ToDictionary(x=>x.Key, 
                x=>x.Value.ToList());
        }

        [HttpGet("MethodThrowingInvalidOperationException")]
        public virtual Task<ControllerResult<Dictionary<string,string>>> MethodThrowingInvalidOperationException()
        {
            throw new InvalidOperationException("Correct");
        }

        [HttpGet("MethodReturning500")]
        public virtual async Task<ControllerResult> MethodReturning500()
        {
            return StatusCode(500);
        }

        [HttpGet("ArrayOfComplexArgumentsInFromQuery")]
        public virtual async Task<ControllerResult<IEnumerable<ComplexArgument>>> ArrayOfComplexArgumentsInFromQuery([FromQuery] IEnumerable<ComplexArgument> complexArguments)
        {
            return complexArguments.ToList();
        }

        [HttpGet("DefaultPropertiesInCtorTest")]
        public virtual async Task<ControllerResult<ObjectWithDefaultProperties>> DefaultPropertiesInCtorTest([FromBody] ObjectWithDefaultProperties objectWithDefaultProperties)
        {
            return objectWithDefaultProperties;
        }

        [HttpGet("CustomBinder")]
        public virtual async Task<ControllerResult<List<int>>> CustomBinder([ModelBinder(BinderType = typeof(CommaDelimitedArrayParameterBinder))]List<int> properties)
        {
            return properties;
        }

        [HttpGet("CustomBinderFullObject/{countryCode}")]
        public virtual async Task<ControllerResult<string>> CustomBinderFullObject([ModelBinder(BinderType = typeof(CountryCodeBinder))]CountryCodeBinded countryCodeBinded)
        {
            await Task.CompletedTask;
            return countryCodeBinded.CountryCode;
        }

        public class CountryCodeBinded
        {
            public string CountryCode { get; set; } = null!;
        }


        public class ComplexArgument
        {
            public ComplexArgument(
                string str,
                int integer,
                DateTime dateTime,
                InnerObject? innerObject = null)
            {
                Str = str;
                Integer = integer;
                DateTime = dateTime;
                InnerObject = innerObject;
            }

#pragma warning disable CS8618
            public ComplexArgument()
            {

            }
#pragma warning restore
            public string Str { get; set; }
            public int Integer { get; set; }
            public DateTime DateTime { get; set; }
            public InnerObject? InnerObject { get; set; }

        }

        public class InnerObject
        {
            public InnerObject(
                string str)
            {
                Str = str;
                List = new List<string>();
            }

#pragma warning disable CS8618
            public InnerObject()
            {

            }
#pragma warning restore

            public string Str { get; set; }

            public IEnumerable<string> List { get; set; }
        }
        
        public enum TestEnum
        {
            Zero = 0,
        }

        [HttpGet("MethodReturningVoid")]
        public virtual async Task MethodReturningVoid()
        {
            return;
        }

        [HttpGet("AsyncMethodReturningTask")]
        public virtual async Task MethodReturningTask()
        {
            await Task.Yield();
        }

        [HttpGet("MethodReturningTaskNotAsync")]
        public virtual Task MethodReturningTaskNotAsync()
        {
            return Task.CompletedTask;
        }

        [HttpGet("MethodReturningIncorrectTypeInTask")]
        public virtual async Task<ActionResult> MethodReturningIncorrectTypeInTask()
        {
            await Task.CompletedTask;
            return Ok();
        }

        [HttpGet("OverloadedAction")]
        public virtual async Task<ControllerResult> OverloadedAction()
        {
            return Ok();
        }
        
        [HttpGet("OverloadedAction2")]
        public virtual async Task<ControllerResult> OverloadedAction(int a)
        {
            return Ok();
        }

        [HttpGet("MethodReturningBadRequestWithTypedResult")]
        public virtual async Task<ControllerResult<int>> MethodReturningBadRequestWithTypedResult()
        {
            return BadRequest("Error");
        }

        [HttpGet("MehodReturningControllerCallWithoutType")]
        public virtual async Task<ControllerResult<string>> MehodReturningControllerCallWithoutType()
        {
            return NoContent();
        }

        [HttpGet("MethodNotReturningControllerResult")]
        public virtual async Task<ActionResult> MethodNotReturningControllerResult()
        {
            return NoContent();
        }
    }

    public class ObjectWithDefaultProperties
    {
        public ObjectWithDefaultProperties(
            string str = "test")
        {
            Str = str;
        }

        public string Str { get; }
    }

    public class CommaDelimitedArrayParameterBinder : IModelBinder
    {

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {

            var value = bindingContext.ActionContext.HttpContext.Request.Query[bindingContext.FieldName].ToString();

            // Check if the argument value is null or empty
            if (string.IsNullOrEmpty(value))
            {
                return Task.CompletedTask;
            }

            var ints = value.Split(',').Select(int.Parse).ToArray();

            bindingContext.Result = ModelBindingResult.Success(ints);

            if(bindingContext.ModelType == typeof(List<int>))
            {
                bindingContext.Result = ModelBindingResult.Success(ints.ToList());
            }

            return Task.CompletedTask;
        }
    }


    public class CountryCodeBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var countryCode = bindingContext.ActionContext.HttpContext.Request.RouteValues["countryCode"]!.ToString();
            bindingContext.Result = ModelBindingResult.Success(new TestController.CountryCodeBinded(){CountryCode = countryCode!});
            return Task.CompletedTask;
        }
    }
}
