using RidgeSourceGenerator.Dtos;
using RidgeSourceGenerator.GeneratorOptions;
using System.Text;

namespace RidgeSourceGenerator.GenerationHelpers;

public static class ControllerClientGenerationHelper
{
    public static void Generate(
        StringBuilder sb,
        ControllerToGenerate controllerToGenerate,
        MethodToGenerate?[] methodsToGenerate,
        RidgeOptions options,
        CancellationToken cancellationToken)
    {
        var className = $"{controllerToGenerate.Name}Client";
        var generateStaticClass = options.GenerateEndpointCallsAsExtensionMethods;
        ClassGenerator.Generate(sb, className, controllerToGenerate.Namespace, generateStaticClass);
        cancellationToken.ThrowIfCancellationRequested();

        for (var index = 0; index < methodsToGenerate.Length; index++)
        {
            var methodToGenerate = methodsToGenerate[index];
            if (methodToGenerate == null)
            {
                continue;
            }
            
            sb.Append(methodToGenerate.GenerateMethod(options.GenerateEndpointCallsAsExtensionMethods, cancellationToken));
        }

        sb.AppendLine(" }");

        if (!string.IsNullOrEmpty(controllerToGenerate.Namespace))
        {
            sb.AppendLine("}");
        }

        cancellationToken.ThrowIfCancellationRequested();
        sb.AppendLine("#pragma warning restore CS0419");
        sb.AppendLine("#pragma warning restore CS8669");
        
        sb.Append("#nullable restore");
    }
}
