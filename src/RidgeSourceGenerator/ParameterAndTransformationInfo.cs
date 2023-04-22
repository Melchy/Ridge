using Microsoft.CodeAnalysis;
using RidgeSourceGenerator.Dtos;

namespace RidgeSourceGenerator;

public struct ParameterAndTransformationInfo
{
    private ParameterAndTransformationInfo(
        string? originalName,
        IParameterSymbol? originalParameterSymbol,
        bool? originalIsOptional,
        ITypeSymbol typeInClient,
        string nameInClient,
        bool wasDeleted,
        bool isOptionalInClient,
        bool isTransformedOrAdded,
        ParameterMapping? addedOrTransformedParameterMapping)
    {
        OriginalName = originalName;
        OriginalParameterSymbol = originalParameterSymbol;
        OriginalIsOptional = originalIsOptional;
        TypeInClient = typeInClient;
        NameInClient = nameInClient;
        WasDeleted = wasDeleted;
        IsOptionalInClient = isOptionalInClient;
        IsTransformedOrAdded = isTransformedOrAdded;
        AddedOrTransformedParameterMapping = addedOrTransformedParameterMapping;
    }

    public string? OriginalName { get; }
    public IParameterSymbol? OriginalParameterSymbol { get; }
    public bool? OriginalIsOptional { get; }
    public ITypeSymbol TypeInClient { get; }
    public string NameInClient { get; }
    public bool WasDeleted { get; }
    public bool IsOptionalInClient { get; }
    public bool IsTransformedOrAdded { get; }
    public ParameterMapping? AddedOrTransformedParameterMapping { get; }

    public static ParameterAndTransformationInfo CreateUnchangedParameter(
        IParameterSymbol parameter)
    {
        return new ParameterAndTransformationInfo(parameter.Name,
            parameter,
            parameter.IsOptional,
            parameter.Type,
            parameter.Name,
            false,
            parameter.IsOptional,
            false,
            null);
    }

    public static ParameterAndTransformationInfo CreateDeletedParameter(
        IParameterSymbol parameter)
    {
        return new ParameterAndTransformationInfo(parameter.Name,
            parameter,
            parameter.IsOptional,
            parameter.Type,
            parameter.Name,
            true,
            parameter.IsOptional,
            false,
            null);
    }

    public static ParameterAndTransformationInfo CreateTransformedParameter(
        IParameterSymbol parameter,
        ParameterTransformation transformation,
        string? newName,
        ParameterMapping parameterMapping)
    {
        return new ParameterAndTransformationInfo(parameter.Name,
            parameter,
            parameter.IsOptional,
            transformation.ToType,
            newName ?? parameter.Name,
            false,
            transformation.Optional ?? parameter.IsOptional,
            true,
            parameterMapping);
    }

    public static ParameterAndTransformationInfo CreateAddedParameter(
        AddParameter parameterToAdd,
        string name,
        ParameterMapping parameterMapping)
    {
        return new ParameterAndTransformationInfo(null,
            null,
            null,
            parameterToAdd.Type,
            name,
            false,
            parameterToAdd.IsOptional,
            true,
            parameterMapping);
    }
}
