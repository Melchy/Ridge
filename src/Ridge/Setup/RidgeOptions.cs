using Ridge.HttpRequestFactoryMiddlewares;
using Ridge.LogWriter;
using Ridge.Serialization;
using System.Collections.Generic;

namespace Ridge.Setup;

/// <summary>
///     Options used to configure Ridge.
/// </summary>
public class RidgeOptions
{
    /// <summary>
    ///     Ridge will catch exception thrown by server and rethrow them in test
    ///     as regular exception with correct call stack. When you set this value to true then you
    ///     need too use app.ThrowExceptionInsteadOfReturning500(); see docs for mor information.
    /// </summary>
    public bool ThrowExceptionInsteadOfReturning500 { get; set; } = false;

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
    ///     Custom request response serializer. If no serializer is provided then
    ///     ridge uses Json.Net or System.Text.Json based on application settings.
    ///     If Ridge can not determine which serializer your application uses then
    ///     System.Text.Json is used as default.
    /// </summary>
    public IRequestResponseSerializer? RequestResponseSerializer { get; set; } = null;
}
