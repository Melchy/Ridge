using Microsoft.AspNetCore.Mvc.Testing;

namespace Ridge.WebApplicationFactoryTools;

/// <summary>
///     This class is just <see cref="WebApplicationFactoryClientOptions" />. We can not use
///     <see cref="WebApplicationFactoryClientOptions" />
///     because we can not ensure that caller contains reference to containing package.
/// </summary>
public class WebAppFactoryClientOptions : WebApplicationFactoryClientOptions
{
}
