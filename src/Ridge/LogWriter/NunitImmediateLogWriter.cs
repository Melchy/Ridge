﻿using NUnit.Framework;

namespace Ridge.LogWriter;

/// <summary>
///     Nunit console logger. Which uses TestContext.Progress.WriteLine.
/// </summary>
public class NunitImmediateLogWriter : ILogWriter
{
    /// <inheritdoc />
    public void WriteLine(
        string text)
    {
        TestContext.Progress.WriteLine(text);
    }
}