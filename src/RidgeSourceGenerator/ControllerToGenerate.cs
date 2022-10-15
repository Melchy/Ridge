using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace RidgeSourceGenerator;

public class ControllerToGenerate : IEquatable<ControllerToGenerate>
{
    public readonly List<IMethodSymbol> PublicMethods;
    private readonly int _cachedHashCode;
    public readonly string Name;
    public readonly string FullyQualifiedName;
    public readonly string Namespace;

    public bool UseHttpResponseMessageAsReturnType;
    public IDictionary<string, ParameterTransformation> ParameterTransformations;

    public ControllerToGenerate(
        string name,
        string fullyQualifiedName,
        string @namespace,
        List<IMethodSymbol> publicMethods,
        ImmutableArray<KeyValuePair<string, TypedConstant>> mainAttributeSettings,
        IEnumerable<AttributeData> typeTransformerAttributes,
        int cachedHashCode)
    {
        PublicMethods = publicMethods;
        _cachedHashCode = cachedHashCode;
        Name = name;
        FullyQualifiedName = fullyQualifiedName;
        Namespace = @namespace;
        UseHttpResponseMessageAsReturnType = GetUseHttpResponseMessageAsReturnType(mainAttributeSettings);
        ParameterTransformations = GetTypeTransformations(typeTransformerAttributes);
    }

    private IDictionary<string, ParameterTransformation> GetTypeTransformations(
        IEnumerable<AttributeData> typeTransformerAttributes)
    {
        var result = new Dictionary<string, ParameterTransformation>();
        foreach (var typeTransformerAttribute in typeTransformerAttributes)
        {
            var fromType = (ISymbol?)typeTransformerAttribute.ConstructorArguments[0].Value;
            var toType = (ISymbol?)typeTransformerAttribute.ConstructorArguments[1].Value;
            var newName = typeTransformerAttribute.NamedArguments.FirstOrDefault(x => x.Key == "GeneratedParameterName").Value.Value as string;
            var optional = typeTransformerAttribute.NamedArguments.FirstOrDefault(x => x.Key == "Optional").Value.Value as bool? ?? false;
            if (fromType == null || toType == null)
            {
                continue;
            }

            result[fromType.Name] = new ParameterTransformation(toType.Name, newName, optional);
        }

        return result;
    }

    private bool GetUseHttpResponseMessageAsReturnType(
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

        return _cachedHashCode == other.GetHashCode();
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
        return _cachedHashCode;
    }
}

public class ParameterTransformation
{
    public readonly string ToType;
    public readonly string? NewName;
    public readonly bool Optional;
    public int NumberOfUsagesOfThisName = 0;

    public ParameterTransformation(
        string toType,
        string? newName,
        bool optional)
    {
        Optional = optional;
        NewName = newName;
        ToType = toType;
    }
}
