using Ridge.Parameters.CallerParams;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ridge.Parameters.ActionParams;

/// <summary>
///     Collection of <see cref="ActionParameter" />.
/// </summary>
public class ActionParameters : IEnumerable<ActionParameter>
{
    private readonly IEnumerable<ActionParameter> _actionParameters;

    /// <summary>
    ///     Create new <see cref="ActionParameters" />.
    /// </summary>
    /// <param name="actionParameters"></param>
    public ActionParameters(
        IEnumerable<ActionParameter> actionParameters)
    {
        _actionParameters = actionParameters;
    }

    /// <summary>
    ///     Get first parameter by type or default.
    /// </summary>
    /// <typeparam name="TType">Type of parameter to find. Type must match type passed to caller.</typeparam>
    /// <returns>Parameter or default.</returns>
    public ActionParameter? GetFirstParameterByTypeOrDefault<TType>()
    {
        return _actionParameters.FirstOrDefault(x => x.Type == typeof(TType));
    }

    /// <summary>
    ///     Get first parameter by type or throw exception.
    /// </summary>
    /// <typeparam name="TType">Type of parameter to find. Type must match type passed to caller.</typeparam>
    /// <returns>Parameter.</returns>
    /// <exception cref="InvalidOperationException">Thrown when parameter with provided type is not found.</exception>
    public ActionParameter GetFirstParameterByTypeOrThrow<TType>()
    {
        var actionParameter = GetFirstParameterByTypeOrDefault<TType>();
        if (actionParameter == null)
        {
            throw new InvalidOperationException($"Could not find parameter with type '{typeof(TType)}'.");
        }

        return actionParameter;
    }

    /// <summary>
    ///     Get parameters by type.
    /// </summary>
    /// <typeparam name="TType">Type which will be used to find parameters. Type must match the type passed to caller.</typeparam>
    /// <returns>Collection of <see cref="CallerParameter" /> with type TType.</returns>
    public IEnumerable<ActionParameter> GetParametersByType<TType>()
    {
        return _actionParameters.Where(x => x.Type == typeof(TType));
    }

    /// <summary>
    ///     Get parameter by name or return default.
    /// </summary>
    /// <param name="parameterName">Name of parameter to find.</param>
    /// <returns>Parameter or default value.</returns>
    public ActionParameter? GetParameterByNameOrDefault(
        string parameterName)
    {
        return _actionParameters.FirstOrDefault(x => x.Name == parameterName);
    }

    /// <summary>
    ///     Get parameter by name or throw exception.
    /// </summary>
    /// <param name="parameterName">Name of parameter to find.</param>
    /// <returns>Parameter.</returns>
    /// <exception cref="InvalidOperationException">Thrown when parameter is not found.</exception>
    public ActionParameter GetParameterByNameOrThrow(
        string parameterName)
    {
        var parameter = GetParameterByNameOrDefault(parameterName);
        if (parameter == null)
        {
            throw new InvalidOperationException($"Could not find parameter with name '{parameterName}'.");
        }

        return parameter;
    }

    /// <inheritdoc />
    public IEnumerator<ActionParameter> GetEnumerator()
    {
        return _actionParameters.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
