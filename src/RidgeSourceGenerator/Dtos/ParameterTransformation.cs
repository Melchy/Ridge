using Microsoft.CodeAnalysis;

namespace RidgeSourceGenerator.Dtos;

public struct ParameterTransformation : IEquatable<ParameterTransformation>
{
    public ParameterMapping ParameterMapping;
    public readonly ITypeSymbol ToType;
    public readonly string? NewName;
    public readonly bool? Optional;

    public ParameterTransformation(
        ITypeSymbol toType,
        string? newName,
        ParameterMapping parameterMapping,
        bool? optional)
    {
        ParameterMapping = parameterMapping;
        Optional = optional;
        NewName = newName;
        ToType = toType;
    }

    public bool Equals(
        ParameterTransformation other)
    {
        return SymbolEqualityComparer.Default.Equals(ToType, other.ToType) && NewName == other.NewName && Optional == other.Optional && ParameterMapping == other.ParameterMapping;
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
            var hashCode = (int)ParameterMapping;
            hashCode = (hashCode * 397) ^ SymbolEqualityComparer.Default.GetHashCode(ToType);
            hashCode = (hashCode * 397) ^ (NewName?.GetHashCode() ?? 1);
            hashCode = (hashCode * 397) ^ Optional.GetHashCode();
            return hashCode;
        }
    }
}
