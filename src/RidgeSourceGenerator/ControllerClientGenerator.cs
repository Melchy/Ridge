﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using RidgeSourceGenerator.Dtos;
using RidgeSourceGenerator.GenerationHelpers;
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
           .CreateSyntaxProvider(
                predicate: static (
                    s,
                    _) => IsSyntaxTargetForGeneration(s),
                transform: static (
                    ctx,
                    ct) => new MethodClassAndAttributeSyntax(GetSemanticTargetForGeneration(ctx), ctx.SemanticModel, ct))
           .Select(GetTypesToGenerate)
           .Collect()
           .SelectMany((
                x,
                _) => x.Distinct());

        var methodsToGenerate = controllerDeclarations.Where(x => x is not null)
           .SelectMany((
                    controllerToGenerate,
                    ct) =>
                controllerToGenerate!.PublicMethods.Select(methodSymbolAndHash =>
                    new MethodToGenerate(methodSymbolAndHash.MethodSymbol,
                        controllerToGenerate.UseHttpResponseMessageAsReturnType,
                        controllerToGenerate.ParameterTransformations,
                        controllerToGenerate.FullyQualifiedName,
                        methodSymbolAndHash.MethodHash,
                        controllerToGenerate.ParametersToAdd,
                        controllerToGenerate.AttributesHash,
                        ct)))
           .Collect();

        var controllersWithMethods = controllerDeclarations.Combine(methodsToGenerate);

        context.RegisterSourceOutput(controllersWithMethods,
            static (
                spc,
                source) => Execute(source, spc));
    }

    private static bool IsSyntaxTargetForGeneration(
        SyntaxNode node)
    {
        if (node is not AttributeSyntax attribute)
        {
            return false;
        }

        var name = ExtractName(attribute.Name);

        return IsMainCorrectAttribute(name);
    }

    private static bool IsMainCorrectAttribute(
        string? name)
    {
        if (name is "GenerateClient" or "GenerateClientAttribute")
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

    private static AttributeSyntax GetSemanticTargetForGeneration(
        GeneratorSyntaxContext context)
    {
        return (AttributeSyntax)context.Node;
    }

    private static void Execute(
        (Dtos.ControllerToGenerate? ControllerToGenerate, ImmutableArray<MethodToGenerate> MethodsToGenerate) controllerAndMethods,
        SourceProductionContext context)
    {
        if (controllerAndMethods.ControllerToGenerate == null)
        {
            return;
        }

        var generatedMethods = controllerAndMethods.MethodsToGenerate
           .Where(x => x?.ContainingControllerFullyQualifiedName == controllerAndMethods.ControllerToGenerate.FullyQualifiedName);
        
        
        StringBuilder sb = new StringBuilder(2048);
        var result = ControllerClientGenerationHelper.GenerateExtensionClass(sb,
            controllerAndMethods.ControllerToGenerate,
            generatedMethods!,
            context.CancellationToken);
        context.AddSource(controllerAndMethods.ControllerToGenerate.Name + "_Client.g.cs", SourceText.From(result, Encoding.UTF8));
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
        var semanticModel = methodClassAndAttributeSyntax.SemanticModel.GetDeclaredSymbol(methodClassAndAttributeSyntax.ClassDeclarationSyntax);


        if (methodClassAndAttributeSyntax.ClassDeclarationSyntax.Arity != 0)
        {
            return null;
        }

        if (semanticModel is not INamedTypeSymbol controllerSymbol)
        {
            return null;
        }

        var attributes = controllerSymbol.GetAttributes();
        var generatorAttribute = attributes.FirstOrDefault(x => IsMainCorrectAttribute(x.AttributeClass?.Name));
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

        var publicMethods = new List<(IMethodSymbol MethodSymbol, int MethodHash)>();

        foreach (var methodDeclarationAndHash in methodClassAndAttributeSyntax.Methods)
        {
            var potentialMethodSymbol = methodClassAndAttributeSyntax.SemanticModel.GetDeclaredSymbol(methodDeclarationAndHash.methodDeclarationSyntax);

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

            publicMethods.Add((methodSymbol, methodDeclarationAndHash.methodDeclarationHash));
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
            attributesClassNameAndMethodsHashCode: methodClassAndAttributeSyntax.AttributesClassNameAndMethodsHashCode,
            attributesHash: methodClassAndAttributeSyntax.AttributesHashCode);
    }
}