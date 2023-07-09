using Ridge.AspNetCore.Options;
using Ridge.AspNetCore.Serialization;
using Ridge.HttpRequestFactoryMiddlewares;
using Ridge.LogWriter;
using Ridge.Serialization;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TestWebApplication")]
[assembly: InternalsVisibleTo("RidgeTests")]

namespace Ridge.Setup;

/// <summary>
///     Options used to configure Ridge.
/// </summary>
public class RidgeOptions : RidgeAspNetCoreOptions
{
    /// <summary>
    ///     <see cref="HttpRequestFactoryMiddleware" /> which will be used in pipeline to create
    ///     HttpRequestMessage.
    /// </summary>
    public List<HttpRequestFactoryMiddleware> HttpRequestFactoryMiddlewares { get; set; } = new();

    /// <summary>
    ///     Log writer used to log requests and responses.
    /// </summary>
    public List<ILogWriter> LogWriter { get; set; } = new();

    /// <summary>
    ///     Sets request/response serializer which will be used by ridge. If no serializer is provided then
    ///     ridge uses Json.Net or System.Text.Json based on application settings.
    ///     If Ridge can not determine which serializer application uses then
    ///     System.Text.Json is used as default.
    /// </summary>
    public IRequestResponseSerializer? RequestResponseSerializer { get; set; } = null;
}
