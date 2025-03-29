using Microsoft.CodeAnalysis;
using RidgeSourceGenerator.GenerationHelpers;

namespace RidgeSourceGenerator.Dtos;

public class MethodToGenerate : IEquatable<MethodToGenerate>
{
    public readonly AddParameter[] ParametersToAdd;
    public readonly IMethodSymbol PublicMethod;
    public readonly bool UseHttpResponseMessageAsReturnType;
    public readonly Dictionary<ITypeSymbol, ParameterTransformation> ParameterTransformations;
    public readonly string ContainingControllerFullyQualifiedName;
    public readonly string ControllerName;
    public bool SingleMethodInController { get; set; }

    private readonly int _methodHash;
    private readonly int _classAttributesHash;

    public MethodToGenerate(
        IMethodSymbol publicMethod,
        bool useHttpResponseMessageAsReturnType,
        Dictionary<ITypeSymbol, ParameterTransformation> parameterTransformations,
        string containingControllerFullyQualifiedName,
        int methodHash,
        AddParameter[] parametersToAdd,
        int classAttributesHash,
        string controllerName)
    {
        ParametersToAdd = parametersToAdd;
        _classAttributesHash = classAttributesHash;
        ControllerName = controllerName;
        PublicMethod = publicMethod;
        UseHttpResponseMessageAsReturnType = useHttpResponseMessageAsReturnType;
        ParameterTransformations = parameterTransformations;
        ContainingControllerFullyQualifiedName = containingControllerFullyQualifiedName;
        _methodHash = methodHash;
    }

    public string GenerateMethod(
        bool isExtensionMethod,
        CancellationToken cancellationToken)
    {
        return MethodGenerationHelper.GenerateMethod(this, isExtensionMethod, cancellationToken);
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
