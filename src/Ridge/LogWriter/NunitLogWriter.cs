using NUnit.Framework;

namespace Ridge.LogWriter
{
    /// <summary>
    ///     Nunit console logger. Which uses TestContext.WriteLine.
    /// </summary>
    public class NunitLogWriter : ILogWriter
    {
        /// <inheritdoc />
        public void WriteLine(
            string text)
        {
            TestContext.WriteLine(text);
        }
    }
}
