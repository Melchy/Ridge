using Microsoft.CodeAnalysis;
using RidgeSourceGenerator.Dtos;
using System.Text;

namespace RidgeSourceGenerator.GenerationHelpers;

public static class CallerMethodGenerationHelper
{
    public static string GenerateMethod(
        MethodToGenerate methodToGenerate,
        CancellationToken cancellationToken)
    {
        StringBuilder sb = new StringBuilder(1024);


        var publicMethod = methodToGenerate.PublicMethod;
        if (publicMethod.IsGenericMethod)
        {
            return "";
        }

        if (publicMethod.ReturnType is not INamedTypeSymbol fullReturnType)
        {
            return "";
        }

        foreach (var publicMethodParameter in publicMethod.Parameters)
        {
            if (publicMethodParameter.Name == "" || publicMethodParameter.Type is IErrorTypeSymbol)
            {
                return "";
            }
        }

        ParameterNamePostfixTransformer parameterNamePostfixTransformer = null!;
        // Small optimization. If there are no user parameterNames then we wont use ParameterNamePostfixTransformer 
        if (methodToGenerate.ParameterTransformations.Count() + methodToGenerate.ParametersToAdd.Length > 0)
        {
            parameterNamePostfixTransformer = new ParameterNamePostfixTransformer(methodToGenerate.PublicMethod.Parameters.Select(x => x.Name));
        }
        
        cancellationToken.ThrowIfCancellationRequested();
        string? returnType = publicMethod.ReturnType.Name;

        GenerateMethodComment(methodToGenerate, sb, publicMethod);
        sb.Append(@"        public async ");

        returnType = AddReturnType(methodToGenerate, sb, returnType, fullReturnType);

        cancellationToken.ThrowIfCancellationRequested();

        sb.Append($" Call{publicMethod.Name}(");

        var stringBuilderForOptionalParameters = new StringBuilder();
        var transformedParameters = ParameterTransformationService.GetTransformation(
            publicMethod.Parameters,
            methodToGenerate.ParameterTransformations,
            parameterNamePostfixTransformer,
            methodToGenerate.ParametersToAdd);

        ProcessParameters(sb,
            stringBuilderForOptionalParameters,
            transformedParameters,
            cancellationToken);
        
        sb.Append(stringBuilderForOptionalParameters);
        AddRidgeParameters(sb);

        cancellationToken.ThrowIfCancellationRequested();
        
        sb.AppendLine(@"        {");
        AddMethodBody(methodToGenerate, cancellationToken, sb, publicMethod, transformedParameters, returnType);
        sb.AppendLine(@"        }");
        sb.AppendLine();

        return sb.ToString();
    }

    private static void AddMethodBody(
        MethodToGenerate methodToGenerate,
        CancellationToken cancellationToken,
        StringBuilder sb,
        IMethodSymbol publicMethod,
        ParameterAndTransformationInfo[] parametersAndTransformationsInfo,
        string? returnType)
    {
        sb.Append($$"""
                                var methodName = nameof({{methodToGenerate.ContainingControllerFullyQualifiedName}}.{{publicMethod.Name}});
                    """);

        sb.Append(@"
            var actionParameters = new Type[] {");
        foreach (var publicMethodParameter in publicMethod.Parameters)
        {
            cancellationToken.ThrowIfCancellationRequested();
            sb.Append($@"
                typeof({publicMethodParameter.Type.ToDisplayString(NullableFlowState.None)}),");
        }

        sb.AppendLine(@"
            };");

        sb.AppendLine("""
                                var parametersAndTransformations = new List<RawParameterAndTransformationInfo>()
                                { 
                    """);
        foreach (var parameterAndTransformationInfo in parametersAndTransformationsInfo)
        {
            cancellationToken.ThrowIfCancellationRequested();

            sb.Append($@"                RawParameterAndTransformationInfo.Create<{methodToGenerate.ContainingControllerFullyQualifiedName}>(");
            sb.Append(parameterAndTransformationInfo.OriginalName == null ? "null," : $"\"{parameterAndTransformationInfo.OriginalName}\",");
            sb.Append(parameterAndTransformationInfo.OriginalParameterSymbol?.Type == null ? "null," : $"typeof({parameterAndTransformationInfo.OriginalParameterSymbol?.Type.ToDisplayString(NullableFlowState.NotNull)}),");
            sb.Append(parameterAndTransformationInfo.OriginalIsOptional == null ? "null," : $"{parameterAndTransformationInfo.OriginalIsOptional.Value.ToString().ToLower()},");
            sb.Append(parameterAndTransformationInfo.AddedOrTransformedParameterMapping == null ? "null," : $"{(int?)parameterAndTransformationInfo.AddedOrTransformedParameterMapping.Value},");

            if (!parameterAndTransformationInfo.WasDeleted)
            {
                sb.Append($"@{parameterAndTransformationInfo.NameInCaller},");
            }
            else
            {
                sb.Append("null,");
            }

            sb.AppendLine($"typeof({parameterAndTransformationInfo.TypeInCaller.ToDisplayString(NullableFlowState.NotNull)}), " +
                          $"\"{parameterAndTransformationInfo.NameInCaller}\", " +
                          $"{parameterAndTransformationInfo.WasDeleted.ToString().ToLower()}, " +
                          $"{parameterAndTransformationInfo.IsOptionalInCaller.ToString().ToLower()}, " +
                          $"{parameterAndTransformationInfo.IsTransformedOrAdded.ToString().ToLower()}," +
                          "methodName," +
                          "actionParameters),");
        }

        sb.AppendLine(@"            };");

        if (methodToGenerate.UseHttpResponseMessageAsReturnType)
        {
            sb.Append(@"            return await _applicationCaller.CallActionWithHttpResponseMessageResult<");
        }
        else
        {
            if (returnType == null)
            {
                sb.Append(@"            return await _applicationCaller.CallAction<");
            }
            else
            {
                sb.Append(@$"           return await _applicationCaller.CallAction<{returnType},");
            }
        }

        sb.AppendLine($"{methodToGenerate.ContainingControllerFullyQualifiedName}>(methodName, actionParameters, additionalParameters, parametersAndTransformations);");
    }

    private static void AddRidgeParameters(
        StringBuilder sb)
    {
        sb.AppendLine(@"params AdditionalParameter[] additionalParameters)");
    }

    private static string? AddReturnType(
        MethodToGenerate methodToGenerate,
        StringBuilder sb,
        string? returnType,
        INamedTypeSymbol fullReturnType)
    {
        if (methodToGenerate.UseHttpResponseMessageAsReturnType)
        {
            sb.Append("Task<HttpResponseMessage>");
        }
        else
        {
            returnType = GetActualReturnType(fullReturnType);

            if (returnType == null)
            {
                sb.Append("Task<HttpCallResponse>");
            }
            else
            {
                sb.Append($"Task<HttpCallResponse<{returnType}>>");
            }
        }

        return returnType;
    }

    private static void GenerateMethodComment(
        MethodToGenerate methodToGenerate,
        StringBuilder sb,
        IMethodSymbol publicMethod)
    {
        sb.AppendLine(
            $$"""
                    /// <summary>
                    ///     Calls <see cref="{{methodToGenerate.ContainingControllerFullyQualifiedName}}.{{publicMethod.Name}}" />. 
                    /// </summary> 
            """);
    }

    private static void ProcessParameters(
        StringBuilder sb,
        StringBuilder stringBuilderForOptionalParameters,
        ParameterAndTransformationInfo[] parameterBuilds,
        CancellationToken cancellationToken)
    {
        foreach (var parameter in parameterBuilds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (parameter.WasDeleted)
            {
                continue;
            }

            if (parameter.IsOptionalInCaller)
            {
                string defaultValue;
                string type;

                if (parameter.IsTransformedOrAdded)
                {
                    defaultValue = "default";
                    type = parameter.TypeInCaller.ToDisplayString(NullableFlowState.MaybeNull);
                }
                else
                {
                    type = parameter.TypeInCaller.ToDisplayString();
                    if (parameter.OriginalParameterSymbol!.HasExplicitDefaultValue)
                    {
                        if (parameter.OriginalParameterSymbol!.ExplicitDefaultValue == null)
                        {
                            defaultValue = "default";
                        }
                        else if (parameter.OriginalParameterSymbol!.ExplicitDefaultValue is string)
                        {
                            defaultValue = $"\"{parameter.OriginalParameterSymbol!.ExplicitDefaultValue}\"";
                        }
                        else if (parameter.OriginalParameterSymbol!.ExplicitDefaultValue is char)
                        {
                            defaultValue = $"\'{parameter.OriginalParameterSymbol!.ExplicitDefaultValue}'";
                        }
                        else
                        {
                            defaultValue = parameter.OriginalParameterSymbol!.ExplicitDefaultValue.ToString();
                        }
                    }
                    else
                    {
                        defaultValue = "default";
                    }
                }

                stringBuilderForOptionalParameters.Append($@"{type} @{parameter.NameInCaller} = {defaultValue}, ");
            }
            else
            {
                sb.Append($@"{parameter.TypeInCaller} @{parameter.NameInCaller}, ");
            }
        }
    }

    private static string? GetActualReturnType(
        INamedTypeSymbol returnType)
    {
        if (returnType.Arity == 0)
        {
            if (returnType.Name == "Task")
            {
                return null;
            }

            if (returnType.Name == "Void")
            {
                return null;
            }

            if (returnType.Name == "IActionResult")
            {
                return null;
            }

            return returnType.ToString();
        }

        if (returnType.Arity == 1)
        {
            var firstGenericArgument = (INamedTypeSymbol)returnType.TypeArguments.First();

            //Task<XXX>
            if (returnType.Name == "Task")
            {
                //Task<IActionResult>
                if (firstGenericArgument.Name == "IActionResult")
                {
                    return null;
                }
                //Task<ActionResult<XXX>> or Task<ActionResult> 

                if (firstGenericArgument.Name == "ActionResult")
                {
                    if (firstGenericArgument.Arity == 0)
                    {
                        //Task<ActionResult>
                        return null;
                    }

                    //Task<ActionResult<XXX>>
                    return firstGenericArgument.TypeArguments.First().ToString();
                }
                //Task<XXX>

                return firstGenericArgument.ToString();
            }
            //ActionResult<XXX> or ActionResult 

            if (returnType.Name == "ActionResult")
            {
                if (returnType.Arity == 0)
                {
                    //ActionResult
                    return null;
                }

                //ActionResult<XXX>
                return returnType.TypeArguments.First().ToString();
            }
            // Generic<XXX>

            return returnType.ToString();
        }
        // Generic<XXX>

        return returnType.ToString();
    }
}
