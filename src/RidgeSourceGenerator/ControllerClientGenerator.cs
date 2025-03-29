using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using RidgeSourceGenerator.Dtos;
using RidgeSourceGenerator.GenerationHelpers;
using RidgeSourceGenerator.GeneratorOptions;
using System.Collections.Immutable;
using System.Text;

namespace RidgeSourceGenerator;

[Generator]
public class ControllerClientGenerator : IIncrementalGenerator
{
    public void Initialize(
        IncrementalGeneratorInitializationContext context)
    {
        var controllerDeclarations = context.SyntaxProvider
           .ForAttributeWithMetadataName("Ridge.AspNetCore.GeneratorAttributes.GenerateClient",
                (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax,
                (context, cancellationToken) => new MethodClassAndAttributeSyntax(context, cancellationToken))
           .Select(GetTypesToGenerate)
           .Collect()
           .SelectMany((
                x,
                _) => x.Distinct());

        var controllersWithMethods = controllerDeclarations.Where(x => x is not null)
           .Select((
                controllerToGenerate,
                ct) =>
            {
                MethodToGenerate?[] methodsToGenerate = new MethodToGenerate[controllerToGenerate!.PublicMethods.Length];
                var numberOfMethods = 0;
                int lastMethodIndex = 0;
                for (var i = 0; i < controllerToGenerate.PublicMethods.Length; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    if (controllerToGenerate.PublicMethods[i] == null)
                    {
                        methodsToGenerate[i] = null;
                    }
                    else
                    {
                        numberOfMethods++;
                        lastMethodIndex = i;
                        methodsToGenerate[i] = new MethodToGenerate(controllerToGenerate.PublicMethods[i]!.Value.MethodSymbol,
                            controllerToGenerate.UseHttpResponseMessageAsReturnType,
                            controllerToGenerate.ParameterTransformations,
                            controllerToGenerate.FullyQualifiedName,
                            controllerToGenerate.PublicMethods[i]!.Value.MethodHash,
                            controllerToGenerate.ParametersToAdd,
                            controllerToGenerate.AttributesHash,
                            controllerToGenerate.Name);
                    }
                }

                if (numberOfMethods == 1)
                {
                    methodsToGenerate[lastMethodIndex]!.SingleMethodInController = true;
                }

                ct.ThrowIfCancellationRequested();
                return new ControllerWithMethodsToGenerate(controllerToGenerate, methodsToGenerate);
            });

        var options = GeneratorOptionsService.GetGeneratorOptions(context);
        context.RegisterSourceOutput(controllersWithMethods.Combine(options),
            static (
                spc,
                source) => Execute(source, spc));
    }

    private static void Execute(
        (ControllerWithMethodsToGenerate ControllerAndMethods, RidgeOptions Options) inputData,
        SourceProductionContext context)
    {
        var controllerAndMethods = inputData.ControllerAndMethods;
        var options = inputData.Options;
        
        var sb = new StringBuilder(2048);
        ControllerClientGenerationHelper.Generate(sb,
            controllerAndMethods.ControllerToGenerate,
            controllerAndMethods.MethodsToGenerate,
            options,
            context.CancellationToken);
        context.AddSource($"{controllerAndMethods.ControllerToGenerate.FullyQualifiedName}_Client.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }


    private static ControllerToGenerate? GetTypesToGenerate(
        MethodClassAndAttributeSyntax methodClassAndAttributeSyntax,
        CancellationToken cancellationToken)
    {
        if (methodClassAndAttributeSyntax.ClassDeclarationSyntax == null)
        {
            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var semanticModel = ModelExtensions.GetDeclaredSymbol(methodClassAndAttributeSyntax.SemanticModel, methodClassAndAttributeSyntax.ClassDeclarationSyntax);


        if (methodClassAndAttributeSyntax.ClassDeclarationSyntax.Arity != 0)
        {
            return null;
        }
        
        if (!methodClassAndAttributeSyntax.ClassDeclarationSyntax.Modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword)))
        {
            return null;
        }

        if (semanticModel is not INamedTypeSymbol controllerSymbol)
        {
            return null;
        }

        var attributes = controllerSymbol.GetAttributes();
        var generatorAttribute = methodClassAndAttributeSyntax.GenerateClientAttribute;
        var mainAttributeSettings = generatorAttribute?.NamedArguments ?? ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty;

        var typeTransformerAttributes = attributes
           .Where(x => x.AttributeClass is not IErrorTypeSymbol)
           .Where(x => x.AttributeClass?.Name is "TransformActionParameter" or "TransformActionParameterAttribute");

        var addParameterAttributes = attributes
           .Where(x => x.AttributeClass is not IErrorTypeSymbol)
           .Where(x => x.AttributeClass?.Name is "AddParameterToClient" or "AddParameterToClientAttribute");

        cancellationToken.ThrowIfCancellationRequested();

        string name = controllerSymbol.Name;
        string classNamespace = controllerSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : controllerSymbol.ContainingNamespace.ToString();

        var publicMethods = new (IMethodSymbol MethodSymbol, int MethodHash)?[methodClassAndAttributeSyntax.Methods.Count];

        for (var index = 0; index < methodClassAndAttributeSyntax.Methods.Count; index++)
        {
            var methodDeclarationAndHash = methodClassAndAttributeSyntax.Methods[index];
            var potentialMethodSymbol = ModelExtensions.GetDeclaredSymbol(methodClassAndAttributeSyntax.SemanticModel, methodDeclarationAndHash.methodDeclarationSyntax);

            cancellationToken.ThrowIfCancellationRequested();

            if (potentialMethodSymbol == null)
            {
                continue;
            }

            if (potentialMethodSymbol is not IMethodSymbol methodSymbol)
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

            publicMethods[index] = (methodSymbol, methodDeclarationAndHash.methodDeclarationHash);
        }

        string fullyQualifiedName = controllerSymbol.ToString();

        return new ControllerToGenerate(
            name: name,
            fullyQualifiedName: fullyQualifiedName,
            @namespace: classNamespace,
            publicMethods: publicMethods,
            mainAttributeSettings: mainAttributeSettings,
            typeTransformerAttributes: typeTransformerAttributes,
            addParameterAttributes: addParameterAttributes,
            attributesClassNameAndMethodsHashCode: methodClassAndAttributeSyntax.AttributesAndClassNameAndMethodsAndNamespaceHashCode,
            attributesHash: methodClassAndAttributeSyntax.AttributesHashCode);
    }
}
