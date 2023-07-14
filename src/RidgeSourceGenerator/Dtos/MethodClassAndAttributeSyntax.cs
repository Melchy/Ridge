using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RidgeSourceGenerator.Dtos;

public class MethodClassAndAttributeSyntax : IEquatable<MethodClassAndAttributeSyntax>
{
    public SemanticModel SemanticModel;
    public ClassDeclarationSyntax? ClassDeclarationSyntax { get; set; }
    public List<(MethodDeclarationSyntax methodDeclarationSyntax, int methodDeclarationHash)> Methods { get; set; } = new List<(MethodDeclarationSyntax methodDeclarationSyntax, int methodDeclaration)>();
    public readonly int AttributesClassNameAndMethodsHashCode;
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
        AttributesClassNameAndMethodsHashCode = GetFullHashCode(AttributesHashCode, className, methodDeclarations);
    }
    
    private static int GetFullHashCode(
        int attributesHashCode,
        string className,
        string methodDeclarations)
    {
        unchecked
        {
            var hashCode = 397 ^ attributesHashCode;
            hashCode = (hashCode * 397) ^ className.GetHashCode();
            hashCode = (hashCode * 397) ^ methodDeclarations.GetHashCode();
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

        return AttributesClassNameAndMethodsHashCode == other.AttributesClassNameAndMethodsHashCode;
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
        return AttributesClassNameAndMethodsHashCode;
    }
}
