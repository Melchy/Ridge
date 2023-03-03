using Ridge.GeneratorAttributes;
using System;
using System.Linq;
using System.Reflection;

namespace Ridge;

/// <summary>
///     Raw parameters created by source generator.
/// </summary>
public class RawParameterAndTransformationInfo
{
    private RawParameterAndTransformationInfo(
        string? originalName,
        Type? originalType,
        bool? originalIsOptional,
        object? passedValue,
        Type typeInClient,
        string nameInClient,
        bool wasDeleted,
        bool isOptionalInClient,
        bool isTransformedOrAdded,
        ParameterInfo? originalParameterInfo,
        ParameterMapping? addedOrTransformedParameterMapping)
    {
        OriginalName = originalName;
        OriginalType = originalType;
        OriginalIsOptional = originalIsOptional;
        PassedValue = passedValue;
        TypeInClient = typeInClient;
        NameInClient = nameInClient;
        WasDeleted = wasDeleted;
        IsOptionalInClient = isOptionalInClient;
        IsTransformedOrAdded = isTransformedOrAdded;
        OriginalParameterInfo = originalParameterInfo;
        AddedOrTransformedParameterMapping = addedOrTransformedParameterMapping;
    }

    /// <summary>
    ///     OriginalName
    /// </summary>
    public string? OriginalName { get; }

    /// <summary>
    ///     OriginalType
    /// </summary>
    public Type? OriginalType { get; }

    /// <summary>
    ///     OriginalIsOptional
    /// </summary>
    public bool? OriginalIsOptional { get; }

    /// <summary>
    ///     PassedValue
    /// </summary>
    public object? PassedValue { get; }

    /// <summary>
    ///     TypeInClient
    /// </summary>
    public Type TypeInClient { get; }

    /// <summary>
    ///     NameInClient
    /// </summary>
    public string NameInClient { get; }

    /// <summary>
    ///     WasDeleted
    /// </summary>
    public bool WasDeleted { get; }

    /// <summary>
    ///     IsOptionalInClient
    /// </summary>
    public bool IsOptionalInClient { get; }

    /// <summary>
    ///     IsTransformedOrAdded
    /// </summary>
    public bool IsTransformedOrAdded { get; }

    /// <summary>
    ///     OriginalParameterInfo
    /// </summary>
    public ParameterInfo? OriginalParameterInfo { get; }

    /// <summary>
    ///     Information about mapping for added or transformed parameter.
    /// </summary>
    public ParameterMapping? AddedOrTransformedParameterMapping { get; }

    /// <summary>
    ///     Create <see cref="RawParameterAndTransformationInfo" />.
    /// </summary>
    /// <param name="methodName">MethodName</param>
    /// <param name="originalName">originalName</param>
    /// <param name="originalType">originalType</param>
    /// <param name="originalIsOptional">originalIsOptional</param>
    /// <param name="passedValue">passedValue</param>
    /// <param name="typeInClient">typeInClient</param>
    /// <param name="nameInClient">nameInClient</param>
    /// <param name="wasDeleted">wasDeleted</param>
    /// <param name="isOptionalInClient">isOptionalInClient</param>
    /// <param name="isTransformedOrAdded">isTransformedOrAdded</param>
    /// <param name="actionParametersTypes">actionParametersTypes</param>
    /// <param name="addedOrTransformedParameterMapping">Information about mapping for transformed or added parameters.</param>
    /// <typeparam name="TController">TController</typeparam>
    /// <returns></returns>
    public static RawParameterAndTransformationInfo Create<TController>(
        string? originalName,
        Type? originalType,
        bool? originalIsOptional,
        int? addedOrTransformedParameterMapping,
        object? passedValue,
        Type typeInClient,
        string nameInClient,
        bool wasDeleted,
        bool isOptionalInClient,
        bool isTransformedOrAdded,
        string methodName,
        Type[] actionParametersTypes)
    {
        var methodInfo = typeof(TController).GetMethod(methodName, actionParametersTypes);
        if (methodInfo == null)
        {
            throw new InvalidOperationException($"Could not find public non static method {methodName} on controller {typeof(TController).FullName}");
        }

        var actionParameterInfo = methodInfo.GetParameters().FirstOrDefault(x => x.Name == originalName);

        return new RawParameterAndTransformationInfo(originalName,
            originalType,
            originalIsOptional,
            passedValue,
            typeInClient,
            nameInClient,
            wasDeleted,
            isOptionalInClient,
            isTransformedOrAdded,
            actionParameterInfo,
            (ParameterMapping?)addedOrTransformedParameterMapping);
    }
}
