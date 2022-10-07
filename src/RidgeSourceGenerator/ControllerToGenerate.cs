using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace RidgeSourceGenerator;

public class ControllerToGenerate
{
    public readonly List<IMethodSymbol> PublicMethods;
    public readonly string Name;
    public readonly string FullyQualifiedName;
    public readonly string Namespace;

    public bool UseHttpResponseMessageAsReturnType;
    public IDictionary<string, string> TypeTransformations;

    public ControllerToGenerate(
        string name,
        string fullyQualifiedName,
        string @namespace,
        List<IMethodSymbol> publicMethods,
        ImmutableArray<KeyValuePair<string, TypedConstant>> mainAttributeSettings,
        IEnumerable<AttributeData> typeTransformerAttributes)
    {
        PublicMethods = publicMethods;
        Name = name;
        FullyQualifiedName = fullyQualifiedName;
        Namespace = @namespace;
        UseHttpResponseMessageAsReturnType = GetUseHttpResponseMessageAsReturnType(mainAttributeSettings);
        TypeTransformations = GetTypeTransformations(typeTransformerAttributes);
    }

    private IDictionary<string, string> GetTypeTransformations(
        IEnumerable<AttributeData> typeTransformerAttributes)
    {
        var result = new Dictionary<string, string>();
        foreach (var typeTransformerAttribute in typeTransformerAttributes)
        {
            var fromType = (ISymbol?)typeTransformerAttribute.ConstructorArguments[0].Value;
            var toType = (ISymbol?)typeTransformerAttribute.ConstructorArguments[1].Value;
            if (fromType == null || toType == null)
            {
                continue;
            }

            result[fromType.Name] = toType.Name;
        }

        return result;
    }

    private bool GetUseHttpResponseMessageAsReturnType(
        ImmutableArray<KeyValuePair<string, TypedConstant>> mainAttributeSettings)
    {
        var result = mainAttributeSettings.FirstOrDefault(x =>
                x.Key == "UseHttpResponseMessageAsReturnType")
           .Value.Value as bool?;
        return result ?? false;
    }
}
