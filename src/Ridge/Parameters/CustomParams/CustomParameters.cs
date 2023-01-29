using Ridge.Parameters.CallerParams;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ridge.Parameters.CustomParams;

/// <summary>
///     Custom parameter passed to caller.
/// </summary>
public class CustomParameters : IEnumerable<CustomParameter>
{
    private readonly IEnumerable<CustomParameter> _customParameters;

    /// <summary>
    ///     Crate new <see cref="CallerParameters" />
    /// </summary>
    /// <param name="customParameters"></param>
    public CustomParameters(
        IEnumerable<CustomParameter> customParameters)
    {
        _customParameters = customParameters;
    }

    /// <summary>
    ///     Get parameter by name or return default.
    /// </summary>
    /// <param name="parameterName">Name of parameter to find.</param>
    /// <returns>Parameter or default value.</returns>
    public CustomParameter? GetParameterByNameOrDefault(
        string parameterName)
    {
        return _customParameters.FirstOrDefault(x => x.Name == parameterName);
    }

    /// <summary>
    ///     Get parameter by name or throw exception.
    /// </summary>
    /// <param name="parameterName">Name of parameter to find.</param>
    /// <returns>Parameter.</returns>
    /// <exception cref="InvalidOperationException">Thrown when parameter is not found.</exception>
    public CustomParameter GetParameterByNameOrThrow(
        string parameterName)
    {
        var parameter = GetParameterByNameOrDefault(parameterName);
        if (parameter == null)
        {
            throw new InvalidOperationException($"Could not find custom parameter with name '{parameterName}'.");
        }

        return parameter;
    }

    /// <summary>
    ///     Get all parameters with given name.
    /// </summary>
    /// <param name="parameterName">Name used to find the parameters.</param>
    /// <returns>Collection of <see cref="CustomParameter" /> with provided name.</returns>
    public IEnumerable<CustomParameter> GetParametersByName(
        string parameterName)
    {
        return _customParameters.Where(x => x.Name == parameterName);
    }

    /// <summary>
    ///     Get first parameter by type or default.
    /// </summary>
    /// <typeparam name="TType">Type of parameter to find.</typeparam>
    /// <returns>Parameter or default.</returns>
    public CustomParameter? GetFirstParameterByTypeOrDefault<TType>()
    {
        return _customParameters.FirstOrDefault(x => x.GetType() == typeof(TType));
    }

    /// <summary>
    ///     Get first parameter by type of value or default.
    /// </summary>
    /// <typeparam name="TType">Type of value to find.</typeparam>
    /// <returns>Parameter or default.</returns>
    public CustomParameter? GetFirstParameterByTypeOfValueOrDefault<TType>()
    {
        return _customParameters.FirstOrDefault(x => x.Value?.GetType() == typeof(TType));
    }

    /// <summary>
    ///     Get first parameter by type or throw exception.
    /// </summary>
    /// <typeparam name="TType">Type of parameter to find.</typeparam>
    /// <returns>Parameter.</returns>
    /// <exception cref="InvalidOperationException">Thrown when parameter with provided type is not found.</exception>
    public CustomParameter GetFirstParameterByTypeOrThrow<TType>()
    {
        var callerParameter = GetFirstParameterByTypeOrDefault<TType>();
        if (callerParameter == null)
        {
            throw new InvalidOperationException($"Could not find parameter with type '{typeof(TType)}'.");
        }

        return callerParameter;
    }

    /// <summary>
    ///     Get first parameter by type of value or throw exception.
    /// </summary>
    /// <typeparam name="TType">Type of value to find.</typeparam>
    /// <returns>Parameter.</returns>
    /// <exception cref="InvalidOperationException">Thrown when value with provided type is not found.</exception>
    public CustomParameter GetFirstParameterByTypeOfValueOrThrow<TType>()
    {
        var callerParameter = GetFirstParameterByTypeOfValueOrDefault<TType>();
        if (callerParameter == null)
        {
            throw new InvalidOperationException($"Could not find parameter with type '{typeof(TType)}'.");
        }

        return callerParameter;
    }

    /// <summary>
    ///     Get parameters by type.
    /// </summary>
    /// <typeparam name="TType">Type which will be used to find parameters.</typeparam>
    /// <returns>Collection of <see cref="CallerParameter" /> with type TType.</returns>
    public IEnumerable<TType> GetParametersByType<TType>()
    {
        return _customParameters.Where(x => x.GetType() == typeof(TType)).Cast<TType>();
    }

    /// <summary>
    ///     Get parameters which value matches the provided type.
    /// </summary>
    /// <typeparam name="TType">Type which will be used to find parameters.</typeparam>
    /// <returns>Collection of <see cref="CallerParameter" /> with values matching TType.</returns>
    public IEnumerable<CustomParameter> GetParametersByTypeOfValue<TType>()
    {
        return _customParameters.Where(x => x.Value?.GetType() == typeof(TType));
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
    public IEnumerator<CustomParameter> GetEnumerator()
    {
        return _customParameters.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
