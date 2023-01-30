# Ridge

Source generator which generates api caller for integration tests.

## Install Ridge

Install Ridge using [nuget](https://www.nuget.org/packages/RidgeDotNet/):

```
Install-Package RidgeDotNet
```

Install using .Net Core command line:

```
dotnet add package RidgeDotNet
```

## Example

```csharp
// Controller class
// Notice the attribute
[GenerateCaller]
public class ExamplesController : Controller
{
    [HttpGet("ReturnGivenNumber")]
    public ActionResult<int> ReturnGivenNumber(
        [FromQuery] int input)
    {
        return input;
    }
}

// Test file
[Test]
public async Task CallControllerUsingRidge()
{
    // Create WebApplicationFactory - https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-5.0#basic-tests-with-the-default-webapplicationfactory
    using var webApplicationFactory = new WebApplicationFactory<Program>();
    // create http client for ridge caller
    var client = webApplicationFactory.CreateRidgeClient();
    var examplesControllerCaller = new ExamplesControllerCaller(client);

    // Ridge wraps the HttpResponseMessage in a convenient wrapper class
    var response = await examplesControllerCaller.CallReturnGivenNumber(10);
    Assert.True(response.IsSuccessStatusCode);
    Assert.AreEqual(10, response.Result);

    // Access the http response directly
    Assert.True(response.HttpResponseMessage.IsSuccessStatusCode);
}

// Equivalent code without using ridge 
[Test]
public async Task CallControllerWithoutRidge()
{
    // Create WebApplicationFactory - https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-5.0#basic-tests-with-the-default-webapplicationfactory
    using var webApplicationFactory = new WebApplicationFactory<Program>();
    // create http client for ridge caller
    var client = webApplicationFactory.CreateClient();

    var response = await client.GetAsync("/returnGivenNumber/?input=10");
    Assert.True(response.IsSuccessStatusCode);
    var responseAsString = await response.Content.ReadAsStringAsync();
    Assert.AreEqual(10, JsonSerializer.Deserialize<int>(responseAsString));
}
```

## Why should I test my controllers?

There is great [article written by Andrew Lock](https://andrewlock.net/should-you-unit-test-controllers-in-aspnetcore/)
which explains why is it good idea to use integration tests to test your controllers.

## Table of contents

![TableOfContent](/table-of-contents.webp)

## Setup

* Mark controller with `[GenerateCaller]` attribute.
* Add tests using `*YourControllerName*Caller`.

`[GenerateCaller]` tells the source generator to generate Controller caller with
name `OriginalClassName`Caller.

## Response

Ridge caller returns one of two object - `HttpCallResponse<TResult>` or `HttpCallResponse`.
Which object is generated depends on the return type of the action -  `HttpCallResponse<TResult>`
is generated when controller returns `ActionResult<TResult>` or `TResult`. `HttpCallResponse`
is generated when action returns `IActionResult`, `void` or `ActionResult`.

> Custom types which implement `IActionResult` are not currently supported.

```csharp
// HttpCallResponse contains following methods and properties
httpCallResponse.HttpResponseMessage
httpCallResponse.ContentAsString
httpCallResponse.StatusCode
httpCallResponse.IsSuccessStatusCode // status code >=200 and <300
httpCallResponse.IsRedirectStatusCode // status code >=300 and <400
httpCallResponse.IsClientErrorStatusCode // status code >=400 and <500
httpCallResponse.IsServerErrorStatusCode // status code >=500 and <600

// HttpCallResponse<TResult> contains the same methods as HttpCallResponse and one additional
httpCallResponse.Result // tries to desirialize response to TResult
```

### Standard HttpResponseMessage

`[GenerateCaller]` has one optional parameter called `UseHttpResponseMessageAsReturnType`. When
this parameter is set to `true` ridge generates methods which returns standard `HttpResponseMessage`
instead of `HttpCallResponse`.

## Exceptions instead of 500 status code

When unexpected exception occurs it is automatically transformed to 500 status code by asp.net core and the exception
details are hidden.
Hiding the exception details and returning 500 is not very useful in tests. That is why
Ridge offers middleware which saves the thrown exception
and then rethrows it (with correct callstack) in test code:

```csharp
// Controller file
[GenerateCaller]
public class ExamplesController : Controller
{
    [HttpGet("ThrowException")]
    public ActionResult ThrowException()
    {
        throw new InvalidOperationException("Exception throw");
    }
}

// Test file
[Test]
public async Task ThrowExceptionTest()
{
    using var webApplicationFactory = new WebApplicationFactory<Program>();
    // notice use of AddExceptionCatching()
    var ridgeHttpClient = webApplicationFactory.AddExceptionCatching().CreateRidgeClient();
    var examplesControllerCaller = new ExamplesControllerCaller(ridgeHttpClient);

    try
    {
        _ = await examplesControllerCaller.CallThrowException();
    }
    catch (InvalidOperationException e)
    {
        Assert.AreEqual("Exception throw", e.Message);
    }
}
```

### How to use

To add this middleware use the following example:

```csharp

// Program.cs
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// WasApplicationCalledFromTestCaller is extension method from ridge package
if (app.WasApplicationCalledFromTestCaller())
{
    app.ThrowExceptionInsteadOfReturning500();
}
app.UseStaticFiles();
app.Run();


// Test file
// Add AddExceptionCatching
var client = webApplicationFactory.AddExceptionCatching().CreateRidgeClient();
```

**Note that any middleware which handles exceptions must be called before the `ThrowExceptionInsteadOfReturning500`
middleware.** For example `app.UseDeveloperExceptionPage();`:

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

if (app.WasApplicationCalledFromTestCaller())
{
    app.ThrowExceptionInsteadOfReturning500();
}

app.UseStaticFiles();
app.Run();
```

## WebApplicationFactory extensions

`Ridge` offers multiple `WebApplicationFactory` extensions which can be used
to setup application settings and dependencies which are used by ridge.

### Configuring Request/Response logging

All requests generated by ridge can be logged to test output window:

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
        using var webApplicationFactory = new WebApplicationFactory<Program>().AddXUnitLogger(_testOutputHelper);
        var ridgeHttpClient = ridgeApplicationFactory.CreateRidgeClient();
        //...
    }
}

// Nunit
public void Test(){
    //...
    using var webApplicationFactory = new WebApplicationFactory<Program>().AddNUnitLogger();
    var ridgeHttpClient = ridgeApplicationFactory.CreateRidgeClient();
    //...
}



// Custom implementation
public class CustomLogWriter : ILogWriter
{
    public void WriteLine(string text)
    {
        // log
    }
}

public void Test(){
    //...
    using var webApplicationFactory = new WebApplicationFactory<Program>().AddCustomLogger(new CustomLogWriter());
    var ridgeHttpClient = ridgeApplicationFactory.CreateRidgeClient();
    //...
}
```

Log example:

```csharp
Request:
Time when request was sent: 15:35:58:167
Method: GET, RequestUri: 'http://localhost/ReturnGivenNumber?input=10', Version: 1.1, Content: System.Net.Http.StringContent, Headers:
{
  ridgeCallId: 2bca4d2d-0cd3-4293-8aae-4c3772db1eb1
  Content-Type: application/json; charset=utf-8
  Content-Length: 2
}
Body:
{}

Response:
Time when response was received: 15:35:58:258
StatusCode: 200, ReasonPhrase: 'OK', Version: 1.1, Content: System.Net.Http.StreamContent, Headers:
{
  Content-Type: application/json; charset=utf-8
  Content-Length: 2
}
Body:
10
```

### Serialization

Serialization library is automatically determined based on asp.net core settings. For custom serialization
implement `IRidgeSerializer` and use `WebApplicationFactory.AddCustomRequestResponseSerializer`.

## Customisation

### Request generation customisation

Ridge generates `HttpRequest` using pipeline composed of `HttpRequestFactoryMiddleware`s.
You can add custom `HttpRequestFactoryMiddleware` to the pipeline:

```csharp
public class AddHeaderHttpRequestFactoryMiddleware : HttpRequestFactoryMiddleware
{
    private readonly string _headerName;
    private readonly string _headerValue;

    public AddHeaderHttpRequestFactoryMiddleware(
        string headerName,
        string headerValue)
    {
        _headerName = headerName;
        _headerValue = headerValue;
    }
    
    public override Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        requestFactoryContext.Headers.Add(_headerName, _headerValue);
        return base.CreateHttpRequest(requestFactoryContext);
    }
}
```

This `HttpRequestFactoryMiddleware` takes provided header and adds it to request generation pipeline.

Example usage of `AddHeaderHttpRequestFactoryMiddleware`:

```csharp
[Test]
public async Task HttpRequestFactoryExample()
{
    using var webApplicationFactory = new WebApplicationFactory<Program>()
       .AddHttpRequestFactoryMiddleware(new AddHeaderHttpRequestFactoryMiddleware("exampleHeader", "exampleHeaderValue"));
    var ridgeHttpClient = webApplicationFactory.CreateRidgeClient();
    var examplesControllerCaller = new ExamplesControllerCaller(ridgeHttpClient);

    // controller finds header by it's name and returns it's value
    var response = await examplesControllerCaller.CallReturnHeader(headerName: "exampleHeader");
    Assert.AreEqual("exampleHeaderValue", response.Result);
}
```

`RequestFactoryContext` is passed to all `HttpRequestFactoryMiddleware`s and contains
information which will be used to generate http request.

Ridge adds to the pipeline initial and final middleware. Initial middleware gathers
information about the request and final middleware generates http request.

Ridge also offers simpler way of adding headers. Instead of creating custom  `HttpRequestFactoryMiddleware`
you can use `WebApplicationFactory<T>.AddHeader` which will add `HttpRequestFactoryMiddleware` for you.
Previous test can be simplified this way:

```csharp
[Test]
public async Task AddHeaderSimple()
{
    using var webApplicationFactory = new WebApplicationFactory<Program>()
       .AddHeader(new HttpHeaderParameter("exampleHeader", "exampleHeaderValue"));
    var ridgeHttpClient = webApplicationFactory.CreateRidgeClient();
    var examplesControllerCaller = new ExamplesControllerCaller(ridgeHttpClient);

    // controller finds header by it's name and returns it's value
    var response = await examplesControllerCaller.CallReturnHeader(headerName: "exampleHeader");
    Assert.AreEqual("exampleHeaderValue", response.Result);
}
```

### Delegation handler

In some cases it can be handy to
add [DelegationHandler](https://learn.microsoft.com/cs-cz/dotnet/api/system.net.http.delegatinghandler?view=net-7.0)
to the `HttpClient`.

How to add DelegationHandler:

```csharp
using var ridgeApplicationFactory = new RidgeApplicationFactory<Program>().CreateRidgeClient(delegatingHandlers: delegationHandler);
```

Ridge provides extension method called `GetRequestDescription` which can be used in any delegation handler used in ridge
calls.
`GetRequestDescription` returns request description:

```csharp
public class ListSeparatedByCommasDelegationHandler : DelegatingHandler
{
  protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage httpRequestMessage,
        CancellationToken cancellationToken)
    {
        //...
        var requestDescription = httpRequestMessage.GetRequestDescription();
        var callerParameters = requestDescription.ParameterProvider.GetCallerParameters();
        //...
    }
}
```

> Ridge uses delegation handler to log requests.

## Caller and request generation

Ridge generates caller based on the signature of actions. For
the following action:

```csharp
public ActionResult<int> ReturnGivenNumber([FromQuery] int input)
```

ridge generates the following method

```csharp
public async Task<HttpCallResponse<int>> CallReturnGivenNumber(int @input)
```

parameters are later analyzed and mapped to query, body and url parameters.

### Parameter mapping

Parameters are mapped by the following heuristic:

* Parameters with attribute `[FromRoute]` are added to `UrlGenerationParameters` dictionary.
* Parameters with attribute `[FromQuery]` are added to `UrlGenerationParameters` dictionary.
* Parameter with attribute `[FromBody]` is added as request body.
* Parameters with attribute `[FromHeader]` are added to request headers.
* Complex parameter without attribute is added as request body.
* Simple parameters without attribute are added to `UrlGenerationParameters` dictionary.

`UrlGenerationParameters` are mapped to route and query parameters
using `_linkGenerator.GetPathByRouteValues("", routeParams);`.
More information about
this [method](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.routing.linkgenerator).

### Request generation tips

* It is reccomended to use [FromXXX] parameters to help ridge map parameters correctly.
* If you are not sure if parameters were mapped correctly you can always use logger (//TODO odkaz) and check test
  output.
* If you are not satisfied with how ridge maps parameters you can always add `RequestFactoryContext` and change the
  generation process.
* Ridge can handle attribute routing and standard routing. It does not matter which one you use.
* Ridge can handle areas.

### Special parameters

When ridge generates caller it automatically removes parameters with attribute `[FromServices]` and
parameters with type `CancellationToken`.

## Customizing caller generation

Attributes `[AddParameterToCaller]` and `[TransformParameterInCaller]` can be used to
customize caller generation process.

### AddParameterToCaller

Controller can be decorated with attribute `[AddParamterToCaller]`. This attribute adds parameter to all methods of
generated caller.

Let's say that we have controller which reads query parameters from `httpContext` instead of binding them to
method parameter:

```csharp
[HttpPost("ReadQueryParameterFromHttpContext")]
public async Task<string> ReadQueryParameterFromHttpContext()
{
    return HttpContext.Request.Query["ExampleQueryParameter"];
}
```

Ridge now generates method `CallReadQueryParameterFromHttpContext()` which does not expect any parameters
because the action does not contain any parameters.

To add the parameter we can use `[AddParameterToCaller]`:

```csharp
[GenerateCaller]
[AddParameterToCaller(typeof(string), "GeneratedParameter", ParameterMapping.MapToQueryOrRouteParameter, Optional = true)]
public class ExamplesController : Controller
{
    [HttpGet("ReadQueryParameterFromHttpContext")]
    public async Task<string> ReadQueryParameterFromHttpContext()
    {
        return HttpContext.Request.Query["GeneratedParameter"];
    }
}
```

Test can use the newly added parameter:

```csharp
[Test]
public async Task ParameterAddedByRidge()
{
    using var webApplicationFactory = new WebApplicationFactory<Program>();

    var ridgeHttpClient = webApplicationFactory.CreateRidgeClient();
    var examplesControllerCaller = new ExamplesControllerCaller(ridgeHttpClient);

    // controller finds header by it's name and returns it's value
    var response = await examplesControllerCaller.CallReadQueryParameterFromHttpContext(GeneratedParameter: "queryParameterValue");
    Assert.AreEqual("queryParameterValue", response.Result);
}
```

`AddParameterToCaller` requires three parameters - type, name, and mapping and one optional parameter which specifies if
the parameter is optional or required.
Parameter mapping can have one of four values - None, MapToQueryOrRouteParameter, MapToBody, MapToHeader. All of these
values

* None - parameter must be mapped manually using custom `HttpRequestFactoryMiddleware` or `DelegationHandler`.
* MapToQueryOrRouteParameter - parameter is added to route or query. Key is equivalent to the name of parameter in
  caller.
* MapToBody - parameter is set as body
* MapToHeader - parameter is added to headers. Key is equivalent to the name of parameter in caller.

If we specified ParameterMapping.None it the previous test we would have to create
custom `HttpRequestFactoryMiddleware`:

```csharp
public class MapQueryParameterHttpRequestFactoryMiddleware : HttpRequestFactoryMiddleware
{
    public override Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        var parameterValue = requestFactoryContext.ParameterProvider
           .GetCallerParameters()
           .GetValueByNameOrDefault<string>("GeneratedParameter");
        if (string.IsNullOrEmpty(parameterValue))
        {
            return base.CreateHttpRequest(requestFactoryContext);
        }
        
        requestFactoryContext.UrlGenerationParameters["GeneratedParameter"] = parameterValue;
        return base.CreateHttpRequest(requestFactoryContext);
    }
}
```

And then we have to use this `MapQueryParameterHttpRequestFactoryMiddleware` in the test:

```csharp
webApplicationFactory.AddHttpRequestFactoryMiddleware(new MapQueryParameterHttpRequestFactoryMiddleware());
```

### TransformParameterInCaller

`TransformParameterInCaller` can be used to decorate controller. This attribute
changes the type and/or name of all parameters which match the chosen type.

`TransformParameterInCaller` is most often used in combination
with [custom model binder](https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/custom-model-binding).
For example let's say we have the following controller and model binder:

```csharp
[GenerateCaller]
public class ExamplesController : Controller
{
    [HttpGet("CallWithCustomModelBinder")]
    public ActionResult<string> CallWithCustomModelBinder(
        [ModelBinder(typeof(BindCountryCodeFromQueryOrHeader))] CountryCode countryCode)
    {
        return countryCode.Value;
    }
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
```

Ridge generates caller for this method which takes parameter `CountryCode`.
This parameter is by default added to request body according to request generation rules (//TODO odkaz).
Problem is that CustomBinder which was used in previous example excepts the parameter as string and it excepts
the parameter to be present in header or query parameter.

We could solve this problem by custom `HttpRequestFactoryMiddleware` which would remove the parameter from body
and then pass it as string to header or query.

Simpler way to achieve the same result is to use transformer which changes `CountryCode` parameter to `string`:

```csharp
[GenerateCaller]
[TransformParameterInCaller(fromType: typeof(CountryCode), toType: typeof(string), ParameterMapping.MapToQueryOrRouteParameter)]
public class ExamplesController : Controller
{
    [HttpGet("CallWithCustomModelBinder")]
    public ActionResult<string> WithCustomModelBinder(
        [ModelBinder(typeof(BindCountryCodeFromQueryOrHeader))] CountryCode countryCode)
    {
        return countryCode.Value;
    }
}
```

With this change Ridge generates method `CallWithCustomModelBinder` which takes parameter `string countryCode`
instead of `CountryCode countryCode`. We can now write test which calls this action:

```csharp
[Test]
public async Task CustomModelBinderTest()
{
    using var webApplicationFactory = new WebApplicationFactory<Program>();

    var ridgeHttpClient = webApplicationFactory.CreateRidgeClient();
    var examplesControllerCaller = new ExamplesControllerCaller(ridgeHttpClient);

    // controller finds header by it's name and returns it's value
    var response = await examplesControllerCaller.CallWithCustomModelBinder("cs-CZ");
    Assert.AreEqual("cs-CZ", response.Result);
}
```

If you would like to test mapping from query and from header you could set `ParameterMapping` to `None` and then write
custom `HttpRequestFactoryMiddleware` which would map the parameter to header or query based on setting.

`[TransformParameterInCaller]` takes three required parameters `fromType`, `toType`, `parameterMapping` and two
optional `GeneratedParameterName`, `Optional`.
`fromType` specifies which parameters will be transformed. `toType` specifies the final type of this parameter which
will be generated in caller.
`parameterMapping` specifies how is the parameter going to be used when generating request.
`GeneratedParameterName` can be used to change the parameter name.  `Optional` can be used to specify if the generated
parameter will be optional.

#### Removing parameters

`[TransformParameterInCaller]` can also remove parameters. To remove parameter set the `toType` to `void`.
For example to remove all parameters of type string you can use:
`[TransformParameterInCaller(typeof(string), typeof(void), ParameterMapping.None)]`

#### Changing default mapping

If you want to change parameter mapping without writing custom `HttpRequestFactoryMiddleware` then you can transform the
parameter to the same type
as it is defined in controller and set custom `ParameterMapping`.

## Custom parameters

Every generated caller method contains optional parameter called `CustomParameters`. Custom parameters can be
used to pass additional parameters to `HttpRequestFactoryMiddleware`.

For example if you need to add header to your request you can use the following:

```csharp
[Test]
public async Task CustomParameter()
{
    using var webApplicationFactory = new WebApplicationFactory<Program>()
       .AddHttpRequestFactoryMiddleware(new AddHeaderFromCustomParameters());
    
    var ridgeHttpClient = webApplicationFactory.CreateRidgeClient();
    var examplesControllerCaller = new ExamplesControllerCaller(ridgeHttpClient);

    // action returns all passed headers
    var response = await examplesControllerCaller.CallReturnAllHeaders(customParameters: new CustomParameter("exampleHeader", "exampleHeaderValue"));

    Assert.AreEqual("exampleHeaderValue", response.Result.First(x => x.key == "exampleHeader").value);
}

public class AddHeaderFromCustomParameters : HttpRequestFactoryMiddleware
{
    public override Task<HttpRequestMessage> CreateHttpRequest(
        IRequestFactoryContext requestFactoryContext)
    {
        var customParameters = requestFactoryContext.ParameterProvider.GetCustomParameters();

        foreach (var customParameter in customParameters)
        {
            requestFactoryContext.Headers.Add(customParameter.Name, customParameter.Value?.ToString());
        }
        
        return base.CreateHttpRequest(requestFactoryContext);
    }
}
```

### ValueTuple as custom parameter

Ridge offer implicit cast from `ValueTuple` to `CustomParamter` therefore you can write:

```csharp
var response = await examplesControllerCaller.CallReturnAllHeaders(customParameters: ("exampleHeader", "exampleHeaderValue"));
```

instead of

```csharp
var response = await examplesControllerCaller.CallReturnAllHeaders(customParameters: new CustomParameter("exampleHeader", "exampleHeaderValue"));
```

### Special custom parameters

Ridge offers special custom parameters which are automatically mapped to headers, query parameters, route parameters and
body.

Example:

```csharp
var response = await examplesControllerCaller.CallExample(customParameters: 
new HttpHeader("exampleHeader", "exampleHeaderValue"),
new QueryOrRouteParameter("exampleQueryOrRouteParameter", "exampleQueryOrRouteParameter"),
new BodyParameter("exampleBodyValue"),
);
```

Those parameters will be mapped automatically without any custom `HttpRequestFactoryMiddleware`.

## Best practices

* Use strongly typed `ActionsResult<T>` when possible.
* Use [FromRoute], [FromQuery], [FromBody] and similar attributes when possible.
* Add logger to check request and response when necessary (TODO link).
* Add ThrowExceptionInsteadOfReturning500 (TODO link).

## Not supported features

* Methods returning custom implementations of IActionResult.
* Projects running on .net 5 and lower.

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

## Contributions

Icon made by [Freepik](https://www.freepik.com) from [www.flaticon.com](https://www.flaticon.com/).
