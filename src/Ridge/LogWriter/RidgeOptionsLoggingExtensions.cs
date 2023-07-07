using Ridge.Setup;

namespace Ridge.LogWriter;

/// <summary>
///     Ridge logging options extensions.
/// </summary>
public static class RidgeOptionsLoggingExtensions
{
    /// <summary>
    ///     Use custom log writer.
    /// </summary>
    /// <param name="ridgeOptions">Options to edit.</param>
    /// <param name="logWriter">Log writer to use.</param>
    /// <returns></returns>
    public static RidgeOptions UseCustomLogWriter(
        this RidgeOptions ridgeOptions,
        ILogWriter logWriter)
    {
        ridgeOptions.LogWriter = logWriter;
        return ridgeOptions;
    }
}
