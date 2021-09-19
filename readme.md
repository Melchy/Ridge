# Ridge

Testing tool which allows strongly typed http requests using
[WebApplicationFactory](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-5.0#basic-tests-with-the-default-webapplicationfactory)
.

## Installing Ridge

Install Ridge using [nuget](https://www.nuget.org/packages/RidgeDotNet/):

```
Install-Package RidgeDotNet
```

Install using .Net Core command line:

```
dotnet add package RidgeDotNet
```

## Simple example

In ASP.NET Core 2.1 microsoft
added [WebApplicationFactory](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-5.0#basic-tests-with-the-default-webapplicationfactory)
which can create mock web server. This web server allows you to make http calls in-process without network overhead.

```csharp
// Example controller
[HttpGet("ReturnGivenNumber")]
public ActionResult<int> ReturnGivenNumber([FromQuery] int input)
{
    return input;
}

//...
[Test]
public async Task ExampleTest()
{
    var webApplicationFactory = new WebApplicationFactory<Startup>();
    var client = webApplicationFactory.CreateClient();

    var result = await client.GetFromJsonAsync<int>("/Test/ReturnGivenNumber?input=10");

    Assert.AreEqual(10, result);
}
```

With Ridge you can change the http call to strongly typed method call:

```csharp
[Test]
public void TestUsingRidge()
{
    var webApplicationFactory = new WebApplicationFactory<Startup>();
    var client = webApplicationFactory.CreateClient();
    var controllerFactory = new ControllerFactory(
        client,
        webApplicationFactory.Services,
        new NunitLogWriter());

    var testController = controllerFactory.CreateController<ExamplesController>();
    // Ridge transforms method call to httpRequest
    var response = testController.ReturnGivenNumber(10);

    Assert.True(response.IsSuccessStatusCode());
    Assert.AreEqual(10, response.GetResult());
}
```

## Setup

* Mark methods in controller as virtual.
* Add `app.UseRidgeImprovedExceptions();` to your `Configure` method in `Startup`. (This middleware is used only if
  application is called from test using Ridge.)

### Startup example

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services) {
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        
        // use exception middleware
        app.UseRidgeImprovedExceptions(); 

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
```

## Assertions

Ridge offers multiple extension methods which you can use to assert result:

```csharp
// Exctension methods on IActionResult and ActionResult
actionResult.HttpResponseMessage()
actionResult.ResultAsString()
actionResult.StatusCode()
actionResult.IsSuccessStatusCode() // status code >=200 and <300
actionResult.IsRedirectStatusCode() // status code >=300 and <400
actionResult.IsClientErrorStatusCode() // status code >=400 and <500
actionResult.IsServerErrorStatusCode() // status code >=500 and <600
actionResult.Unwrap() // get ControllerResult which contains all of the above information

// Exctension methods on ActionResult<T>
actionResult.GetResult<T>() // tries to desirialize response to T
actionResult.HttpResponseMessage()
actionResult.ResultAsString()
actionResult.StatusCode()
actionResult.IsSuccessStatusCode() // status code >=200 and <300
actionResult.IsRedirectStatusCode() // status code >=300 and <400
actionResult.IsClientErrorStatusCode() // status code >=400 and <500
actionResult.IsServerErrorStatusCode() // status code >=500 and <600
actionResult.Unwrap() // get ControllerResult<T> which contains all of the above information
```

## Exceptions

When using WebApplicationFactory all exceptions are transformed to 500 status code. Ridge behaves differently and
rethrows the exceptions.

```csharp
[HttpGet("ThrowException")]
public virtual ActionResult ThrowException()
{
    throw new InvalidOperationException("Exception throw");
}

//...
[Test]
public async Task ThrowExceptionTest()
{
    var webApplicationFactory = new WebApplicationFactory<Startup>();
    var client = webApplicationFactory.CreateClient();
    var controllerFactory = new ControllerFactory(client, webApplicationFactory.Services);

    var testController = controllerFactory.CreateController<ExamplesController>();
    try
    {
        _ = testController.ThrowException();
    }
    catch (InvalidOperationException e)
    {
        Assert.AreEqual("Exception throw", e.Message);
    }
}

```

Note that if your application returns 500 instead of throwing exception it is possible that you forgot to register Ridge
middleware (see setup).

## Complex example

```csharp
// notice that you don't have to use endpoint routing
// route for this action is defined in startup in following way
// endpoints.MapControllerRoute(name: "complexExample", "{controller}/{action}/{fromRoute}/{boundFromCustomModelBinder}");
public virtual async Task<ActionResult<ResponseObject>> ComplexExample(
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
        ListOfSimpleTypesFromQuery = listOfSimpleTypesFromQuery
    };
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

//test code 

public void Test()
{

//Arange
//omitted for brevity

//Act
var testController = controllerFactory.CreateController<ExamplesController>();
var response = testController.ComplexExample(
    complexObjectFromQuery: new ComplexObject()
    {
        Str = "str",
        NestedComplexObject = new NestedComplexObject()
        {
            Integer = 1,
            Str = "string",
        },
    },
    listOfSimpleTypesFromQuery: new List<string>()
    {
        "foo", "bar",
    },
    complexObjectsFromBody: new List<ComplexObject>()
    {
        new ComplexObject()
        {
            Str = "str",
            NestedComplexObject = new NestedComplexObject()
            {
                Integer = 5,
                Str = "bar",
            },
        },
    },
    fromRoute: 1,
    examplesController: null); // this value won`t be used
    
//Assert
//omitted for brevity
```

## Custom model binders

```csharp
[HttpGet("customModelBinder/{thisIsBoundedUsingCustomBinder}")]
public virtual ActionResult<string> CustomModelBinderExample(
    [ModelBinder(typeof(CountryCodeBinder))] string boundFromCustomModelBinder)
{
    return boundFromCustomModelBinder;
}

// simple model binder which does the same as FromRoute attribute
public class CountryCodeBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var str = bindingContext.ActionContext.HttpContext
            .Request.RouteValues["thisIsBoundedUsingCustomBinder"]!.ToString();
        bindingContext.Result = ModelBindingResult.Success(str);
        return Task.CompletedTask;
    }
}

// Test 

[Test]
public void CustomModelBinderTest()
{
    var webAppFactory = new WebApplicationFactory<Startup>();
    var client = webAppFactory.CreateClient();
    var controllerFactory = new ControllerFactory(client, webAppFactory.Services);
    
    // Register action transformer which allows us to work with custom model binder
    controllerFactory.AddActionInfoTransformer(new CustomModelBinderTransformer());
    
    var testController = controllerFactory.CreateController<ExamplesController>();
    var response = testController.CustomModelBinderExample("exampleValue");

    Assert.AreEqual("exampleValue", response.GetResult());
}

public class CustomModelBinderTransformer : IActionInfoTransformer
{
    public Task TransformAsync(
        IActionInfo actionInfo, // IActionInfo contains information about request
        InvocationInfo invocationInfo) // invocation info contains information about method that was called
    {
        // set route parameter "thisIsBoundedUsingCustomBinder" to value of first argument passed to method
        actionInfo.RouteParams.Add("thisIsBoundedUsingCustomBinder", invocationInfo.Arguments.First());
        return Task.CompletedTask;
    }
}
```

## Ridge extendability

Ridge contains two extendability points - `IActionInfoTransformer` and `IHttpRequestPipelinePart`.
`IActionTransformer` can manipulate data right before the url is created using
[linkGenerator.GetPathByRouteValues](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.routing.linkgeneratorroutevaluesaddressextensions.getpathbyroutevalues?view=aspnetcore-5.0)
. This allows you to transform or add all the necessary things to generate request. Example usage
of `IActionTransformer`
can be seen in custom model binders section.

If `IActionTransformer` is not enough for you then you can use `IHttpRequestPipelinePart` which allows transformation of
the `HttpRequest` right before it is sent.

```csharp

[Test]
public void HttpRequestPipelineTest()
{
    var webAppFactory = new WebApplicationFactory<Startup>();
    var client = webAppFactory.CreateClient();
    var controllerFactory = new ControllerFactory(client, webAppFactory.Services);

    controllerFactory.AddHttpRequestPipelinePart(new HttpRequestTransformationPipelinePart());

    var testController = controllerFactory.CreateController<ExamplesController>();
    var response = testController.CallThatNeedsHeaders();

    Assert.True(response.IsSuccessStatusCode());
}

public class HttpRequestTransformationPipelinePart : IHttpRequestPipelinePart
{
    public async Task<HttpResponseMessage> InvokeAsync(
        Func<Task<HttpResponseMessage>> next,
        HttpRequestMessage httpRequestMessage,
        IReadOnlyActionInfo actionInfo,
        InvocationInfo invocationInfo)
    {
        // transform http request
        httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var response = await next();
        // we could even transform response
        return response;
    }
}
```

## Logging

To log request and responses add `XunitLogWriter` or `NunitLogWriter` or implement custom log writer and pass it
to `ControllerFactory`. Example:

```csharp

// xunit 
public class XunitLoggerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public XunitLoggerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    
    public void Test(){
        //...
        var controllerFactory = new ControllerFactory(client,
                    webAppFactory.Services,
                    new XunitLogWriter(_testOutputHelper));
        //...
    }


// Nunit
public void Test(){
//...
var controllerFactory = new ControllerFactory(client,
                webAppFactory.Services,
                new NunitLogWriter());
//...
}
         
                
// Custom implementation
public class CustomLogWriter : ILogWriter
{
    public void WriteLine(string text)
    {
        // write line somewhere
    }
}

public void Test(){
//...
var controllerFactory = new ControllerFactory(client,
                webAppFactory.Services,
                new CustomLogWriter());
//...
}
```

Example log for `simpleExample` mentioned in this readme:

```
Request:
Method: GET, RequestUri: '/ReturnGivenNumber?input=10', 
Version: 1.1, Content: System.Net.Http.StringContent, Headers:
{
  ridgeCallId: 565458ac-2217-4304-adf1-a2baa86bc33b
  Content-Type: application/json; charset=utf-8
  Content-Length: 2
}
Body:
{}

Response:
StatusCode: 200, ReasonPhrase: 'OK', Version: 1.1, Content: System.Net.Http.StreamContent, Headers:
{
  Content-Type: application/json; charset=utf-8
  Content-Length: 2
}
Body:
10
```

Note that `ridgeCallId` header is ridge specific header necessary for internal request processing.

## Serialization

Serialization library is automatically determined based on asp.net core settings. For custom serialization
implement `IRidgeSerializer` and pass it to `ControllerFactory`.

## Best practices

* Use strongly typed `ActionsResult<T>` when possible.
* Use [FromRoute], [FromQuery], [FromBody] and similar attributes.

## Not supported features

* Methods not returning ActionResult, ActionResult<T>, IActionResult.
* Projects running on .net core 2.1 and lower.

### Features which may be implemented in future

* `[FromQuery]` with array of complex arguments is not supported:

```csharp
public virtual ActionResult NotSupported([FromQuery] IEnumerable<ComplexArgument> complexArguments){
   //..
}
```

* Complex types with `[FromXXX]` attributes on properties are not supported:

```csharp
public virtual ActionResult NotSupported(Mixed mixed){
   //..
}


public class Mixed
{
    [FromBody]
    public string BodyName { get; set; }
    [FromHeader]
    public string HeaderName { get; set; }
}
```
