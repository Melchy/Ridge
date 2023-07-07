using Ridge.LogWriter;
using Ridge.Setup;

namespace Ridge.Extensions.Nunit;

/// <summary>
///     Ridge logging options extensions.
/// </summary>
public static class RidgeOptionsLoggingExtensions
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
        ridgeOptions.LogWriter = new NunitLogWriter();
        return ridgeOptions;
    }
}
