﻿using Ridge.LogWriter;
using Xunit.Abstractions;

namespace Ridge.Extensions.Xunit;

/// <summary>
///     Add XUnit logger which will be used to log requests and responses generated by ridge.
///     To read more about <see cref="ITestOutputHelper" /> see documentation https://xunit.net/docs/capturing-output.
/// </summary>
public class XunitLogWriter : ILogWriter
{
    private readonly ITestOutputHelper _outputHelper;

    /// <summary>
    ///     Create xunit log writer.
    /// </summary>
    /// <param name="outputHelper"><see cref="ITestOutputHelper" /> used to write logs.</param>
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