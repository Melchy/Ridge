using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace RidgeSourceGenerator;

public class ControllerToGenerate
{
    public readonly List<IMethodSymbol> PublicMethods;
    public readonly string Name;
    public readonly string FullyQualifiedName;
    public readonly string Namespace;
    private readonly ImmutableArray<KeyValuePair<string, TypedConstant>> MainAttributeSettings;

    public bool UseHttpResponseMessageAsReturnType
    {
        get
        {
            var result = MainAttributeSettings.FirstOrDefault(x =>
                    x.Key == "UseHttpResponseMessageAsReturnType")
               .Value.Value as bool?;
            return result ?? false;
        }
    }

    public ControllerToGenerate(
        string name,
        string fullyQualifiedName,
        string @namespace,
        List<IMethodSymbol> publicMethods,
        ImmutableArray<KeyValuePair<string, TypedConstant>> mainAttributeSettings)
    {
        PublicMethods = publicMethods;
        MainAttributeSettings = mainAttributeSettings;
        Name = name;
        FullyQualifiedName = fullyQualifiedName;
        Namespace = @namespace;
    }
}
