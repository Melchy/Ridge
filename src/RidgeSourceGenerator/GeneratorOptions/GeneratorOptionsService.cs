using Microsoft.CodeAnalysis;

namespace RidgeSourceGenerator.GeneratorOptions;

public static class GeneratorOptionsService
{
    public static IncrementalValueProvider<RidgeOptions> GetGeneratorOptions(
        IncrementalGeneratorInitializationContext context)
    {
        return context.AnalyzerConfigOptionsProvider
           .Select((
                options, 
                _) =>
            {
                var useSingleClientPresent = options.GlobalOptions
                   .TryGetValue("build_property.Ridge_GenerateEndpointCallsAsExtensionMethods",
                        out var useSingleClientValue);
                if (useSingleClientPresent)
                {
                    return new RidgeOptions(IsFeatureEnabled(useSingleClientValue));
                }

                return new RidgeOptions(false);
            });
    }

    private static bool IsFeatureEnabled(
        string? value)
    {
        return StringComparer.OrdinalIgnoreCase.Equals("enable", value)
               || StringComparer.OrdinalIgnoreCase.Equals("enabled", value)
               || StringComparer.OrdinalIgnoreCase.Equals("true", value);
    }
}
