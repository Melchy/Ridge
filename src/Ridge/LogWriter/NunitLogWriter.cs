using NUnit.Framework;

namespace Ridge.LogWriter
{
    public class NunitLogWriter : ILogWriter
    {
        public void WriteLine(
            string text)
        {
            TestContext.Progress.WriteLine(text);
        }
    }

    public class CustomLogWriter : ILogWriter
    {
        public void WriteLine(string text)
        {
            // write line somewhere
        }
    }
}
