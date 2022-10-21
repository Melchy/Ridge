using System;

namespace Ridge;

/// <summary>
///     Adds parameter to caller methods
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class AddParameterToCaller : Attribute
{
    /// <summary>
    /// </summary>
    /// <param name="parameterType">Type of the parameter in the caller</param>
    /// <param name="parameterName">Name of the parameter in caller</param>
    public AddParameterToCaller(
        Type parameterType,
        string parameterName)
    {
        ParameterType = parameterType;
        ParameterName = parameterName;
    }


    /// <summary>
    ///     Type of the parameter in the caller
    /// </summary>
    public Type ParameterType { get; }

    /// <summary>
    ///     Name of the parameter in caller
    /// </summary>
    public string ParameterName { get; }

    /// <summary>
    ///     Set to true if the parameter should be optional. Remember that optional parameter can not be followed by non
    ///     optional parameter.
    /// </summary>
    public bool Optional { get; set; }
}
