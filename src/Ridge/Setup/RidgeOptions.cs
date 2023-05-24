using Ridge.HttpRequestFactoryMiddlewares;
using Ridge.LogWriter;
using Ridge.Serialization;
using System;
using System.Collections.Generic;

namespace Ridge.Setup;

/// <summary>
///     Options used to configure Ridge.
/// </summary>
public class RidgeOptions
{
    /// <summary>
    ///     <see cref="HttpRequestFactoryMiddleware" /> which will be used in pipeline to create
    ///     HttpRequestMessage.
    /// </summary>
    public List<HttpRequestFactoryMiddleware> HttpRequestFactoryMiddlewares { get; set; } = new();

    /// <summary>
    ///     Log writer used to log requests and responses.
    /// </summary>
    public ILogWriter? LogWriter { get; set; } = null;

    /// <summary>
    ///     Sets request/response serializer which will be used by ridge. If no serializer is provided then
    ///     ridge uses Json.Net or System.Text.Json based on application settings.
    ///     If Ridge can not determine which serializer application uses then
    ///     System.Text.Json is used as default.
    /// </summary>
    public IRequestResponseSerializer? RequestResponseSerializer { get; set; } = null;

    /// <summary>
    ///    Filter which can be used to decide if exception should be saved or not.
    ///    When filter returns true exception is rethrown instead of returning response.
    /// </summary>
    public Func<Exception, bool>? ExceptionRethrowFilter { get; set; } = null;
}
