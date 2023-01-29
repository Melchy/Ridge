using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ridge.Parameters.CallerParams;

/// <summary>
///     Represents collection of caller parameters.
/// </summary>
public class CallerParameters : IEnumerable<CallerParameter>
{
    private readonly IEnumerable<CallerParameter> _callerParameters;

    /// <summary>
    ///     Creates new <see cref="CallerParameters" />
    /// </summary>
    /// <param name="callerParameters">Parameters.</param>
    public CallerParameters(
        IEnumerable<CallerParameter> callerParameters)
    {
        _callerParameters = callerParameters;
    }

    /// <summary>
    ///     Get parameter by name or return default.
    /// </summary>
    /// <param name="parameterName">Name of parameter to find.</param>
    /// <returns>Parameter or default value.</returns>
    public CallerParameter? GetParameterByNameOrDefault(
        string parameterName)
    {
        return _callerParameters.FirstOrDefault(x => x.Name == parameterName);
    }

    /// <summary>
    ///     Get parameter by name or throw exception.
    /// </summary>
    /// <param name="parameterName">Name of parameter to find.</param>
    /// <returns>Parameter.</returns>
    /// <exception cref="InvalidOperationException">Thrown when parameter is not found.</exception>
    public CallerParameter GetParameterByNameOrThrow(
        string parameterName)
    {
        var parameter = GetParameterByNameOrDefault(parameterName);
        if (parameter == null)
        {
            throw new InvalidOperationException($"Could not find parameter with name '{parameterName}'.");
        }

        return parameter;
    }

    /// <summary>
    ///     Get first parameter by type or default.
    /// </summary>
    /// <typeparam name="TType">Type of parameter to find. Type must match type passed to caller.</typeparam>
    /// <returns>Parameter or default.</returns>
    public CallerParameter? GetFirstParameterByTypeOrDefault<TType>()
    {
        return _callerParameters.FirstOrDefault(x => x.Type == typeof(TType));
    }

    /// <summary>
    ///     Get first parameter by type or throw exception.
    /// </summary>
    /// <typeparam name="TType">Type of parameter to find. Type must match type passed to caller.</typeparam>
    /// <returns>Parameter.</returns>
    /// <exception cref="InvalidOperationException">Thrown when parameter with provided type is not found.</exception>
    public CallerParameter GetFirstParameterByTypeOrThrow<TType>()
    {
        var callerParameter = GetFirstParameterByTypeOrDefault<TType>();
        if (callerParameter == null)
        {
            throw new InvalidOperationException($"Could not find parameter with type '{typeof(TType)}'.");
        }

        return callerParameter;
    }

    /// <summary>
    ///     Get parameters by type.
    /// </summary>
    /// <typeparam name="TType">Type which will be used to find parameters. Type must match the type passed to caller.</typeparam>
    /// <returns>Collection of <see cref="CallerParameter" /> with type TType.</returns>
    public IEnumerable<CallerParameter> GetParametersByType<TType>()
    {
        return _callerParameters.Where(x => x.Type == typeof(TType));
    }

    /// <summary>
    ///     Find parameter by name and get it's value casted to provided type.
    /// </summary>
    /// <param name="parameterName">Name which will be searched for.</param>
    /// <typeparam name="TValue">Value of parameter will be casted to this type.</typeparam>
    /// <returns>Casted value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when parameter is not found or if we can not cast the value.</exception>
    public TValue? GetValueByNameOrThrow<TValue>(
        string parameterName)
    {
        var parameter = GetParameterByNameOrThrow(parameterName);
        return parameter.GetValueOrThrow<TValue>();
    }

    /// <summary>
    ///     Find parameter by name and get it's value casted to provided type.
    /// </summary>
    /// <param name="parameterName">Name which will be searched for.</param>
    /// <typeparam name="TValue">Value of parameter will be casted to this type.</typeparam>
    /// <returns>Casted value or default when parameter is not found or can not be casted.</returns>
    public TValue? GetValueByNameOrDefault<TValue>(
        string parameterName)
    {
        var parameter = GetParameterByNameOrDefault(parameterName);
        if (parameter == null)
        {
            return default;
        }

        return parameter.GetValueOrDefault<TValue?>() ?? default(TValue);
    }

    /// <summary>
    ///     Find first parameter by type and get it's value casted to provided type. If the parameter can not be found or can
    ///     not be casted exception is thrown.
    /// </summary>
    /// <typeparam name="TValue">Value used to find the parameter. Found parameter is also casted to this type.</typeparam>
    /// <returns>Casted value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when parameter is not found or if we can not cast the value.</exception>
    public TValue? GetFirstValueByTypeOrThrow<TValue>()
    {
        var parameter = GetFirstParameterByTypeOrThrow<TValue>();
        return parameter.GetValueOrThrow<TValue>();
    }

    /// <summary>
    ///     Find first parameter by type and get it's value casted to provided type.
    /// </summary>
    /// <typeparam name="TValue">Value used to find the parameter. Found parameter is also casted to this type.</typeparam>
    /// <returns>Casted value or default.</returns>
    public TValue? GetFirstValueByTypeOrDefault<TValue>()
    {
        var parameter = GetFirstParameterByTypeOrDefault<TValue>();
        if (parameter == null)
        {
            return default;
        }

        return parameter.GetValueOrThrow<TValue?>() ?? default(TValue);
    }


    /// <inheritdoc />
    public IEnumerator<CallerParameter> GetEnumerator()
    {
        return _callerParameters.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
