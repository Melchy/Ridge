namespace Ridge.LogWriter
{
    /// <summary>
    ///     This interface allows custom implementation of log writer.
    ///     LogWriter is used by ridge to log generated request and response from server.
    /// </summary>
    public interface ILogWriter
    {
        /// <summary>
        ///     Write log line to console.
        /// </summary>
        /// <param name="text">Message to be logged.</param>
        public void WriteLine(
            string text);
    }
}
