using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace RidgeSourceGenerator.Dtos;

public class ControllerToGenerate : IEquatable<ControllerToGenerate>
{
    public readonly IEnumerable<(IMethodSymbol MethodSymbol, int MethodHash)> PublicMethods;
    public readonly string Name;
    public readonly string FullyQualifiedName;
    public readonly string Namespace;

    public bool UseHttpResponseMessageAsReturnType;
    public Dictionary<ITypeSymbol, ParameterTransformation> ParameterTransformations;
    public AddParameter[] ParametersToAdd;

    public readonly int AttributesHash;
    private readonly int _attributesClassNameAndMethodsHashCode;

    public ControllerToGenerate(
        string name,
        string fullyQualifiedName,
        string @namespace,
        IEnumerable<(IMethodSymbol MethodSymbol, int MethodHash)> publicMethods,
        ImmutableArray<KeyValuePair<string, TypedConstant>> mainAttributeSettings,
        IEnumerable<AttributeData> typeTransformerAttributes,
        IEnumerable<AttributeData> addParameterAttributes,
        int attributesClassNameAndMethodsHashCode,
        int attributesHash)
    {
        AttributesHash = attributesHash;
        PublicMethods = publicMethods;
        _attributesClassNameAndMethodsHashCode = attributesClassNameAndMethodsHashCode;
        Name = name;
        FullyQualifiedName = fullyQualifiedName;
        Namespace = @namespace;
        UseHttpResponseMessageAsReturnType = GetUseHttpResponseMessageAsReturnType(mainAttributeSettings);
        ParameterTransformations = GetTypeTransformations(typeTransformerAttributes);
        ParametersToAdd = GetParametersToAdd(addParameterAttributes);
    }

    public AddParameter[] GetParametersToAdd(
        IEnumerable<AttributeData> addParameterAttributes)
    {
        var result = new List<AddParameter>();
        foreach (var addParameterAttribute in addParameterAttributes)
        {
            if (addParameterAttribute.ConstructorArguments.Length != 3)
            {
                continue;
            }

            var parameterType = addParameterAttribute.ConstructorArguments[0].Value as ITypeSymbol;
            var parameterName = (string?)addParameterAttribute.ConstructorArguments[1].Value;
            var parameterMapping = (int?)addParameterAttribute.ConstructorArguments[2].Value;
            var optional = addParameterAttribute.NamedArguments.FirstOrDefault(x => x.Key == "Optional").Value.Value as bool? ?? false;
            if (parameterType == null || parameterName == null || parameterMapping == null)
            {
                continue;
            }

            var parameterMappingEnum = GetParameterMappingOrDefault(parameterMapping.Value);
            if (parameterMappingEnum == null)
            {
                continue;
            }

            result.Add(new AddParameter(parameterName, parameterType, parameterMappingEnum.Value, optional));
        }

        return result.ToArray();
    }

    private static ParameterMapping? GetParameterMappingOrDefault(
        int parameterMapping)
    {
        if (Enum.IsDefined(typeof(ParameterMapping), parameterMapping))
        {
            return (ParameterMapping)parameterMapping;
        }

        return null;
    }

    private static Dictionary<ITypeSymbol, ParameterTransformation> GetTypeTransformations(
        IEnumerable<AttributeData> typeTransformerAttributes)
    {
// false positive https://github.com/dotnet/roslyn-analyzers/issues/4845
#pragma warning disable RS1024
        var result = new Dictionary<ITypeSymbol, ParameterTransformation>(SymbolEqualityComparer.Default);
#pragma warning restore RS1024
        foreach (var typeTransformerAttribute in typeTransformerAttributes)
        {
            if (typeTransformerAttribute.ConstructorArguments.Length != 3)
            {
                continue;
            }

            var fromType = typeTransformerAttribute.ConstructorArguments[0].Value as ITypeSymbol;
            var toType = typeTransformerAttribute.ConstructorArguments[1].Value as ITypeSymbol;
            var parameterMapping = (int?)typeTransformerAttribute.ConstructorArguments[2].Value;
            var newName = typeTransformerAttribute.NamedArguments.FirstOrDefault(x => x.Key == "GeneratedParameterName").Value.Value as string;
            var optional = typeTransformerAttribute.NamedArguments.FirstOrDefault(x => x.Key == "Optional").Value.Value as bool?;
            if (fromType == null || toType == null || parameterMapping == null)
            {
                continue;
            }

            var parameterMappingEnum = GetParameterMappingOrDefault(parameterMapping.Value);
            if (parameterMappingEnum == null)
            {
                continue;
            }

            result[fromType] = new ParameterTransformation(toType, newName, parameterMappingEnum.Value, optional);
        }

        return result;
    }

    private static bool GetUseHttpResponseMessageAsReturnType(
        ImmutableArray<KeyValuePair<string, TypedConstant>> mainAttributeSettings)
    {
        var result = mainAttributeSettings.FirstOrDefault(x =>
                x.Key == "UseHttpResponseMessageAsReturnType")
           .Value.Value as bool?;
        return result ?? false;
    }

    public bool Equals(
        ControllerToGenerate? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return _attributesClassNameAndMethodsHashCode == other.GetHashCode();
    }

    public override bool Equals(
        object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((ControllerToGenerate)obj);
    }

    public override int GetHashCode()
    {
        return _attributesClassNameAndMethodsHashCode;
    }
}
