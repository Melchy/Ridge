using Microsoft.CodeAnalysis;
using System.Text;

namespace RidgeSourceGenerator;

public static class MethodGenerationHelper
{
    public static string GenerateMethod(
        MethodToGenerate methodToGenerate,
        CancellationToken cancellationToken)
    {
        StringBuilder sb = new StringBuilder();


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
        sb.Append(@"    public async ");

        returnType = AddReturnType(methodToGenerate, sb, returnType, fullReturnType);

        cancellationToken.ThrowIfCancellationRequested();

        sb.Append(" ");
        sb.Append("Call_");
        sb.Append(publicMethod.Name);
        sb.Append("(");

        var stringBuilderForOptionalParameters = new StringBuilder();
        var nonRemovedArgumentNames = AddUserParameters(methodToGenerate, cancellationToken, publicMethod, sb, parameterNamePostfixTransformer, stringBuilderForOptionalParameters);
        sb.Append(stringBuilderForOptionalParameters);
        AddRidgeParameters(sb);

        cancellationToken.ThrowIfCancellationRequested();
        
        sb.AppendLine(@"    {");
        AddMethodBody(methodToGenerate, cancellationToken, sb, publicMethod, nonRemovedArgumentNames, returnType);
        sb.AppendLine(@"    }");
        sb.AppendLine();

        return sb.ToString();
    }

    private static void AddMethodBody(
        MethodToGenerate methodToGenerate,
        CancellationToken cancellationToken,
        StringBuilder sb,
        IMethodSymbol publicMethod,
        List<string> nonRemovedArgumentNames,
        string? returnType)
    {
        sb.Append(@"        var methodName = ");
        sb.Append("nameof(");
        sb.Append(methodToGenerate.ContainingControllerFullyQualifiedName);
        sb.Append(".");
        sb.Append(publicMethod.Name);
        sb.AppendLine(");");

        sb.AppendLine(@"        var arguments = new List<object?>()");
        sb.AppendLine(@"        {");
        foreach (var parameterName in nonRemovedArgumentNames)
        {
            cancellationToken.ThrowIfCancellationRequested();
            sb.Append(@"            ");
            sb.Append(parameterName);
            sb.AppendLine(",");
        }

        sb.AppendLine(@"        };");


        sb.Append(@"
        var actionParameters = new Type[] {");
        foreach (var publicMethodParameter in publicMethod.Parameters)
        {
            cancellationToken.ThrowIfCancellationRequested();
            sb.Append(@"
        ");
            sb.Append("typeof(");
            sb.Append(publicMethodParameter.Type.ToDisplayString(NullableFlowState.None));
            sb.Append("),");
        }

        sb.AppendLine(@"
        };");

        sb.AppendLine(@"
        var caller = new ActionCaller();");
        if (methodToGenerate.UseHttpResponseMessageAsReturnType)
        {
            sb.Append(@"        return await caller.CallActionWithHttpResponseMessageResult<");
        }
        else
        {
            if (returnType == null)
            {
                sb.Append(@"        return await caller.CallAction<");
            }
            else
            {
                sb.Append(@"        return await caller.CallAction<");
                sb.Append(returnType);
                sb.Append(",");
            }
        }

        sb.Append(methodToGenerate.ContainingControllerFullyQualifiedName);
        sb.AppendLine(">(arguments, methodName, this, actionParameters, headers, authenticationHeaderValue, actionInfoTransformers, httpRequestPipelineParts);");
    }

    private static void AddRidgeParameters(
        StringBuilder sb)
    {
        sb.AppendLine(@"IEnumerable<(string Key, string? Value)>? headers = null,
            AuthenticationHeaderValue? authenticationHeaderValue = null,
            IEnumerable<IActionInfoTransformer>? actionInfoTransformers = null,
            IEnumerable<IHttpRequestPipelinePart>? httpRequestPipelineParts = null
        )");
    }

    private static List<string> AddUserParameters(
        MethodToGenerate methodToGenerate,
        CancellationToken cancellationToken,
        IMethodSymbol publicMethod,
        StringBuilder sb,
        ParameterNamePostfixTransformer parameterNamePostfixTransformer,
        StringBuilder stringBuilderForOptionalParameters)
    {
        var nonRemovedArgumentNames = new List<string>();

        foreach (var publicMethodParameter in publicMethod.Parameters)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ProcessParameter(sb, stringBuilderForOptionalParameters, methodToGenerate, publicMethodParameter, nonRemovedArgumentNames, parameterNamePostfixTransformer);
        }

        foreach (var addParameter in methodToGenerate.ParametersToAdd)
        {
            ProcessParameterAddedByUser(sb, stringBuilderForOptionalParameters, addParameter, parameterNamePostfixTransformer);
        }

        return nonRemovedArgumentNames;
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
        sb.Append(@"
    /// <summary>
    ///     Calls <see cref=""");
        sb.Append(methodToGenerate.ContainingControllerFullyQualifiedName);
        sb.Append(".");
        sb.Append(publicMethod.Name);
        sb.AppendLine(@""" />.
    /// </summary>");
    }

    private static void ProcessParameterAddedByUser(
        StringBuilder stringBuilder,
        StringBuilder stringBuilderForOptionalParameters,
        AddParameter parameterToAdd,
        ParameterNamePostfixTransformer parameterNamePostfixTransformer)
    {
        StringBuilder stringBuidlerToUse;
        if (parameterToAdd.IsOptional)
        {
            stringBuidlerToUse = stringBuilderForOptionalParameters;
        }
        else
        {
            stringBuidlerToUse = stringBuilder;
        }

        stringBuidlerToUse.Append(@"
            ");
        if (parameterToAdd.IsOptional)
        {
            stringBuidlerToUse.Append(MakeTypeNullableIfItIsNotAlready(parameterToAdd.Type));
        }
        else
        {
            stringBuidlerToUse.Append(parameterToAdd.Type);
        }

        stringBuidlerToUse.Append(" ");
        stringBuidlerToUse.Append(parameterNamePostfixTransformer.TransformName(parameterToAdd.Name));
        if (parameterToAdd.IsOptional)
        {
            stringBuidlerToUse.Append(" = default");
        }

        stringBuidlerToUse.AppendLine(",");
    }

    private static void ProcessParameter(
        StringBuilder sb,
        StringBuilder stringBuilderForOptionalParameters,
        MethodToGenerate methodToGenerate,
        IParameterSymbol publicMethodParameter,
        List<string> nonRemovedArgumentNames,
        ParameterNamePostfixTransformer parameterNamePostfixTransformer)
    {
        var fromServicesAttribute = publicMethodParameter.GetAttributes().FirstOrDefault(x => x.AttributeClass?.Name == "FromServicesAttribute");
        StringBuilder builderToUseForCurrentParameter;
        if (fromServicesAttribute != null)
        {
            return;
        }

        string resultName;
        var typeMustBeTransformed =
            methodToGenerate.ParameterTransformations.TryGetValue(
                publicMethodParameter.Type.Name,
                out var result);

        if (typeMustBeTransformed)
        {
            if (result.ToType == "Void")
            {
                return;
            }

            if (result.Optional)
            {
                builderToUseForCurrentParameter = stringBuilderForOptionalParameters;
            }
            else
            {
                builderToUseForCurrentParameter = sb;
            }

            builderToUseForCurrentParameter.Append(@"
            ");


            if (result.Optional)
            {
                builderToUseForCurrentParameter.Append(MakeTypeNullableIfItIsNotAlready(result.ToType));
            }
            else
            {
                builderToUseForCurrentParameter.Append(result.ToType);
            }

            builderToUseForCurrentParameter.Append(" ");
            if (result.NewName != null)
            {
                resultName = result.NewName;
                builderToUseForCurrentParameter.Append(parameterNamePostfixTransformer.TransformName(resultName));
            }
            else
            {
                resultName = $"@{publicMethodParameter.Name}";
                builderToUseForCurrentParameter.Append(resultName);
            }

            if (result.Optional)
            {
                builderToUseForCurrentParameter.Append(" = default");
            }
        }
        else
        {
            if (publicMethodParameter.IsOptional)
            {
                builderToUseForCurrentParameter = stringBuilderForOptionalParameters;
            }
            else
            {
                builderToUseForCurrentParameter = sb;
            }

            builderToUseForCurrentParameter.Append(@"
            ");

            resultName = $"@{publicMethodParameter.Name}";
            builderToUseForCurrentParameter.Append(publicMethodParameter.Type);
            builderToUseForCurrentParameter.Append(" ");
            builderToUseForCurrentParameter.Append(resultName);

            if (publicMethodParameter.HasExplicitDefaultValue)
            {
                builderToUseForCurrentParameter.Append("= ");
                if (publicMethodParameter.ExplicitDefaultValue == null)
                {
                    builderToUseForCurrentParameter.Append("default");
                }
                else if (publicMethodParameter.ExplicitDefaultValue is string)
                {
                    builderToUseForCurrentParameter.Append("\"");
                    builderToUseForCurrentParameter.Append(publicMethodParameter.ExplicitDefaultValue);
                    builderToUseForCurrentParameter.Append("\"");
                }
                else if (publicMethodParameter.ExplicitDefaultValue is char)
                {
                    builderToUseForCurrentParameter.Append("\'");
                    builderToUseForCurrentParameter.Append(publicMethodParameter.ExplicitDefaultValue);
                    builderToUseForCurrentParameter.Append("\'");
                }
                else
                {
                    builderToUseForCurrentParameter.Append(publicMethodParameter.ExplicitDefaultValue);
                }
            }
        }

        nonRemovedArgumentNames.Add(resultName);
        builderToUseForCurrentParameter.Append(",");
    }

    private static string MakeTypeNullableIfItIsNotAlready(
        string type)
    {
        var typeWithNullable = type.TrimEnd('?');
        return $"{typeWithNullable}?";
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
