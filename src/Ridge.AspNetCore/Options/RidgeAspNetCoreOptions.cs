using System;

namespace Ridge.AspNetCore.Options;

/// <summary>
/// Options for Ridge.AspNetCore
/// </summary>
public class RidgeAspNetCoreOptions
{
    /// <summary>
    ///    Filter which can be used to decide if exception should be saved or not.
    ///    When filter returns true exception is rethrown instead of returning response.
    /// </summary>
    public Func<Exception, bool>? ExceptionRethrowFilter { get; set; }
}
