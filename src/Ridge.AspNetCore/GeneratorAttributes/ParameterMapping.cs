namespace Ridge.AspNetCore.GeneratorAttributes;

/// <summary>
///     Decides how to map parameter when creating request.
/// </summary>
public enum ParameterMapping
{
    /// <summary>
    ///     Parameter will be ignored by Ridge.
    ///     To use this parameter it is necessary to add custom
    ///     HttpRequestFactoryMiddleware or DelegationHandler and map the parameter manually.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Parameter will be added to `UrlGenerationParameters` in IRequestFactoryContext.
    ///     Those parameter are then mapped to Route or Query parameters using _linkGenerator.GetPathByRouteValues("",
    ///     routeParams);
    ///     where link generator is https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.routing.linkgenerator.
    ///     Key is equivalent to the parameter name in generated client class.
    /// </summary>
    MapToQueryOrRouteParameter = 1,

    /// <summary>
    ///     Value will be used as parameter body.
    /// </summary>
    MapToBody = 2,

    /// <summary>
    ///     Parameter will be added as header to the request.
    ///     Key is equivalent to the parameter name in generated client class.
    /// </summary>
    MapToHeader = 3,
}
