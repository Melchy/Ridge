using System;

namespace Ridge.GeneratorAttributes;

/// <summary>
///     Transforms types of parameters in actions from one type to another.
///     If you want to remove parameter from action use typeof(void).
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class TransformParameterInCaller : Attribute
{
    /// <summary>
    /// </summary>
    /// <param name="fromType">Type which will be transformed.</param>
    /// <param name="toType">
    ///     Type to which will <see cref="FromType" /> be transformed. If you want to remove parameter from
    ///     action use typeof(void).
    /// </param>
    public TransformParameterInCaller(
        Type fromType,
        Type toType)
    {
        FromType = fromType;
        ToType = toType;
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
    ///     Set to true if the parameter should be optional. Remember that optional parameter can not be followed by non optional parameter.
    /// </summary>
    public bool Optional { get; set; }

    /// <summary>
    ///     Set this property to change the parameter name.
    /// </summary>
    public string? GeneratedParameterName { get; set; }
}
