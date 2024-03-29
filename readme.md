[![build](https://github.com/melchy/ridge/actions/workflows/release.yml/badge.svg)](https://github.com/Melchy/Ridge/actions)
[![](https://img.shields.io/nuget/v/ridgedotnet)](https://www.nuget.org/packages/RidgeDotNet/)
[![](https://img.shields.io/github/v/release/melchy/ridge?label=latest%20release)](https://github.com/Melchy/Ridge/releases)

# Ridge

Ridge is a **source generator** that creates strongly typed HTTP clients for integration tests. HTTP clients generated by Ridge require the
[WebApplicationFactory](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0#basic-tests-with-the-default-webapplicationfactory).
The use of the `WebApplicationFactory` allows Ridge to access internal components of ASP.NET and analyze them. 
This significantly improves route generation and allows implicit support of areas, routing without attributes, and so on.

> Ridge supports .NET 6 and newer.

## Quick links

* [NuGet package](https://www.nuget.org/packages/RidgeDotNet/)
* [Documentation](https://github.com/Melchy/Ridge/wiki)

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
        new WebApplicationFactory<Program>()
            .WithRidge(); // add ridge dependencies to WebApplicationFactory
    var client = webApplicationFactory.CreateClient();
    // create instance of client generated by source generator
    var examplesControllerClient = new ExamplesControllerClient(client, webApplicationFactory.Services); 

    var response = await examplesControllerClient.ReturnGivenNumber(10);
    
    Assert.True(response.IsSuccessStatusCode);
    Assert.AreEqual(10, response.Result);
}
```

## Setup

* Mark controller with the `[GenerateClient]` attribute. This attribute tells the source generator to generate
  class `*YourControllerName*Client` in the assembly which contains the controller.
* Call `WithRidge()` extension method on `WebApplicationFactory`.
* Create instance of `*YourControllerName*Client`.
* Create requests using `*YourControllerName*Client` instance.


> Hint: Use package [`RidgeDotNet.AspNetCore`](https://www.nuget.org/packages/RidgeDotNet.AspNetCore) in your `AspNetCore` project instead of [`RidgeDotNet`](https://www.nuget.org/packages/RidgeDotNet). `RidgeDotNet.AspNetCore` has minimal dependencies, preventing unnecessary test code in your project.

## Best practices

* Use `ActionResult<T>` when possible to enable strongly typed response generation.
* Use `[FromRoute]`, `[FromQuery]`, `[FromBody]`, and similar attributes when possible to ensure correct parameter
  mapping.
* Add a logger to check generated requests and responses when
  necessary. More information [here](https://github.com/Melchy/Ridge/wiki/4.-Request-response-logging).
* Use [`RethrowExceptionInsteadOfReturningHttpResponse`](https://github.com/Melchy/Ridge/wiki/3.-rethrow-exceptions-instead-of-http-response)
  for improved test experience.


## Wiki

Full documentation can be found [in the wiki](https://github.com/Melchy/Ridge/wiki).

## Features that are not currently supported

> Note that you can always fall back to `WebApplicationFactory` when you need to test something that is not supported by
> Ridge.

* Minimal API
* Custom request types. JSON is the only request type currently supported.
* Single action parameter transformations (add parameter to single action or transform parameter in single action)
* `[FromForm]` attributes
* Actions returning custom implementation of `IActionResult`.

### Mappings that are not supported by default

Ridge supports a wide range of parameter mappings, but some special cases are currently not supported by default. 
Known unsupported mappings are the following:

* `[FromQuery]` with an array of complex arguments
* Complex types with `[FromXXX]` attributes on properties

Example of `[FromQuery]` with an array of complex arguments:

```csharp
public virtual ActionResult NotSupported([FromQuery] IEnumerable<ComplexArgument> complexArguments)
{
   //..
}
```

Example of complex types with `[FromXXX]` attributes on properties:

```csharp
public virtual ActionResult NotSupported(Mixed mixed)
{
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

If you need to use this feature then consider writing
[custom `HttpRequestFactoryMiddleware`](https://github.com/Melchy/Ridge/wiki/2.-Request-creation#custom-middlewares)
or creating an issue.

## Contributions

Icon made by [Freepik](https://www.freepik.com) from [www.flaticon.com](https://www.flaticon.com/).