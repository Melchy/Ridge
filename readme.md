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
    // Create standard webAppFactory
    // https://docs.microsoft.com/cs-cz/aspnet/core/test/integration-tests?view=aspnetcore-5.0
    var webAppFactory = new WebApplicationFactory<Startup>();
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
Note that you must specify `[FromXXX]` attribute for every argument which should be bound.

### Documentation
Find more information in documentation.
