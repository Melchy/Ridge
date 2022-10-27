using Ridge.LogWriter;
using Ridge.Serialization;
using System;
using System.Net.Http;

namespace Ridge.Caller;

/// <summary>
///     Provides information about ControllerCaller
/// </summary>
public interface IControllerCaller
{
    /// <summary>
    ///     HttpClient
    /// </summary>
    HttpClient HttpClient { get; set; }

    /// <summary>
    ///     ServiceProvider
    /// </summary>
    IServiceProvider ServiceProvider { get; set; }

    /// <summary>
    ///     LogWriter
    /// </summary>
    ILogWriter? LogWriter { get; set; }

    /// <summary>
    ///     RidgeSerializer
    /// </summary>
    IRequestResponseSerializer? RidgeSerializer { get; set; }

    /// <summary>
    ///     RequestBuilder
    /// </summary>
    RequestBuilder RequestBuilder { get; set; }
}
