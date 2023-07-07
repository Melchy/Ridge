using System;

namespace Ridge.AspNetCore.Parameters;

/// <summary>
///     Additional parameters added to action call.
/// </summary>
public class AdditionalParameter
{
    /// <summary>
    ///     Creates additional parameter.
    /// </summary>
    /// <param name="name">Name of parameter.</param>
    /// <param name="value">Value of parameter.</param>
    public AdditionalParameter(
        string name,
        object? value)
    {
        Name = name;
        Value = value;
    }

    /// <summary>
    ///     Name of parameter.
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    ///     Value of parameter.
    /// </summary>
    public object? Value { get; protected set; }

    /// <summary>
    ///     Gets value of parameter with casted to given type or throws if cast is not possible.
    /// </summary>
    /// <typeparam name="TValue">Type of value to be returned.</typeparam>
    /// <returns>Value of parameter with correct type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when cast is not possible.</exception>
    public TValue? GetValueOrThrow<TValue>()
    {
        if (Value is TValue typedValue)
        {
            return (TValue?)typedValue;
        }

        throw new InvalidOperationException(
            $"Could not cast parameter '{Name}' to type '{typeof(TValue)}'. Parameter type is '{Value?.GetType().FullName}'");
    }

    /// <summary>
    ///     Gets value of parameter with casted to given type or returns null if cast is not possible.
    /// </summary>
    /// <typeparam name="TValue">Type of value to be returned.</typeparam>
    /// <returns>Value of parameter with correct type.</returns>
    public TValue? GetValueOrDefault<TValue>()
    {
        if (Value is TValue)
        {
            return (TValue?)Value;
        }

        return (TValue?)(object?)null;
    }

    /// <summary>
    ///     Create additional parameter from value tuple.
    /// </summary>
    /// <param name="from">Value tuple which will be used to create <see cref="AdditionalParameter" />.</param>
    /// <returns>
    ///     <see cref="AdditionalParameter" />
    /// </returns>
    public static implicit operator AdditionalParameter(
        (string Name, object? Value) from)
    {
        return new AdditionalParameter(from.Name, from.Value);
    }
}
