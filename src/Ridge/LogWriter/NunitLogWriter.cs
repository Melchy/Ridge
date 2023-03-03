using NUnit.Framework;

namespace Ridge.LogWriter;

/// <summary>
///     Nunit logger which uses TestContext.Out.WriteLine. See
///     https://docs.nunit.org/articles/nunit/writing-tests/TestContext.html#out.
/// </summary>
public class NunitLogWriter : ILogWriter
{
    /// <inheritdoc />
    public void WriteLine(
        string text)
    {
        TestContext.Out.WriteLine(text);
    }
}
