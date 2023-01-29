using NUnit.Framework;

namespace Ridge.LogWriter.Internal;

internal class NunitLogWriter : ILogWriter
{
    public void WriteLine(
        string text)
    {
        TestContext.Out.WriteLine(text);
    }
}
