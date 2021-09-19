using NUnit.Framework;

namespace Ridge.LogWriter
{
    /// <summary>
    ///     Nunit console logger.
    /// </summary>
    public class NunitLogWriter : ILogWriter
    {
        /// <inheritdoc />
        public void WriteLine(
            string text)
        {
            TestContext.Progress.WriteLine(text);
        }
    }
}
