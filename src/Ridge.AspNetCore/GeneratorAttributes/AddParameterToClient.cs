using System;

namespace Ridge.AspNetCore.GeneratorAttributes;

/// <summary>
///     Adds parameter to client methods
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class AddParameterToClient : Attribute
{
    /// <summary>
    /// </summary>
    /// <param name="parameterType">Type of the parameter in the client.</param>
    /// <param name="parameterName">Name of the parameter in client.</param>
    /// <param name="parameterMapping">Select how to map the parameter when creating request.</param>
    public AddParameterToClient(
        Type parameterType,
        string parameterName,
        ParameterMapping parameterMapping)
    {
        ParameterType = parameterType;
        ParameterName = parameterName;
        ParameterMapping = parameterMapping;
    }


    /// <summary>
    ///     Type of the parameter in the client.
    /// </summary>
    public Type ParameterType { get; }

    /// <summary>
    ///     Name of the parameter in client.
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
