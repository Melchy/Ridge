using Ridge.LogWriter;
using Ridge.Setup;
using System;

// Namespace is correct
namespace Microsoft.AspNetCore.Mvc.Testing;

/// <summary>
/// Exception rethrow options.
/// </summary>
public static class RidgeOptionsExceptionRethrowExtensions
{
    /// <summary>
    /// Adds filter which is used to decide if exception should be saved or not.
    /// </summary>
    /// <param name="ridgeOptions">Options to edit.</param>
    /// <param name="exceptionFilter">Filter to apply. When filter returns true exception is rethrown instead of returning http response.</param>
    /// <returns></returns>
    public static RidgeOptions UseExceptionRethrowFilter(
        this RidgeOptions ridgeOptions,
        Func<Exception, bool> exceptionFilter)
    {
        ridgeOptions.ExceptionRethrowFilter = exceptionFilter;
        return ridgeOptions;
    }
}
