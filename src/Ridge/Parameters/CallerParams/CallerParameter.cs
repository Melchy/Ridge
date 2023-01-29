using Ridge.GeneratorAttributes;
using System;

namespace Ridge.Parameters.CallerParams;

/// <summary>
///     Represents parameter passed to caller.
/// </summary>
public class CallerParameter
{
    /// <summary>
    ///     Create new <see cref="CallerParameter" />.
    /// </summary>
    /// <param name="name">Name</param>
    /// <param name="type">Type</param>
    /// <param name="value">Value</param>
    /// <param name="addedOrTransformedParameterMapping">Select how to map the parameter when creating request.</param>
    public CallerParameter(
        string name,
        Type type,
        object? value,
        ParameterMapping? addedOrTransformedParameterMapping)
    {
        Name = name;
        Type = type;
        Value = value;
        AddedOrTransformedParameterMapping = addedOrTransformedParameterMapping;
    }

    /// <summary>
    ///     Name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Type.
    /// </summary>
    public Type Type { get; set; }

    /// <summary>
    ///     Value.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    ///     Select how to map the parameter when creating request.
    /// </summary>
    public ParameterMapping? AddedOrTransformedParameterMapping { get; }

    /// <summary>
    ///     Get value casted to provided type or throw exception.
    /// </summary>
    /// <typeparam name="TValue">Value will be casted to this type.</typeparam>
    /// <returns>Value casted to the provided type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when value can not be casted.</exception>
    public TValue? GetValueOrThrow<TValue>()
    {
        if (Value is TValue typedValue)
        {
            return (TValue?)typedValue;
        }

        throw new InvalidOperationException(
            $"Could not cast parameter '{Name}' to type '{typeof(TValue)}'. Parameter type is '{Value?.GetType().FullName ?? Type.FullName}'");
    }

    /// <summary>
    ///     Get value casted to provided type or return default.
    /// </summary>
    /// <typeparam name="TValue">Value will be casted to this type.</typeparam>
    /// <returns>Value casted to the provided type.</returns>
    public TValue? GetValueOrDefault<TValue>()
    {
        if (Value is TValue)
        {
            return (TValue?)Value;
        }

        return (TValue?)(object?)null;
    }
}
