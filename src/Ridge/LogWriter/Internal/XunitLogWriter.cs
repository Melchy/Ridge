using Xunit.Abstractions;

namespace Ridge.LogWriter.Internal;

internal class XunitLogWriter : ILogWriter
{
    private readonly ITestOutputHelper _outputHelper;

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
