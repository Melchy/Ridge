using Ridge.AspNetCore.Parameters;
using Ridge.AspNetCore.Response;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.AspNetCore;

/// <summary>
///     Helpers used to simplify generated code.
/// </summary>
public interface IApplicationClient
{
    /// <summary>
    ///     Creates http request for the given method and calls the server.
    /// </summary>
    /// <param name="methodName">Controller method for which the request will be generated.</param>
    /// <param name="callParameters">Argument types of controller method.</param>
    /// <param name="additionalParameters">Additional parameters passed to client.</param>
    /// <param name="parameterAndTransformationInfo">Information about parameters and transformations of parameters.</param>
    /// <typeparam name="TController">Controller to be called</typeparam>
    /// <typeparam name="TReturn">Return type</typeparam>
    /// <returns>Returns the response from server.</returns>
    Task<HttpCallResponse<TReturn>> CallAction<TReturn, TController>(
        string methodName,
        Type[] callParameters,
        AdditionalParameter[] additionalParameters,
        IEnumerable<RawParameterAndTransformationInfo> parameterAndTransformationInfo);

    /// <summary>
    ///     Creates http request for the given method and calls the server.
    /// </summary>
    /// <param name="methodName">Controller method for which the request will be generated.</param>
    /// <param name="callParameters">Argument types of controller method.</param>
    /// <param name="additionalParameters">Additional parameters passed to client.</param>
    /// <param name="parameterAndTransformationInfo">Information about parameters and transformations of parameters.</param>
    /// <typeparam name="TController">Controller to be called</typeparam>
    /// <returns>Returns the response from server.</returns>
    Task<HttpCallResponse> CallAction<TController>(
        string methodName,
        Type[] callParameters,
        AdditionalParameter[] additionalParameters,
        IEnumerable<RawParameterAndTransformationInfo> parameterAndTransformationInfo);

    /// <summary>
    ///     Creates http request for the given method and calls the server.
    /// </summary>
    /// <param name="methodName">Controller method for which the request will be generated.</param>
    /// <param name="callParameters">Argument types of controller method.</param>
    /// <param name="additionalParameters">Additional parameters passed to client.</param>
    /// <param name="parameterAndTransformationInfo">Information about parameters and transformations of parameters.</param>
    /// <typeparam name="TController">Controller to be called</typeparam>
    /// <returns>Returns the response from server.</returns>
    Task<HttpResponseMessage> CallActionWithHttpResponseMessageResult<TController>(
        string methodName,
        Type[] callParameters,
        AdditionalParameter[] additionalParameters,
        IEnumerable<RawParameterAndTransformationInfo> parameterAndTransformationInfo);
}
