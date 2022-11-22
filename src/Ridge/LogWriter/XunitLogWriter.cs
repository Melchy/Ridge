using Xunit.Abstractions;

namespace Ridge.LogWriter;

/// <summary>
///     Xunit console logger.
/// </summary>
public class XunitLogWriter : ILogWriter
{
    private readonly ITestOutputHelper _outputHelper;

    /// <summary>
    ///     Create new XunitLogWriter.
    ///     To read more about <see cref="ITestOutputHelper" /> see documentation https://xunit.net/docs/capturing-output.
    /// </summary>
    /// <param name="outputHelper">Output helper which will be used for logging.</param>
    public XunitLogWriter(
        ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    /// <inheritdoc />
    public void WriteLine(
        string text)
    {
        _outputHelper.WriteLine(text);
    }
}