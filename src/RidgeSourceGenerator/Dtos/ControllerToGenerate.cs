using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace RidgeSourceGenerator;

public class ControllerToGenerate : IEquatable<ControllerToGenerate>
{
    public readonly IEnumerable<(IMethodSymbol MethodSymbol, int MethodHash)> PublicMethods;
    private readonly int _cachedHashCode;
    public readonly string Name;
    public readonly string FullyQualifiedName;
    public readonly string Namespace;

    public bool UseHttpResponseMessageAsReturnType;
    public IDictionary<string, ParameterTransformation> ParameterTransformations;
    public AddParameter[] ParametersToAdd;

    public ControllerToGenerate(
        string name,
        string fullyQualifiedName,
        string @namespace,
        IEnumerable<(IMethodSymbol MethodSymbol, int MethodHash)> publicMethods,
        ImmutableArray<KeyValuePair<string, TypedConstant>> mainAttributeSettings,
        IEnumerable<AttributeData> typeTransformerAttributes,
        IEnumerable<AttributeData> addParameterAttributes,
        int cachedHashCode)
    {
        PublicMethods = publicMethods;
        _cachedHashCode = cachedHashCode;
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
            if (addParameterAttribute.ConstructorArguments.Length != 2)
            {
                continue;
            }
            var parameterType = (ISymbol?)addParameterAttribute.ConstructorArguments[0].Value;
            var parameterName = (string?)addParameterAttribute.ConstructorArguments[1].Value;
            var optional = addParameterAttribute.NamedArguments.FirstOrDefault(x => x.Key == "Optional").Value.Value as bool? ?? false;
            if (parameterType == null || parameterName == null)
            {
                continue;
            }

            result.Add(new AddParameter(parameterName, parameterType.ToDisplayString(), optional));
        }

        return result.ToArray();
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

public struct AddParameter : IEquatable<AddParameter>
{
    public bool IsOptional;
    public string Name;
    public string Type;

    public AddParameter(
        string name,
        string type,
        bool isOptional)
    {
        IsOptional = isOptional;
        Name = name;
        Type = type;
    }

    public bool Equals(
        AddParameter other)
    {
        return IsOptional == other.IsOptional && Name == other.Name && Type == other.Type;
    }

    public override bool Equals(
        object? obj)
    {
        return obj is AddParameter other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = IsOptional.GetHashCode();
            hashCode = (hashCode * 397) ^ Name.GetHashCode();
            hashCode = (hashCode * 397) ^ Type.GetHashCode();
            return hashCode;
        }
    }
}

public struct ParameterTransformation : IEquatable<ParameterTransformation>
{
    public readonly string ToType;
    public readonly string? NewName;
    public readonly bool Optional;

    public ParameterTransformation(
        string toType,
        string? newName,
        bool optional)
    {
        Optional = optional;
        NewName = newName;
        ToType = toType;
    }

    public bool Equals(
        ParameterTransformation other)
    {
        return ToType == other.ToType && NewName == other.NewName && Optional == other.Optional;
    }

    public override bool Equals(
        object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((ParameterTransformation)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = ToType.GetHashCode();
            hashCode = (hashCode * 397) ^ (NewName != null ? NewName.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ Optional.GetHashCode();
            return hashCode;
        }
    }
}
