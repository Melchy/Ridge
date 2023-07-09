using Microsoft.Extensions.Options;
using Ridge.Setup;
using System.Collections.Generic;

namespace Ridge.LogWriter.Internal;

internal class CompositeLogWriter : ILogWriter
{
    private readonly IEnumerable<ILogWriter> _logWriters;

    public CompositeLogWriter(
        IOptions<RidgeOptions>? ridgeOptions)
    {
        _logWriters = ridgeOptions?.Value.LogWriter ?? new List<ILogWriter>();
    }
    
    public void WriteLine(
        string text)
    {
        foreach (var logWriter in _logWriters)
        {
            logWriter.WriteLine(text);
        }
    }
}
