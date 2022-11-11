using Microsoft.CodeAnalysis;

namespace RidgeSourceGenerator.Dtos;

public class MethodToGenerate : IEquatable<MethodToGenerate>
{
    public readonly AddParameter[] ParametersToAdd;
    public readonly IMethodSymbol PublicMethod;
    public readonly bool UseHttpResponseMessageAsReturnType;
    public readonly IDictionary<string, ParameterTransformation> ParameterTransformations;
    public readonly string ContainingControllerFullyQualifiedName;

    private readonly int _methodHash;
    private readonly int _classAttributesHash;
    private readonly Lazy<string> _generatedMethod;

    public MethodToGenerate(
        IMethodSymbol publicMethod,
        bool useHttpResponseMessageAsReturnType,
        IDictionary<string, ParameterTransformation> parameterTransformations,
        string containingControllerFullyQualifiedName,
        int methodHash,
        AddParameter[] parametersToAdd,
        int classAttributesHash,
        CancellationToken cancellationToken)
    {
        ParametersToAdd = parametersToAdd;
        _classAttributesHash = classAttributesHash;
        PublicMethod = publicMethod;
        UseHttpResponseMessageAsReturnType = useHttpResponseMessageAsReturnType;
        ParameterTransformations = parameterTransformations;
        ContainingControllerFullyQualifiedName = containingControllerFullyQualifiedName;
        _methodHash = methodHash;
        _generatedMethod = new Lazy<string>(() => MethodGenerationHelper.GenerateMethod(this, cancellationToken));
    }

    public string GenerateMethod(
        CancellationToken cancellationToken)
    {
        return _generatedMethod.Value;
    }

    public bool Equals(
        MethodToGenerate? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }


        if (ContainingControllerFullyQualifiedName != other.ContainingControllerFullyQualifiedName ||
            _methodHash != other._methodHash ||
            _classAttributesHash != other._classAttributesHash)
        {
            return false;
        }

        return true;
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

        return Equals((MethodToGenerate)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = ContainingControllerFullyQualifiedName.GetHashCode();
            hashCode = (hashCode * 397) ^ _methodHash;
            hashCode = (hashCode * 397) ^ _classAttributesHash;
            return hashCode;
        }
    }
}
