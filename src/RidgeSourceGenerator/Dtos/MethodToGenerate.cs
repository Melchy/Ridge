using Microsoft.CodeAnalysis;

namespace RidgeSourceGenerator;

public class MethodToGenerate : IEquatable<MethodToGenerate>
{
    public readonly AddParameter[] ParametersToAdd;
    public readonly IMethodSymbol PublicMethod;
    public bool UseHttpResponseMessageAsReturnType;
    public IDictionary<string, ParameterTransformation> ParameterTransformations;
    public readonly string ContainingControllerFullyQualifiedName;
    public readonly int MethodHash;

    private readonly Lazy<string> _generatedMethod;

    public MethodToGenerate(
        IMethodSymbol publicMethod,
        bool useHttpResponseMessageAsReturnType,
        IDictionary<string, ParameterTransformation> parameterTransformations,
        string containingControllerFullyQualifiedName,
        int methodHash,
        AddParameter[] parametersToAdd,
        CancellationToken cancellationToken)
    {
        ParametersToAdd = parametersToAdd;
        PublicMethod = publicMethod;
        UseHttpResponseMessageAsReturnType = useHttpResponseMessageAsReturnType;
        ParameterTransformations = parameterTransformations;
        ContainingControllerFullyQualifiedName = containingControllerFullyQualifiedName;
        MethodHash = methodHash;
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


        var everythingIsSame = ContainingControllerFullyQualifiedName == other.ContainingControllerFullyQualifiedName && MethodHash == other.MethodHash
                                                                                                                      && UseHttpResponseMessageAsReturnType == other.UseHttpResponseMessageAsReturnType;

        if (!everythingIsSame)
        {
            return false;
        }

        if (ParameterTransformations.Count != other.ParameterTransformations.Count)
        {
            return false;
        }

        foreach (var keyValuePair in ParameterTransformations)
        {
            var isPresent = other.ParameterTransformations.TryGetValue(keyValuePair.Key, out var otherParamTransformation);
            if (!isPresent)
            {
                return false;
            }

            if (!keyValuePair.Value.Equals(otherParamTransformation))
            {
                return false;
            }
        }

        if (ParametersToAdd.Length != other.ParametersToAdd.Length)
        {
            return false;
        }

        for (int i = 0; i < ParametersToAdd.Length - 1; i++)
        {
            if (!ParametersToAdd[i].Equals(other.ParametersToAdd[i]))
            {
                return false;
            }
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
            return (ContainingControllerFullyQualifiedName.GetHashCode() * 397) ^ MethodHash;
        }
    }
}
