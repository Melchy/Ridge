using System;

namespace Ridge.AspNetCore.GeneratorAttributes;

/// <summary>
///     Transforms types of parameters in actions from one type to another.
///     If you want to remove parameter from action use typeof(void).
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class TransformActionParameter : Attribute
{
    /// <summary>
    /// </summary>
    /// <param name="fromType">Type which will be transformed.</param>
    /// <param name="toType">
    ///     Type to which will <see cref="FromType" /> be transformed. If you want to remove parameter from
    ///     action use typeof(void).
    /// </param>
    /// <param name="parameterMapping">Select how to map the parameter when creating request.</param>
    public TransformActionParameter(
        Type fromType,
        Type toType,
        ParameterMapping parameterMapping)
    {
        FromType = fromType;
        ToType = toType;
        ParameterMapping = parameterMapping;
    }

    /// <summary>
    ///     Type which will be transformed.
    /// </summary>
    public Type FromType { get; }

    /// <summary>
    ///     Type to which will
    ///     <see cref="FromType" /> type be transformed to. If you want to remove parameter from action use typeof(void).
    /// </summary>
    public Type ToType { get; }

    /// <summary>
    /// Select how to map the parameter when creating request.
    /// </summary>
    public ParameterMapping ParameterMapping { get; }

    /// <summary>
    ///     Set to true if the parameter should be optional.
    /// </summary>
    public bool Optional { get; set; }

    /// <summary>
    ///     Set this property to change the parameter name.
    /// </summary>
    public string? GeneratedParameterName { get; set; }
}
