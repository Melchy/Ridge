using Ridge.Parameters;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace Ridge.HttpRequestFactoryMiddlewares;

/// <summary>
///     Contains information about request. This Context will be later used to generate
///     <see cref="HttpRequestMessage" />.
/// </summary>
public interface IRequestFactoryContext
{
    /// <summary>
    ///     Contains client and action parameters and arguments information.
    /// </summary>
    ParameterProvider ParameterProvider { get; }

    /// <summary>
    ///     Id used to identify call. This id is added to header.
    /// </summary>
    Guid CallId { get; }

    /// <summary>
    ///     Controller method which should be called when request is processed.
    /// </summary>
    MethodInfo CalledControllerMethodInfo { get; }

    /// <summary>
    ///     Request body.
    /// </summary>
    object? Body { get; set; }

    /// <summary>
    ///     If true body will be send in request. By default every time you set
    ///     body then this will be set to true.
    ///     This variable is important
    ///     because there is difference between not setting body
    ///     and setting body to null.
    /// </summary>
    bool RequestHasBody { get; set; }

    /// <summary>
    ///     Parameters used to Generate URL. This dictionary must contain all query parameters,
    ///     route parameters, controller name, action name and area.
    ///     This dictionary is used to generated URL using _linkGenerator.GetPathByRouteValues("", routeParams);
    ///     where link generator is https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.routing.linkgenerator
    /// </summary>
    IDictionary<string, object?> UrlGenerationParameters { get; set; }

    /// <summary>
    ///     Http content type.
    /// </summary>
    string HttpContentType { get; }

    /// <summary>
    ///     Http headers.
    /// </summary>
    HttpRequestHeaders Headers { get; set; }

    /// <summary>
    ///     Http method.
    /// </summary>
    string HttpMethod { get; set; }
}
