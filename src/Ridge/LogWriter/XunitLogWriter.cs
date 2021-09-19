using Xunit.Abstractions;

namespace Ridge.LogWriter
{
    public class XunitLogWriter : ILogWriter
    {
        private readonly ITestOutputHelper _outputHelper;

        public XunitLogWriter(
            ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public void WriteLine(
            string text)
        {
            _outputHelper.WriteLine(text);
        }
    }
}
