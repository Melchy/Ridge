using Microsoft.CodeAnalysis;

namespace RidgeSourceGenerator.GeneratorOptions;

// TODO by convention if controller contains single method called - run, execute or handle it will not be used in the name
// TODO if controller contains name endpoint than it is removed from the client method name
// TODO split to partial classes if too long
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
                   .TryGetValue("build_property.Ridge_UseSingleClient",
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
