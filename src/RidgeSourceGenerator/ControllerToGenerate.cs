using Microsoft.CodeAnalysis;

namespace RidgeSourceGenerator;

public class ControllerToGenerate
{
    public readonly List<IMethodSymbol> PublicMethods;
    public readonly string Name;
    public readonly string FullyQualifiedName;

    public readonly string Namespace;
    // public readonly IEnumerator<string> NamespacesToAddToUsing;

    public ControllerToGenerate(
            string name,
            string fullyQualifiedName,
            string @namespace,
            List<IMethodSymbol> publicMethods)
        // IEnumerator<string> namespacesToAddToUsing)
    {
        PublicMethods = publicMethods;
        // NamespacesToAddToUsing = namespacesToAddToUsing;
        Name = name;
        FullyQualifiedName = fullyQualifiedName;
        Namespace = @namespace;
    }
}
