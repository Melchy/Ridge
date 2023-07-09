using Ridge.Extensions.Nunit;
using Ridge.LogWriter;
using Ridge.Setup;

// Namespace is correct
namespace Microsoft.AspNetCore.Mvc.Testing;

/// <summary>
///     Ridge logging options extensions.
/// </summary>
public static class RidgeOptionsNunitExtensions
{
    /// <summary>
    ///     Use Nunit logger which uses TestContext.Out.WriteLine. See
    ///     https://docs.nunit.org/articles/nunit/writing-tests/TestContext.html#out.
    /// </summary>
    /// <param name="ridgeOptions">Options to edit.</param>
    /// <returns></returns>
    public static RidgeOptions UseNunitLogWriter(
        this RidgeOptions ridgeOptions)
    {
        ridgeOptions.UseCustomLogWriter(new NunitLogWriter());
        return ridgeOptions;
    }
}
