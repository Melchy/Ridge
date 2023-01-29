using System;

namespace Ridge.Parameters.CustomParams;

/// <summary>
///     Custom parameters added to action call.
/// </summary>
public class CustomParameter
{
    /// <summary>
    ///     Creates custom parameter.
    /// </summary>
    /// <param name="name">Name of parameter.</param>
    /// <param name="value">Value of parameter.</param>
    public CustomParameter(
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
    /// <returns>Value of custom parameter with correct type.</returns>
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
    /// <returns>Value of custom parameter with correct type.</returns>
    public TValue? GetValueOrDefault<TValue>()
    {
        if (Value is TValue)
        {
            return (TValue?)Value;
        }

        return (TValue?)(object?)null;
    }

    /// <summary>
    ///     Create custom parameter from value tuple.
    /// </summary>
    /// <param name="from">Value tuple which will be used to create <see cref="CustomParameter" />.</param>
    /// <returns>
    ///     <see cref="CustomParameter" />
    /// </returns>
    public static implicit operator CustomParameter(
        (string Name, object? Value) from)
    {
        return new CustomParameter(from.Name, from.Value);
    }
}
