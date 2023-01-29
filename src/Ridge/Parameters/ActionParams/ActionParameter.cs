using System;
using System.Reflection;

namespace Ridge.Parameters.ActionParams;

/// <summary>
///     Represents controller parameter.
/// </summary>
public class ActionParameter
{
    /// <summary>
    ///     Create new <see cref="ActionParameter" />
    /// </summary>
    /// <param name="parameterInfo">Information about controller parameter.</param>
    /// <param name="name">Name.</param>
    /// <param name="type">Type.</param>
    public ActionParameter(
        ParameterInfo parameterInfo,
        string name,
        Type type)
    {
        ParameterInfo = parameterInfo;
        Name = name;
        Type = type;
    }

    /// <summary>
    ///     Information about controller parameter.
    /// </summary>
    public ParameterInfo ParameterInfo { get; }

    /// <summary>
    ///     Name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Type.
    /// </summary>
    public Type Type { get; }
}
