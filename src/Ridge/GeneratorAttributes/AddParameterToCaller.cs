using System;

namespace Ridge.GeneratorAttributes;

/// <summary>
///     Adds parameter to caller methods
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class AddParameterToCaller : Attribute
{
    /// <summary>
    /// </summary>
    /// <param name="parameterType">Type of the parameter in the caller.</param>
    /// <param name="parameterName">Name of the parameter in caller.</param>
    /// <param name="parameterMapping">Select how to map the parameter when creating request.</param>
    public AddParameterToCaller(
        Type parameterType,
        string parameterName,
        ParameterMapping parameterMapping)
    {
        ParameterType = parameterType;
        ParameterName = parameterName;
        ParameterMapping = parameterMapping;
    }


    /// <summary>
    ///     Type of the parameter in the caller.
    /// </summary>
    public Type ParameterType { get; }

    /// <summary>
    ///     Name of the parameter in caller.
    /// </summary>
    public string ParameterName { get; }

    /// <summary>
    ///     Select how to map the parameter when creating request.
    /// </summary>
    public ParameterMapping ParameterMapping { get; }

    /// <summary>
    ///     Set to true if the parameter should be optional. Remember that optional parameter can not be followed by non
    ///     optional parameter.
    /// </summary>
    public bool Optional { get; set; }
}
