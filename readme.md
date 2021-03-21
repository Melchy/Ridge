# Ridge

Testing tool which allows strongly typed http requests and 
[subcutaneous](https://martinfowler.com/bliki/SubcutaneousTest.html) razor pages testing.


## Installing Ridge

Install Ridge using [nuget](https://www.nuget.org/packages/RidgeDotNet/):
```
Install-Package RidgeDotNet
```
Install using .Net Core command line:
```
dotnet add package RidgeDotNet
```
## Goal

Currently there is no simple way to test ASP.Net core controllers. One of the viable ways is to use
[WebApplicationFactory](https://docs.microsoft.com/cs-cz/aspnet/core/test/integration-tests?view=aspnetcore-5.0).
But this package has few downsides:

* Controllers must be called using url. Because of this we lose strong typing and all of it's benefits. 
* Exceptions are returned as http response with code 500.
* Response must be deserialized in test. Before result is deserialized it is necessary to check if 
  application returned http response with code 2xx.
  
Ridge solves all these problems and allows `WebApplicationFactory` to be used in unit tests.

## Simple example

```csharp
// Example controller
// Notice the ControllerResult instead of standard ActionResult
[HttpGet("ReturnGivenNumber")]
public virtual ControllerResult<int> ReturnGivenNumber(
    [FromQuery] int input)
{
    return input;
}

//...

[Test]
public void ExampleTest()
{
    // Create WebAppFactory
    // https://docs.microsoft.com/cs-cz/aspnet/core/test/integration-tests?view=aspnetcore-5.0
    using var webAppFactory = new WebApplicationFactory<Startup>();
    var client = webAppFactory.CreateClient();
    // Create controller factory
    var controllerFactory = new ControllerFactory(client, webAppFactory.Services);
    // Create instance of controller using controllerFactory
    var testController = controllerFactory.CreateController<TestController>();
    // Make standard method call which will be transformed into Http call.
    var response = testController.ReturnGivenNumber(10);
    Assert.AreEqual(10, response.Result);
    // Equivalent call would look like this:
    // var result = await client.GetFromJsonAsync<int>("/Test/ReturnGivenNumber?input=10");
}
```

## SetUp

* Change all usages of `ActionResult`, `IActionResult` and `ActionResult<T>` to `ControllerResult` or `ControllerResult<T>`
* Mark all public methods in controller as virtual.
* Mark all controller arguments which should be bound from request with attributes `[FromBody]`,`[FromRoute]`,`[FromQuery]` or`[FromHeaders]`.
* Add `app.UseRidgeImprovedExceptions();` to your `Configure` method in `Startup`
* Add `services.AddRidge();` to your `ConfigureServices` method in `Startup`

### Startup example

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services) {
        services.AddRidge(); // register ridge
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        app.UseRidgeImprovedExceptions(); // use exception middleware

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
```

## Complex example

```csharp
// notice that you do not have to use endpoint routing
// route for this action is defined in startup in following way
// endpoints.MapControllerRoute(name: "complexExample", "{controller}/{action}/{fromRoute}/{boundFromCustomModelBinder}");
public virtual async Task<ControllerResult<ResponseObject>> ComplexExample(
    [FromQuery] ComplexObject complexObjectFromQuery,
    [FromQuery] List<string> listOfSimpleTypesFromQuery,
    [FromBody] List<ComplexObject> complexObjectsFromBody,
    [FromRoute] int fromRoute,
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

////////////////////////////////// Test file //////////////////////////////
[Test]
public async Task ComplexTest()
{
    var webAppFactory = new WebApplicationFactory<Startup>();
    var client = webAppFactory.CreateClient();
    var controllerFactory = new ControllerFactory(client,
        webAppFactory.Services,
        new NunitLogWriter()); // add writer which writes generated requests to test output.
    // Register transformer which allows us to work with custom model binder
    controllerFactory.AddActionInfoTransformer(new CustomModelBinderTransformer());
    // add httpRequestTransformation which allows us to transform final http request
    controllerFactory.AddHttpRequestPipelinePart(new HttpRequestTransformationPipelinePart());
    var testController = controllerFactory.CreateController<ExamplesController>();
    var response = await testController.ComplexExample(
        complexObjectFromQuery: new ComplexObject()
        {
            Str = "str",
            NestedComplexObject = new NestedComplexObject()
            {
                Integer = 1,
                Str = "string"
            },
        },
        listOfSimpleTypesFromQuery:new List<string>()
        {
            "foo", "bar"
        },
        complexObjectsFromBody:new List<ComplexObject>()
        {
            new ComplexObject()
            {
                Str = "str",
                NestedComplexObject = new NestedComplexObject()
                {
                    Integer = 5,
                    Str = "bar"
                }
            }
        },
        fromRoute: 1,
        // this value is used only because we added CustomModelBinderTransformer
        customModelBinder: "customModelBinder",
        examplesController:null); // this value wont be used

    Assert.AreEqual("str", response.Result.ComplexObjectFromQuery.Str);
    Assert.AreEqual("string", response.Result.ComplexObjectFromQuery.NestedComplexObject.Str);
    Assert.AreEqual("foo", response.Result.ListOfSimpleTypesFromQuery.First());
    Assert.AreEqual(5, response.Result.ComplexObjectsFromBody.First().NestedComplexObject.Integer);
    Assert.AreEqual(1, response.Result.FromRoute);
    Assert.AreEqual("customModelBinder", response.Result.CustomModelBinder);
}


public class CustomModelBinderTransformer : IActionInfoTransformer
{
    public Task TransformAsync(
        IActionInfo actionInfo, //action info contains information about request
        InvocationInfo invocationInfo)
    {
        // using ElementAt is not recommended.
        // Better way would be to wrap customModelBinder argument in custom class and search for that class
        actionInfo.RouteParams.Add("boundFromCustomModelBinder", invocationInfo.Arguments.ElementAt(4));
        return Task.CompletedTask;
    }
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
        httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
        var response = await next();
        // we could even transform response
        //response.Content = new StringContent("foo");
        return response;
    }
}
```

## Features which are not supported
