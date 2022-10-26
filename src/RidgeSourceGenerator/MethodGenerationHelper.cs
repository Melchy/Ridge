using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace RidgeSourceGenerator;

public class ParameterNamePostfixTransformer
{
    private readonly Dictionary<string, int> _namesBuffer = new();

    public ParameterNamePostfixTransformer(
        IEnumerable<string> methodParameters)
    {
        foreach (var methodParameter in methodParameters)
        {
            _namesBuffer.Add(methodParameter, 0);
        }
    }

    public string TransformName(
        string name)
    {
        _namesBuffer.TryGetValue(name, out var numberOfUses);
        _namesBuffer[name] = (numberOfUses + 1);
        if (numberOfUses == 0)
        {
            return name;
        }

        return $"{name}{numberOfUses}";
    }
}

public static class MethodGenerationHelper
{
    public static string GenerateMethod(
        MethodToGenerate methodToGenerate,
        CancellationToken cancellationToken)
    {
        StringBuilder sb = new StringBuilder();
        ParameterNamePostfixTransformer parameterNamePostfixTransformer = null!;

        // Small optimization. If there are no user parameterNames then we wont use ParameterNamePostfixTransformer 
        if (methodToGenerate.ParameterTransformations.Count() + methodToGenerate.ParametersToAdd.Length > 0)
        {
            parameterNamePostfixTransformer = new ParameterNamePostfixTransformer(methodToGenerate.PublicMethod.Parameters.Select(x => x.Name));
        }

        var publicMethod = methodToGenerate.PublicMethod;
        cancellationToken.ThrowIfCancellationRequested();

        if (publicMethod.IsGenericMethod)
        {
            return "";
        }

        var syntax = ((MethodDeclarationSyntax?)publicMethod
           .DeclaringSyntaxReferences.FirstOrDefault()
          ?.GetSyntax());

        // action must be explicitly defined
        if (syntax == null)
        {
            return "";
        }

        string? returnType = publicMethod.ReturnType.Name;

        if (publicMethod.ReturnType is not INamedTypeSymbol fullReturnType)
        {
            return "";
        }

        sb.Append(@"
    /// <summary>
    ///     Calls <see cref=""");
        sb.Append(methodToGenerate.ContainingControllerFullyQualifiedName);
        sb.Append(".");
        sb.Append(publicMethod.Name);
        sb.AppendLine(@""" />.
    /// </summary>");
        sb.Append(@"    public async ");

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

        cancellationToken.ThrowIfCancellationRequested();

        sb.Append(" ");
        sb.Append("Call_");
        sb.Append(publicMethod.Name);
        sb.Append("(");

        var nonRemovedArgumentNames = new List<string>();

        var stringBuilderForOptionalParameters = new StringBuilder();
        foreach (var publicMethodParameter in publicMethod.Parameters)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ProcessParameter(sb, stringBuilderForOptionalParameters, methodToGenerate, publicMethodParameter, nonRemovedArgumentNames, parameterNamePostfixTransformer);
        }

        foreach (var addParameter in methodToGenerate.ParametersToAdd)
        {
            ProcessParameterAddedByUser(sb, stringBuilderForOptionalParameters, addParameter, parameterNamePostfixTransformer);
        }

        sb.Append(stringBuilderForOptionalParameters);

        sb.AppendLine(@"        IEnumerable<(string Key, string? Value)>? headers = null,
            AuthenticationHeaderValue? authenticationHeaderValue = null,
            IEnumerable<IActionInfoTransformer>? actionInfoTransformers = null,
            IEnumerable<IHttpRequestPipelinePart>? httpRequestPipelineParts = null
        )");

        sb.AppendLine(@"    {");
        sb.Append(@"        var methodName = ");
        sb.Append("nameof(");
        sb.Append(methodToGenerate.ContainingControllerFullyQualifiedName);
        sb.Append(".");
        sb.Append(publicMethod.Name);
        sb.AppendLine(");");

        sb.Append(@"        var controllerType = typeof(");
        sb.Append(methodToGenerate.ContainingControllerFullyQualifiedName);
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


        sb.AppendLine(@"
        var requestBuilder = _requestBuilder.CreateNewBuilderByCopyingExisting();
        requestBuilder.AddHeaders(headers);
        requestBuilder.AddAuthenticationHeaderValue(authenticationHeaderValue);
        requestBuilder.AddHttpRequestPipelineParts(httpRequestPipelineParts);
        requestBuilder.AddActionInfoTransformers(actionInfoTransformers);
        var caller = new ActionCaller(requestBuilder,
            _logWriter,
            _httpClient,
            _serviceProvider,
            _ridgeSerializer);");


        sb.AppendLine(@"
        var methodInfo = controllerType.GetMethod(methodName, new Type[] {");

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
        });");
        sb.AppendLine(@"
        if (methodInfo == null)
        {
            throw new InvalidOperationException($""Method with name {methodName} not found in class {controllerType.FullName}."");
        }
    ");

        if (methodToGenerate.UseHttpResponseMessageAsReturnType)
        {
            sb.AppendLine(@"        return await caller.CallActionWithHttpResponseMessageResult(arguments, methodInfo);");
        }
        else
        {
            if (returnType == null)
            {
                sb.AppendLine(@"        return await caller.CallAction(arguments, methodInfo);");
            }
            else
            {
                sb.AppendLine($@"        return await caller.CallAction<{returnType}>(arguments, methodInfo);");
            }
        }

        sb.AppendLine(@"    }");
        sb.AppendLine();


        return sb.ToString();
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

public class MethodToGenerate : IEquatable<MethodToGenerate>
{
    public readonly AddParameter[] ParametersToAdd;
    public readonly IMethodSymbol PublicMethod;
    public bool UseHttpResponseMessageAsReturnType;
    public IDictionary<string, ParameterTransformation> ParameterTransformations;
    public readonly string ContainingControllerFullyQualifiedName;
    public readonly int MethodHash;

    private readonly Lazy<string> _generatedMethod;

    public MethodToGenerate(
        IMethodSymbol publicMethod,
        bool useHttpResponseMessageAsReturnType,
        IDictionary<string, ParameterTransformation> parameterTransformations,
        string containingControllerFullyQualifiedName,
        int methodHash,
        AddParameter[] parametersToAdd,
        CancellationToken cancellationToken)
    {
        ParametersToAdd = parametersToAdd;
        PublicMethod = publicMethod;
        UseHttpResponseMessageAsReturnType = useHttpResponseMessageAsReturnType;
        ParameterTransformations = parameterTransformations;
        ContainingControllerFullyQualifiedName = containingControllerFullyQualifiedName;
        MethodHash = methodHash;
        _generatedMethod = new Lazy<string>(() => MethodGenerationHelper.GenerateMethod(this, cancellationToken));
    }

    public string GenerateMethod(
        CancellationToken cancellationToken)
    {
        return _generatedMethod.Value;
    }

    public bool Equals(
        MethodToGenerate? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }


        var everythingIsSame = ContainingControllerFullyQualifiedName == other.ContainingControllerFullyQualifiedName && MethodHash == other.MethodHash
                                                                                                                      && UseHttpResponseMessageAsReturnType == other.UseHttpResponseMessageAsReturnType;

        if (!everythingIsSame)
        {
            return false;
        }

        if (ParameterTransformations.Count != other.ParameterTransformations.Count)
        {
            return false;
        }

        foreach (var keyValuePair in ParameterTransformations)
        {
            var isPresent = other.ParameterTransformations.TryGetValue(keyValuePair.Key, out var otherParamTransformation);
            if (!isPresent)
            {
                return false;
            }

            if (!keyValuePair.Value.Equals(otherParamTransformation))
            {
                return false;
            }
        }

        if (ParametersToAdd.Length != other.ParametersToAdd.Length)
        {
            return false;
        }

        for (int i = 0; i < ParametersToAdd.Length - 1; i++)
        {
            if (!ParametersToAdd[i].Equals(other.ParametersToAdd[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(
        object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((MethodToGenerate)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (ContainingControllerFullyQualifiedName.GetHashCode() * 397) ^ MethodHash;
        }
    }
}
