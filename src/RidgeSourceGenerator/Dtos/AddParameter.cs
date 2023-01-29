using Microsoft.CodeAnalysis;

namespace RidgeSourceGenerator.Dtos;

public struct AddParameter : IEquatable<AddParameter>
{
    public ParameterMapping ParameterMapping;
    public bool IsOptional;
    public string Name;
    public ITypeSymbol Type;

    public AddParameter(
        string name,
        ITypeSymbol type,
        ParameterMapping parameterMapping,
        bool isOptional)
    {
        ParameterMapping = parameterMapping;
        IsOptional = isOptional;
        Name = name;
        Type = type;
    }

    public bool Equals(
        AddParameter other)
    {
        return IsOptional == other.IsOptional && Name == other.Name && SymbolEqualityComparer.Default.Equals(Type, other.Type) && ParameterMapping == other.ParameterMapping;
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
            hashCode = (hashCode * 397) ^ SymbolEqualityComparer.Default.GetHashCode(Type);
            hashCode = (hashCode * 397) ^ ParameterMapping.GetHashCode();
            return hashCode;
        }
    }
}
