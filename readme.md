# Ridge

Testing tool which allows strongly typed http requests.


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

```csharp
// Example controller
// Method must be virtual
[HttpGet("ReturnGivenNumber")]
public virtual ActionResult<int> ReturnGivenNumber([FromQuery] int input)
{
    return input;
}

//...
[Test]
public async Task ExampleTest()
{
    // Create webApplicationFactory
    // https://docs.microsoft.com/cs-cz/aspnet/core/test/integration-tests?view=aspnetcore-5.0
    var webApplicationFactory = new WebApplicationFactory<Startup>();
    var client = webApplicationFactory.CreateClient();
    // Create controller factory using ridge package
    var controllerFactory = new ControllerFactory(client, webApplicationFactory.Services);


    // Create instance of controller using controllerFactory.
    // This is where the magic happens. Ridge replaces controller implementation
    // with custom code which transforms method calls to http calls.
    var testController = controllerFactory.CreateController<ExamplesController>();
    // Make standard method call which will be transformed into Http call.
    var response = testController.ReturnGivenNumber(10);
    // Equivalent call using WebAppFactory would look like this:
    // var result = await client.GetFromJsonAsync<int>("/Test/ReturnGivenNumber?input=10");


    //Assert httpResponseMessage
    var httpResponseMessage = response.HttpResponseMessage();
    var content = await httpResponseMessage.Content.ReadAsStringAsync();
    Assert.AreEqual(10, int.Parse(content));
    Assert.True(httpResponseMessage.IsSuccessStatusCode);

    //You can use our extension methods to simplify assertion.


    //Instead of Assert.True(response.HttpResponseMessage.IsSuccessStatusCode) Use:
    Assert.True(response.IsSuccessStatusCode());
    // Instead of
    // var content = await httpResponseMessage.Content.ReadAsStringAsync();
    // Assert.AreEqual(10, int.Parse(content));
    // Use:
    Assert.AreEqual(10, response.GetResult());
}
```


## Setup

* Mark methods in controller as virtual
* Add `app.UseRidgeImprovedExceptions();` to your `Configure` method in `Startup`. This middleware is used only 
if application is called from test using ridge.

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



## Motivation

In our application we often used thin controllers containing methods like this:

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] NewPostCommand command)
{
    var result = await Mediator.Send(command);
    return Ok(result);
}
```

Initially we taught it is not necessary to test those thin controllers but after a while we realized that even those 
thin controllers can contain many bugs. Examples of these bugs are:

* Using [FromRoute] with [HttpPost("someUrl")] instead of [HttpPost("someUrl/{variable}")].
* Forgetting to add `[FromRoute]`, `[FromQuery]`, `[FromHeader]` and other similar attribute
* Incorrect return types. Sometimes application returned 200 instead of 400.
* Bugs in middleware and filters.
* Bugs in validation. For example missing [Required] attribute on property.
* And many others

Furthermore we couldn't test many of the controller responsibilities like obtaining values from httpContext, parsing headers
and so on.

### Microsoft for the win

After some time of struggling with these problems we found [WebApplicationFactory](https://docs.microsoft.com/cs-cz/aspnet/core/test/integration-tests?view=aspnetcore-5.0).
This factory allows you to make fake http request to your application. For more information about 
WebApplicationFactory see [documentation](https://docs.microsoft.com/cs-cz/aspnet/core/test/integration-tests?view=aspnetcore-5.0).

### Struggle continues

For a while we used combination of `WebApplicationFactory` and unit tests but after 
some time we found this approach to be cumbersome. This was mainly because of the following problems with `WebApplicationFactory`:

* `WebApplicationFactory` makes calls using Urls which are not strongly typed (obviously) because of this it was sometimes hard to 
find which action is called from the test.
* Exceptions are not propagated out of your application. They are transformed into http result with error code 500. This is useful in production
but not so much in tests.
  
* Building the request manually was time consuming. Programmers spent lot of time figuring out how to build body, query parameters
and route parameters.
  
* Response validation was not trivial. It was necessary to check returned status code and if it 
  was successful deserialize result. If status code indicated failure it was necessary to propagate this error to test result
  or validate correct error message.

All these problems lead us to create Ridge - the strongly typed controller caller. We used this tool since and didn't look back.

## Exceptions

Exceptions are rethrown with correct call stack.

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

If your application returns 500 instead of throwing exception it is possible that you forgot to register
ridge middleware (see setup).

## Complex example

Ridge can handle nearly all of the use cases you can imagine. Following example show call of complex action:

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

Custom model binders are supported. Example:

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
[linkGenerator.GetPathByRouteValues](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.routing.linkgeneratorroutevaluesaddressextensions.getpathbyroutevalues?view=aspnetcore-5.0). 
This allows you to transform or add all the necessary things to generate request. Example usage of `IActionTransformer`
can be seen in custom model binders section.

If `IActionTransformer` is not enough for your use case you can use `IHttpRequestPipelinePart` which allows you to
transform the `HttpRequest` right before it is sent. Example usage:

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

Sometimes it is nice to see what request Ridge generated. 
To log request add `XunitLogWriter` or `NunitLogWriter` or implement custom 
log writer and pass it to `ControllerFactory`. Example:

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

Example log:
```
Ridge generated request:
Method: POST, RequestUri: '/Test/complexBody', Version: 1.1, Content: System.Net.Http.StringContent, Headers:
{
ridgeCallId: c61d152a-f6eb-4c03-8023-ce34fdbdc000
Content-Type: application/json; charset=utf-8
Content-Length: 109
}
Body:
{"Str":"test","Integer":10,"DateTime":"2021-05-15T00:00:00+02:00","InnerObject":{"Str":"InnerStr","List":[]}}
```

Note that ridgeCallId header is ridge specific header necessary for internal request processing.

## Features which are not supported

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

### Features which can not be implemented

* Only methods returning ActionResult, ActionResult<T>, IActionResult or class implementing IActionResult are supported.
* Methods called with Ridge must be virtual.

