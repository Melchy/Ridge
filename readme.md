# Ridge

Source generator which generates C# clients for integration tests.
Supports .NET 6 and newer.

## Install Ridge

Install Ridge using [nuget](https://www.nuget.org/packages/RidgeDotNet/):

```
Install-Package RidgeDotNet
```

Install using .Net Core command line:

```
dotnet add package RidgeDotNet
```

## Wiki

Full documentation can be found in [wiki](TODO).

## Example

```csharp
// --------------------------------------------ExampleController.cs-------------------------------------------------
[GenerateClient] // Notice the attribute
public class ExamplesController : Controller
{
    [HttpGet("ReturnGivenNumber")]
    public ActionResult<int> ReturnGivenNumber(
        [FromQuery] int input)
    {
        return input;
    }
}


// ------------------------------------------Test.cs----------------------------------------------------------------
[Test]
public async Task CallControllerUsingRidge()
{
    using var webApplicationFactory = 
        new WebApplicationFactory<Program>() // WebApplicationFactory - https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests
       .WithRidge(); // add ridge dependencies to WebApplicationFactory
    var client = webApplicationFactory.CreateClient(); // create http client
    var examplesControllerClient = new ExamplesControllerClient(client, webApplicationFactory.Services); // create strongly typed client

    var response = await examplesControllerClient.ReturnGivenNumber(10);
    
    Assert.True(response.IsSuccessStatusCode);
    Assert.AreEqual(10, response.Result); // Response is wrapped in HttpCallResponse<int>
}

// Equivalent code without using Ridge 
[Test]
public async Task CallControllerWithoutRidge()
{
    using var webApplicationFactory = 
        new WebApplicationFactory<Program>(); // WebApplicationFactory - https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests
    var client = webApplicationFactory.CreateClient(); // create http client
    
    var response = await client.GetAsync("/returnGivenNumber/?input=10");
    
    Assert.True(response.IsSuccessStatusCode);
    var responseAsString = await response.Content.ReadAsStringAsync();
    Assert.AreEqual(10, JsonSerializer.Deserialize<int>(responseAsString));
}
```

> More examples can be found in [examples](TODO) folder.

## Setup

* Mark controller with `[GenerateClient]` attribute. This attribute tells the source generator to generate
  class `*YourControllerName*Client` in assembly which contains the controller.
* Call `WithRidge()` extension method on `WebApplicationFactory`.
* Create instance of `*YourControllerName*Client`.
* Create http requests using `*YourControllerName*Client` instance.

## Response

Return type of client method is determined based on the return type of controller.
Following table explains which return type is generated:

| Controller method return type                | Client return type          |
|----------------------------------------------|-----------------------------|
| `ActionResult<TResult>`                      | `HttpCallResponse<TResult>` |
| `TResult`                                    | `HttpCallResponse<TResult>` |
| `ActionResult`                               | `HttpCallResponse`          |
| `IActionResult`                              | `HttpCallResponse`          |
| `void`                                       | `HttpCallResponse`          |
| Custom types which implement `IActionResult` | Not supported               |

`HttpCallResponse` contains following helper methods:

| Method name               | Description                                   |
|---------------------------|-----------------------------------------------|
| `IsSuccessStatusCode`     | Returns true if status code is >=200 and <300 |
| `IsRedirectStatusCode`    | Returns true if status code is >=300 and <400 |
| `IsClientErrorStatusCode` | Returns true if status code is >=400 and <500 |
| `IsServerErrorStatusCode` | Returns true if status code is >=500 and <600 |
| `ContentAsString`         | Returns response content as string            |
| `StatusCode`              | Returns status code                           |
| `HttpResponseMessage`     | Returns standard `HttpResponseMessage`        |

`HttpCallResponse<TResult>` contains same methods as `HttpCallResponse` and one additional:

| Method name | Description                                |
|-------------|--------------------------------------------|
| `Result`    | Tries to deserialize response to `TResult` |

## Best practices

* Use strongly typed `ActionsResult<T>` when possible to enable ridge to generate client with correct return type.
* Use `[FromRoute]`, `[FromQuery]`, `[FromBody]` and similar attributes when possible to ensure correct parameter
  mapping.
* Add logger to check generated request and response when necessary (TODO link).
* Use `ThrowExceptionInsteadOfReturning500` [wiki](TODO) for improved test experience.

## Customization

Following list contains quick summary of supported customizations:

* Transform client parameters [wiki](TODO)
* Add parameter to client [wiki](TODO)
* Edit request generation pipeline [wiki](TODO)
* Remove action parameter from client [wiki](TODO)
* Pass additional parameters to client [wiki](TODO)

## Wiki

Full documentation can be found [in wiki](TODO).

## Features which are currently not supported

> Note that you can always fall back to `WebApplicationFactory` when you need to test something that is not supported by
> ridge.

* Minimal API
* Custom request types. JSON is the only request type currently supported.
* Single action parameter transformations (add parameter to single action or transform parameter in single action)
* `[FromForm]` attributes
* Actions returning custom implementation of `IActionResult`.

### Default request mapping improvements

Ridge supports wide range of parameter mappings but there are some special cases which are currently not supported
by default. Known unsupported mappings are following:

* `[FromQuery]` with array of complex arguments
* Complex types with `[FromXXX]` attributes on properties

Example of`[FromQuery]` with array of complex arguments:

```csharp
public virtual ActionResult NotSupported([FromQuery] IEnumerable<ComplexArgument> complexArguments){
   //..
}
```

Example of complex types with `[FromXXX]` attributes on properties:

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

If you need to use this feature then consider writing custom `HttpRequestFactoryMiddleware` [wiki](TODO) or create an
issue.

## Contributions

Icon made by [Freepik](https://www.freepik.com) from [www.flaticon.com](https://www.flaticon.com/).
