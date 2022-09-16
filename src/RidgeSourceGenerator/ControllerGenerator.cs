using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace RidgeSourceGenerator;

//TODO interceptory se nastavuji pro celou tridu ale bude potreba to nastavovat pro jednotlive metody. Nejak.
[Generator]
public class ControllerGenerator : IIncrementalGenerator
{
    private const string RidgeAttribute = "Ridge.GenerateStronglyTypedCallerForTesting";

    public void Initialize(
        IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ClassDeclarationSyntax> controllerDeclarations = context.SyntaxProvider
           .CreateSyntaxProvider(
                predicate: static (
                    s,
                    _) => IsSyntaxTargetForGeneration(s),
                transform: static (
                    ctx,
                    _) => GetSemanticTargetForGeneration(ctx))
           .Where(static m => m is not null)!;

        IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndClasses
            = context.CompilationProvider.Combine(controllerDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses,
            static (
                spc,
                source) => Execute(source.Item1, source.Item2, spc));
    }

    private static bool IsSyntaxTargetForGeneration(
        SyntaxNode node)
    {
        if (node is not AttributeSyntax attribute)
        {
            return false;
        }

        var name = ExtractName(attribute.Name);

        if (name is "GenerateStronglyTypedCallerForTesting" or "GenerateStronglyTypedCallerForTestingAttribute")
        {
            return true;
        }

        return false;
    }

    private static string? ExtractName(
        NameSyntax? name)
    {
        return name switch
        {
            SimpleNameSyntax ins => ins.Identifier.Text,
            QualifiedNameSyntax qns => qns.Right.Identifier.Text,
            _ => null,
        };
    }

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(
        GeneratorSyntaxContext context)
    {
        return (ClassDeclarationSyntax?)context.Node.Parent?.Parent;
    }

    private static void Execute(
        Compilation compilation,
        ImmutableArray<ClassDeclarationSyntax> controllers,
        SourceProductionContext context)
    {
        IEnumerable<ClassDeclarationSyntax> distinctEnums = controllers.Distinct();

        IEnumerable<ControllerToGenerate> controllersToGenerates = GetTypesToGenerate(compilation, distinctEnums, context.CancellationToken);
        if (!controllersToGenerates.Any())
        {
            return;
        }

        StringBuilder sb = new StringBuilder();
        foreach (var controllerToGenerate in controllersToGenerates)
        {
            sb.Clear();
            var result = SourceGenerationHelper.GenerateExtensionClass(sb, controllerToGenerate);
            context.AddSource(controllerToGenerate.Name + "_TestCaller.g.cs", SourceText.From(result, Encoding.UTF8));
        }

        if (controllers.IsDefaultOrEmpty)
        {
            // nothing to do yet
        }
    }

    private static IEnumerable<ControllerToGenerate> GetTypesToGenerate(
        Compilation compilation,
        IEnumerable<ClassDeclarationSyntax> controllers,
        CancellationToken ct)
    {
        var controllersToGenerates = new List<ControllerToGenerate>();
        INamedTypeSymbol? attribute = compilation.GetTypeByMetadataName(RidgeAttribute);
        if (attribute == null)
        {
            // nothing to do if this type isn't available
            return controllersToGenerates;
        }

        foreach (var controllerDeclarationSyntax in controllers)
        {
            if (controllerDeclarationSyntax.Arity != 0)
            {
                continue;
            }

            // stop if we're asked to
            ct.ThrowIfCancellationRequested();

            SemanticModel semanticModel = compilation.GetSemanticModel(controllerDeclarationSyntax.SyntaxTree);
            if (semanticModel.GetDeclaredSymbol(controllerDeclarationSyntax) is not INamedTypeSymbol controllerSymbol)
            {
                // report diagnostic, something went wrong
                continue;
            }

            string name = controllerSymbol.Name;
            string nameSpace = controllerSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : controllerSymbol.ContainingNamespace.ToString();

            var publicMethods = new List<IMethodSymbol>();

            foreach (var member in controllerSymbol.GetMembers())
            {
                if (member is not IMethodSymbol methodSymbol)
                {
                    continue;
                }

                if (methodSymbol.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                // skip ctor and other weird things
                if (methodSymbol.IsImplicitlyDeclared)
                {
                    continue;
                }

                if (!methodSymbol.CanBeReferencedByName)
                {
                    continue;
                }

                if (methodSymbol.IsStatic)
                {
                    continue;
                }

                publicMethods.Add(methodSymbol);
            }

            string fullyQualifiedName = controllerSymbol.ToString();

            controllersToGenerates.Add(new ControllerToGenerate(
                name: name,
                fullyQualifiedName: fullyQualifiedName,
                @namespace: nameSpace,
                publicMethods: publicMethods));
        }

        return controllersToGenerates;
    }
}
