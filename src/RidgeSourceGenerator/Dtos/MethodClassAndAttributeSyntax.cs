using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RidgeSourceGenerator;

public class MethodClassAndAttributeSyntax : IEquatable<MethodClassAndAttributeSyntax>
{
    public SemanticModel SemanticModel;
    public ClassDeclarationSyntax? ClassDeclarationSyntax { get; set; }
    public List<(MethodDeclarationSyntax methodDeclarationSyntax, int methodDeclarationHash)> Methods { get; set; } = new List<(MethodDeclarationSyntax methodDeclarationSyntax, int methodDeclaration)>();
    public readonly int CachedHashCode;

    public MethodClassAndAttributeSyntax(
        ClassDeclarationSyntax classDeclarationSyntax,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        SemanticModel = semanticModel;
        ClassDeclarationSyntax = classDeclarationSyntax;
        if (ClassDeclarationSyntax == null)
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var attributesDeclaration = ClassDeclarationSyntax.AttributeLists.ToString();
        var className = ClassDeclarationSyntax.Identifier.ToString();
        var methodDeclarations = string.Join(";",
            ClassDeclarationSyntax?.DescendantNodes()
               .OfType<MethodDeclarationSyntax>()
               .Select(x =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string methodDeclaration;
                    var methodAsString = x.ToString();
                    var index = methodAsString.IndexOf("{");
                    if (index == -1)
                    {
                        methodDeclaration = methodAsString;
                    }
                    else
                    {
                        methodDeclaration = x.ToString().Substring(0, index);
                    }

                    Methods.Add((x, methodDeclaration.GetHashCode()));
                    return methodDeclaration;
                }) ?? Array.Empty<string>());
        cancellationToken.ThrowIfCancellationRequested();
        CachedHashCode = GetHashCode(attributesDeclaration, className, methodDeclarations);
    }

    public bool Equals(
        MethodClassAndAttributeSyntax? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return CachedHashCode == other.CachedHashCode;
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

        return Equals((MethodClassAndAttributeSyntax)obj);
    }

    public override int GetHashCode()
    {
        return CachedHashCode;
    }

    private int GetHashCode(
        string attributesDeclaration,
        string className,
        string methodDeclarations)
    {
        unchecked
        {
            var hashCode = 397 ^ attributesDeclaration.GetHashCode();
            hashCode = (hashCode * 397) ^ className.GetHashCode();
            hashCode = (hashCode * 397) ^ methodDeclarations.GetHashCode();
            return hashCode;
        }
    }
}
