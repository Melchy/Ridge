﻿## This example shows how to alter the request generation process

In this example we used package [Asp.Versioning.Mvc](https://www.nuget.org/packages/Asp.Versioning.Mvc/).
This package allows us to define attribute `ApiVersion` which specifies the version 
which must be passed in request header or query parameter.
Following example shows setup of apiVersioning from program.cs file:

```csharp
builder.Services.AddApiVersioning(o =>
{
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.DefaultApiVersion = new ApiVersion(1, 0);
    o.ReportApiVersions = true;
    o.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version"),
        new HeaderApiVersionReader("X-Version"));
});
```

As you can see client must define query parameter `api-version` or header `X-Version` to specify the version of the API.

Ridge does not support VersionApi by default, but we can easily alter the request generation by adding
custom `HttpRequestFactoryMiddleware`. See `ApiVersionMiddleware` file.

Full documentation can be found [here](https://github.com/Melchy/Ridge/wiki/2.-Request-creation).