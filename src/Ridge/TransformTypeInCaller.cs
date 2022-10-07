using System;

namespace Ridge;

//TODO adding new parameters
//TODO rename parameter
//TODO set if parameter is optional

/// <summary>
///     Transforms types of parameters in actions from one type to another.
///     If you want to remove parameter from action use typeof(void).
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class TransformTypeInCaller : Attribute
{
    /// <summary>
    /// </summary>
    /// <param name="fromType">Type which will be transformed.</param>
    /// <param name="toType">
    ///     Type to which will <see cref="FromType" /> be transformed. If you want to remove parameter from
    ///     action use typeof(void).
    /// </param>
    public TransformTypeInCaller(
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
    ///     <exception cref="FromType"> type be transformed to. If you want to remove parameter from action use typeof(void).</exception>
    /// </summary>
    public Type ToType { get; }
}
