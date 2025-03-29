using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RidgeSourceGenerator.Dtos;

public class MethodClassAndAttributeSyntax : IEquatable<MethodClassAndAttributeSyntax>
{
    public SemanticModel SemanticModel;
    public ClassDeclarationSyntax? ClassDeclarationSyntax { get; set; }
    public List<(MethodDeclarationSyntax methodDeclarationSyntax, int methodDeclarationHash)> Methods { get; set; } = new List<(MethodDeclarationSyntax methodDeclarationSyntax, int methodDeclaration)>();
    public readonly int AttributesAndClassNameAndMethodsAndNamespaceHashCode;
    public readonly int AttributesHashCode;
    public readonly AttributeData? GenerateClientAttribute;

    public MethodClassAndAttributeSyntax(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken)
    {
        GenerateClientAttribute = context.Attributes.FirstOrDefault();
        SemanticModel = context.SemanticModel;
        // target node must be class because of filter we used previously
        ClassDeclarationSyntax = (ClassDeclarationSyntax)context.TargetNode;
        if (ClassDeclarationSyntax == null)
        {
            return;
        }
        
        cancellationToken.ThrowIfCancellationRequested();
        var attributesDeclaration = ClassDeclarationSyntax.AttributeLists.ToString();
        var containingNamespace = context.TargetSymbol.ContainingNamespace.ToDisplayString();
        var className = context.TargetSymbol.ToDisplayString();
        var methodDeclarations = string.Join(";",
            ClassDeclarationSyntax?.ChildNodes()
               .OfType<MethodDeclarationSyntax>()
               .Select(x =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var methodDeclaration = GetMethodWithoutBody(x);
                    Methods.Add((x, methodDeclaration.GetHashCode()));
                    return methodDeclaration;
                }) ?? Array.Empty<string>());
        cancellationToken.ThrowIfCancellationRequested();
        AttributesHashCode = attributesDeclaration.GetHashCode();
        AttributesAndClassNameAndMethodsAndNamespaceHashCode = GetFullHashCode(AttributesHashCode, className, methodDeclarations, containingNamespace);
    }
    
    private static int GetFullHashCode(
        int attributesHashCode,
        string className,
        string methodDeclarations,
        string containingNamespace)
    {
        unchecked
        {
            var hashCode = 397 ^ attributesHashCode;
            hashCode = (hashCode * 397) ^ className.GetHashCode();
            hashCode = (hashCode * 397) ^ methodDeclarations.GetHashCode();
            hashCode = (hashCode * 397) ^ containingNamespace.GetHashCode();
            return hashCode;
        }
    }

    private static string GetMethodWithoutBody(
        MethodDeclarationSyntax x)
    {
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

        return methodDeclaration;
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

        return AttributesAndClassNameAndMethodsAndNamespaceHashCode == other.AttributesAndClassNameAndMethodsAndNamespaceHashCode;
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
        return AttributesAndClassNameAndMethodsAndNamespaceHashCode;
    }
}
