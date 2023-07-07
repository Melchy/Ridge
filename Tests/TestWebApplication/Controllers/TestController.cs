using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Ridge.AspNetCore.GeneratorAttributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TestWebApplication.Controllers;

[GenerateClient]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(
        ILogger<TestController> logger)
    {
        _logger = logger;
    }

    [HttpGet("GetWithBody")]
    public async Task<ActionResult<int>> HttpGetWithBody(
        [FromBody] int foo)
    {
        await Task.CompletedTask;
        return Ok(foo);
    }

    [HttpGet("ActionWithEventParamter")]
    public void ActionWithEventParamter(
        string @event)
    {
    }


    [HttpPost("PostWithoutBody")]
    public async Task<ActionResult> HttpPostWithoutBody()
    {
        return await Task.FromResult(Ok());
    }

    [HttpGet]
    public async Task<ActionResult> BadRequestAsync()
    {
        return await Task.FromResult(BadRequest());
    }

    [HttpPost]
    public Task<ActionResult> BadRequestAction()
    {
        return Task.FromResult<ActionResult>(BadRequest());
    }

    [HttpPut]
    public Task<ActionResult<int>> Return10()
    {
        return Task.FromResult<ActionResult<int>>(10);
    }

    [HttpDelete]
    public async Task<ActionResult<int>> ReturnAsync()
    {
        return await Task.FromResult(10);
    }

    [HttpDelete("ReturnSync")]
    public ActionResult ReturnSync()
    {
        return Ok();
    }

    [HttpPatch]
    public async Task<ActionResult> IActionResultAsync()
    {
        return await Task.FromResult(Ok());
    }

    [HttpOptions]
    public Task<ActionResult> IActionResult()
    {
        return Task.FromResult<ActionResult>(Ok());
    }

    [HttpPost("SimpleArguments/{fromRoute}/{fromRoute2}")]
    public Task<ActionResult<SimpleArgumentResult>> SimpleArguments(
        [FromRoute] int fromRoute,
        [FromBody] DateTime body,
        [FromQuery] TestEnum fromQuery,
        [FromRoute] [BindRequired] long fromRoute2,
        [FromQuery] DateTime fromQuery2)
    {
        return Task.FromResult<ActionResult<SimpleArgumentResult>>(Ok(new SimpleArgumentResult(fromRoute, body, fromQuery, fromRoute2, fromQuery2)));
    }

    public class SimpleArgumentResult
    {
        public int FromRoute { get; }
        public DateTime Body { get; }
        public TestEnum FromQuery { get; }
        public long FromRoute2 { get; }
        public DateTime FromQuery2 { get; }

        public SimpleArgumentResult(
            int fromRoute,
            DateTime body,
            TestEnum fromQuery,
            long fromRoute2,
            DateTime fromQuery2)
        {
            FromRoute = fromRoute;
            Body = body;
            FromQuery = fromQuery;
            FromRoute2 = fromRoute2;
            FromQuery2 = fromQuery2;
        }

        [HttpPatch("PatchWithBody")]
        public Task<ActionResult<ComplexObject>> PatchWithBody(
            [FromBody] ComplexObject complexObject)
        {
            return Task.FromResult<ActionResult<ComplexObject>>(complexObject);
        }
    }

    [HttpPost("complexBody")]
    public Task<ActionResult<ComplexArgument>> ComplexBody(
        [FromBody] ComplexArgument body)
    {
        return Task.FromResult<ActionResult<ComplexArgument>>(body);
    }

    [HttpHead("complexFromQuery")]
    public Task<ActionResult<ComplexArgument>> ComplexFromQuery(
        [FromQuery(Name = "foo")] ComplexArgument fromQuery)
    {
        return Task.FromResult<ActionResult<ComplexArgument>>(fromQuery);
    }

    [HttpPost("ComplexFromForm")]
    public Task<ActionResult<ComplexArgument>> FromForm(
        [FromForm] ComplexArgument fromRoute)
    {
        return Task.FromResult<ActionResult<ComplexArgument>>(fromRoute);
    }

    [HttpGet("FromHeader")]
    public Task<ActionResult<ComplexArgument>> FromHeader(
        [FromHeader] ComplexArgument fromHeader)
    {
        return Task.FromResult(new ActionResult<ComplexArgument>(fromHeader));
    }

    [HttpPatch("PatchWithBody")]
    public ActionResult<ComplexObject> PatchWithBody(
        [FromBody] ComplexObject complexObject)
    {
        return complexObject;
    }
    
    [HttpPost("FromHeaderSimple")]
    public Task<ActionResult<int>> FromHeaderSimple(
        [FromHeader] int fromHeader)
    {
        return Task.FromResult(new ActionResult<int>(fromHeader));
    }

    [HttpGet("FromHeaderWithArray")]
    public Task<ActionResult<(int header1, int header2)>> FromHeaderWithArray(
        [FromHeader(Name = "header")] int[] fromHeader)
    {
        return Task.FromResult(new ActionResult<(int header1, int header2)>((fromHeader.First(), fromHeader.Skip(1).First())));
    }

    [HttpGet("Foo/{asd}")]
    public Task<ActionResult<int>> Foo(
        [FromQuery(Name = "asd")] int foo2,
        [FromRoute(Name = "asd")] int foo)
    {
        var result = RedirectToAction("Foo",
            new Dictionary<string, string>()
            {
                ["asd"] = "1",
            });
        return Task.FromResult<ActionResult<int>>(result);
    }

    public class FooTest
    {
        public string Test2 { get; set; }

        public FooTest()
        {
            Test2 = null!;
        }
    }


    [HttpGet("FromQueryWithNameComplexArgument")]
    public Task<ActionResult<Test>> FromQueryWithNameComplexArgument(
        [FromQuery(Name = "Something")] Test fromQuery)
    {
        return Task.FromResult<ActionResult<Test>>(new Test()
        {
            Foo = fromQuery.Foo,
        });
    }

    public class Test
    {
        public int Foo { get; set; }
    }

    [HttpGet("FromQueryWithNameSimpleArgument")]
    public Task<ActionResult<int>> FromQueryWithNameSimpleArgument(
        [FromQuery(Name = "Something")] int fromQuery)
    {
        return Task.FromResult<ActionResult<int>>(fromQuery);
    }


    [HttpGet("NullsTest/{route}")]
    public Task<ActionResult<(int?, ComplexArgument?, DateTime?, string?)>> NullsTest(
        int? query,
        [FromBody] ComplexArgument? body,
        [FromHeader] DateTime? header,
        [FromRoute] string? route)
    {
        return Task.FromResult<ActionResult<(int?, ComplexArgument?, DateTime?, string?)>>((query, body, header, route));
    }


    [HttpGet("FromRouteFromQuerySame/{route}")]
    public Task<ActionResult> FromRouteFromQuerySameName(
        [FromQuery(Name = "foo")] string foo,
        [FromRoute(Name = "foo")] string foo2)
    {
        return Task.FromResult<ActionResult>(Ok());
    }

    [HttpGet("FromRouteWithNameSimpleArgument")]
    public Task<ActionResult<int>> FromRouteWithNameSimpleArgument(
        [FromQuery(Name = "Something")] int fromRoute)
    {
        return Task.FromResult<ActionResult<int>>(fromRoute);
    }

    [HttpPost("FromServices")]
    public Task<ActionResult<bool>> FromServices(
        [FromServices] ControllerInArea? fromServices)
    {
        return Task.FromResult<ActionResult<bool>>(fromServices != null);
    }

    [HttpPost("ArrayInBody")]
    public async Task<ActionResult<IEnumerable<ComplexArgument>>> ArrayInBody(
        [FromBody] IEnumerable<ComplexArgument> complexArguments)
    {
        await Task.CompletedTask;
        return complexArguments.ToList();
    }

    [HttpPost("ArrayInFromQuery")]
    public async Task<ActionResult<IEnumerable<int>>> ArrayInFromQuery(
        [FromQuery(Name = "foo")] IEnumerable<int> array)
    {
        await Task.CompletedTask;
        return array.ToList();
    }

    [HttpPost("MethodReturningHeaders")]
    public Task<ActionResult<Dictionary<string, List<string?>>>> MethodReturningHeaders()
    {
        return Task.FromResult<ActionResult<Dictionary<string, List<string?>>>>(HttpContext.Request.Headers.ToDictionary(x => x.Key,
            x => x.Value.ToList()));
    }

    [HttpPost("MethodReturningBody")]
    public async Task<string> MethodReturningBody()
    {
        StreamReader reader = new StreamReader(HttpContext.Request.Body);
        return await reader.ReadToEndAsync();
    }

    [HttpGet("RouteAndQueryParameters/{routeParameter}")]
    public (string? routeParameter, string? queryParameter) RouteAndQueryParameters()
    {
        return (HttpContext.Request.RouteValues["routeParameter"]?.ToString(), HttpContext.Request.Query["queryParameter"].FirstOrDefault());
    }

    [HttpGet("MethodThrowingInvalidOperationException")]
    public Task<ActionResult<Dictionary<string, string>>> MethodThrowingInvalidOperationException()
    {
        throw new InvalidOperationException("Correct");
    }

    [HttpGet("MethodReturning500")]
    public Task<ActionResult> MethodReturning500()
    {
        return Task.FromResult<ActionResult>(StatusCode(500));
    }

    [HttpGet("ArrayOfComplexArgumentsInFromQuery")]
    public Task<ActionResult<IEnumerable<ComplexArgument>>> ArrayOfComplexArgumentsInFromQuery(
        [FromQuery] IEnumerable<ComplexArgument> complexArguments)
    {
        return Task.FromResult<ActionResult<IEnumerable<ComplexArgument>>>(complexArguments.ToList());
    }

    [HttpGet("ArgumentsWithoutAttributes/{fromRoute}")]
    public Task<ActionResult<SpecialComplexObject>> ArgumentsWithoutAttributes(
        ComplexObject complexObjectFromQuery,
        int fromRoute,
        int fromQuery)
    {
        return Task.FromResult<ActionResult<SpecialComplexObject>>(new SpecialComplexObject()
        {
            ComplexObject = complexObjectFromQuery,
            FromQuery = fromQuery,
            FromRoute = fromRoute,
        });
    }

    [HttpGet("DefaultPropertiesInCtorTest")]
    public Task<ActionResult<ObjectWithDefaultProperties>> DefaultPropertiesInCtorTest(
        [FromBody] ObjectWithDefaultProperties objectWithDefaultProperties)
    {
        return Task.FromResult<ActionResult<ObjectWithDefaultProperties>>(objectWithDefaultProperties);
    }

    [HttpGet("CustomBinder")]
    public Task<ActionResult<List<int>>> CustomBinder(
        [ModelBinder(BinderType = typeof(CommaDelimitedArrayParameterBinder))] List<int> properties)
    {
        return Task.FromResult<ActionResult<List<int>>>(properties);
    }

    [HttpGet("CustomBinderFullObject/{countryCode}")]
    public async Task<ActionResult<string>> CustomBinderFullObject(
        [ModelBinder(BinderType = typeof(CountryCodeBinder))] CountryCodeBinded countryCodeBinded)
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
    public Task MethodReturningVoid()
    {
        return Task.CompletedTask;
    }

    [HttpGet("AsyncMethodReturningTask")]
    public async Task MethodReturningTask()
    {
        await Task.Yield();
    }

    [HttpGet("TestThisIsNowGood")]
    public async Task TestThisIsNowGood()
    {
        await Task.Yield();
    }

    [HttpGet("MethodReturningTaskNotAsync")]
    public Task MethodReturningTaskNotAsync()
    {
        return Task.CompletedTask;
    }

    [HttpGet("MethodReturningIncorrectTypeInTask")]
    public Task<int> MethodReturningIncorrectTypeInTask()
    {
        return Task.FromResult(1);
    }

    [HttpGet("OverloadedAction")]
    public Task<ActionResult> OverloadedAction()
    {
        return Task.FromResult<ActionResult>(Ok());
    }

    [HttpGet("OverloadedAction2")]
    public Task<ActionResult> OverloadedAction(
        int a)
    {
        return Task.FromResult<ActionResult>(Ok());
    }

    [HttpGet("MethodReturningBadRequestWithTypedResult")]
    public Task<ActionResult<int>> MethodReturningBadRequestWithTypedResult()
    {
        return Task.FromResult<ActionResult<int>>(BadRequest("Error"));
    }

    [HttpGet("MehodReturningControllerCallWithoutType")]
    public Task<ActionResult<string>> MehodReturningControllerCallWithoutType()
    {
        return Task.FromResult<ActionResult<string>>(NoContent());
    }

    [HttpGet("MethodNotReturningActionResult")]
    public Task<int> MethodNotReturningActionResult()
    {
        return Task.FromResult(1);
    }

    [HttpGet("SyncThrow")]
    public ActionResult SyncThrow()
    {
        throw new InvalidOperationException("Error");
    }

    [HttpGet("SyncNotReturningActionResult")]
    public int SyncNotReturningActionResult()
    {
        return 1;
    }

    [HttpGet("ReturnSyncWithResult")]
    public ActionResult<string> ReturnSyncWithResult()
    {
        return "ok";
    }

    [HttpGet("TaskCancellationTokenIsRemoved")]
    public ActionResult<string> TaskCancellationTokenIsRemoved(
        CancellationToken cancellationToken)
    {
        return "ok";
    }

    [HttpGet("ReturnsBody")]
    public ActionResult<string> ReturnsBody(
        [FromBody] string fromBody)
    {
        return fromBody;
    }
    
    [HttpGet("ExceptionToBeRethrown")]
    public ActionResult ExceptionToBeRethrown()
    {
        throw new ExceptionToBeRethrown();
    }
}

public class CountryCodeBinder : IModelBinder
{
    public Task BindModelAsync(
        ModelBindingContext bindingContext)
    {
        var countryCode = bindingContext.ActionContext.HttpContext.Request.RouteValues["countryCode"]!.ToString();
        bindingContext.Result = ModelBindingResult.Success(new TestController.CountryCodeBinded() {CountryCode = countryCode!});
        return Task.CompletedTask;
    }
}

public class SpecialComplexObject
{
    public ComplexObject ComplexObject { get; set; } = null!;
    public int FromRoute { get; set; }
    public int FromQuery { get; set; }
}

public class ObjectWithDefaultProperties
{
    public string Str { get; }

    public ObjectWithDefaultProperties(
        string str = "test")
    {
        Str = str;
    }
}

public class CommaDelimitedArrayParameterBinder : IModelBinder
{
    public Task BindModelAsync(
        ModelBindingContext bindingContext)
    {
        var value = bindingContext.ActionContext.HttpContext.Request.Query[bindingContext.FieldName].ToString();

        // Check if the argument value is null or empty
        if (string.IsNullOrEmpty(value))
        {
            return Task.CompletedTask;
        }

        var ints = value.Split(',').Select(int.Parse).ToArray();

        bindingContext.Result = ModelBindingResult.Success(ints);

        if (bindingContext.ModelType == typeof(List<int>))
        {
            bindingContext.Result = ModelBindingResult.Success(ints.ToList());
        }

        return Task.CompletedTask;
    }
}

public class ExceptionToBeRethrown : Exception
{
}
