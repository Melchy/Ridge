using Microsoft.CodeAnalysis;
using RidgeSourceGenerator.Dtos;
using System.Collections.Immutable;

namespace RidgeSourceGenerator;

internal class ParameterTransformationService
{
    public static ParameterAndTransformationInfo[] GetTransformation(
        ImmutableArray<IParameterSymbol> parameters,
        Dictionary<ITypeSymbol, ParameterTransformation> parameterTransformations,
        ParameterNamePostfixTransformer parameterNamePostfixTransformer,
        AddParameter[] parametersToAdd)
    {
        ParameterAndTransformationInfo[] parametersList = new ParameterAndTransformationInfo[parameters.Length + parametersToAdd.Length];
        var index = 0;
        foreach (var parameter in parameters)
        {
            parametersList[index] = ProcessParameter(parameterTransformations, parameterNamePostfixTransformer, parameter);
            index++;
        }

        foreach (var addParameter in parametersToAdd)
        {
            var addedParameters =
                ParameterAndTransformationInfo.CreateAddedParameter(
                    addParameter,
                    parameterNamePostfixTransformer.TransformName(addParameter.Name),
                    addParameter.ParameterMapping);
            parametersList[index] = addedParameters;
            index++;
        }

        return parametersList;
    }

    private static ParameterAndTransformationInfo ProcessParameter(
        Dictionary<ITypeSymbol, ParameterTransformation> parameterTransformations,
        ParameterNamePostfixTransformer parameterNamePostfixTransformer,
        IParameterSymbol parameter)
    {
        var originalParameterAttributes = parameter.GetAttributes();
        var fromServiceAttribute = originalParameterAttributes.FirstOrDefault(x => x.AttributeClass?.Name == "FromServicesAttribute");
        if (fromServiceAttribute != null)
        {
            return ParameterAndTransformationInfo.CreateDeletedParameter(parameter);
        }

        if (parameter.Type.Name == "CancellationToken")
        {
            return ParameterAndTransformationInfo.CreateDeletedParameter(parameter);
        }

        var typeMustBeTransformed =
            parameterTransformations.TryGetValue(
                parameter.Type,
                out var transformation);

        if (!typeMustBeTransformed)
        {
            return ParameterAndTransformationInfo.CreateUnchangedParameter(parameter);
        }

        if (transformation.ToType.Name == "Void")
        {
            return ParameterAndTransformationInfo.CreateDeletedParameter(parameter);
        }

        return ParameterAndTransformationInfo.CreateTransformedParameter(
            parameter,
            transformation,
            transformation.NewName == null ? null : parameterNamePostfixTransformer.TransformName(transformation.NewName),
            transformation.ParameterMapping
        );
    }
}
