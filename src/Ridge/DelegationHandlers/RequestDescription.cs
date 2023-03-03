using Ridge.Parameters;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;

namespace Ridge.DelegationHandlers;

/// <summary>
///     Contains all the information about request gathered by ridge
/// </summary>
public class RequestDescription
{
    /// <summary>
    /// Key which identified this object in <see cref="HttpRequestOptions"/>.
    /// </summary>
    public const string OptionsKey = nameof(RequestDescription);

    
    /// <summary>
    /// Action and client arguments.
    /// </summary>
    public ParameterProvider ParameterProvider { get; }
    
    
    /// <summary>
    ///     Id used to identify call. This id is was to header.
    /// </summary>
    public Guid CallId { get; }
    
    /// <summary>
    ///     Controller method which should be called when request is processed.
    /// </summary>
    public MethodInfo CalledControllerMethodInfo { get; }

    /// <summary>
    /// Parameters which were used to Generate URL. This dictionary must contain all query parameters,
    /// route parameters, controller name, action name and area.
    /// </summary>
    public IReadOnlyDictionary<string, object?> UrlGenerationParameters { get; }

    /// <summary>
    /// Creates <see cref="RequestDescription"/> 
    /// </summary>
    /// <param name="callId">Id used to identify call. This id is was to header.</param>
    /// <param name="parameterProvider">Action and client arguments.</param>
    /// <param name="urlGenerationParameters">Parameters which were used to Generate URL. This dictionary must contain all query parameters, route parameters, controller name, action name and area.</param>
    /// <param name="calledControllerMethodInfo">Controller method which should be called when request is processed.</param>
    public RequestDescription(
        IReadOnlyDictionary<string, object?> urlGenerationParameters,
        MethodInfo calledControllerMethodInfo,
        Guid callId,
        ParameterProvider parameterProvider)
    {
        UrlGenerationParameters = urlGenerationParameters;
        CalledControllerMethodInfo = calledControllerMethodInfo;
        CallId = callId;
        ParameterProvider = parameterProvider;
    }
}
