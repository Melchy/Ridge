﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ridge.Parameters.ActionAndCallerParams;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ridge.HttpRequestFactoryMiddlewares.Internal;

internal static class ParameterAnalyzer
{
    public static string GetAttributeNamePropertyOrParameterName(
        ActionAndCallerParameterLinked actionAndCallerParameterLinked)
    {
        if (actionAndCallerParameterLinked.ActionParameter == null)
        {
            return actionAndCallerParameterLinked.CallerParameter!.Name;
        }

        var parameter = actionAndCallerParameterLinked.ActionParameter.ParameterInfo;

        var modelNameProvider = (IModelNameProvider?)parameter.GetCustomAttributes(typeof(IModelNameProvider), true).FirstOrDefault();
        if (modelNameProvider == null)
        {
            return parameter.Name!;
        }

        if (string.IsNullOrEmpty(modelNameProvider.Name))
        {
            // This value can not be null. Null is only for return parameter
            return parameter.Name!;
        }

        return modelNameProvider.Name;
    }

    public static IEnumerable<(string Key, string? Value)> AnalyzeHeader(
        ActionAndCallerParameterLinked headerParameter)
    {
        List<(string Key, string? Value)> resultHeaders = new();
        var parameterNameInRequest = GetAttributeNamePropertyOrParameterName(headerParameter);
        var parameterValue = headerParameter.CallerParameter!.Value;

        if (parameterValue == null)
        {
            resultHeaders.Add((parameterNameInRequest, null));
            return resultHeaders;
        }

        var parameterType = parameterValue.GetType();
        if (GeneralHelpers.IsSimpleType(parameterType))
        {
            resultHeaders.Add((parameterNameInRequest, parameterValue.ToString()));
            return resultHeaders;
        }

        if (GeneralHelpers.IsOrImplements(parameterType, typeof(IEnumerable)))
        {
            var enumerable = (IEnumerable)parameterValue;
            foreach (var valuesOfHeader in enumerable)
            {
                resultHeaders.Add((parameterNameInRequest, valuesOfHeader.ToString()));
            }

            return resultHeaders;
        }

        // Complex types in header are not supported
        return resultHeaders;
    }

    public static IDictionary<string, object?> AnalyzeQueryOrRouteParameters(
        IEnumerable<ActionAndCallerParameterLinked> actionAndCallerParametersLinked)
    {
        IDictionary<string, object?> routeDataDictionary = new Dictionary<string, object?>();
        foreach (var controllerAndCallerParameterLinked in actionAndCallerParametersLinked)
        {
            var queryOrRouteParameters = AnalyzeQueryOrRouteParameter(controllerAndCallerParameterLinked);
            foreach (var queryOrRouteParameter in queryOrRouteParameters)
            {
                routeDataDictionary[queryOrRouteParameter.Key] = queryOrRouteParameter.Value;
            }
        }

        return routeDataDictionary;
    }

    public static IEnumerable<(string Key, object? Value)> AnalyzeQueryOrRouteParameter(
        ActionAndCallerParameterLinked actionAndCallerParameterLinked)
    {
        var result = new List<(string Key, object? Value)>();

        string parameterNameInRequest;
        if (actionAndCallerParameterLinked.ActionParameter == null)
        {
            parameterNameInRequest = actionAndCallerParameterLinked.CallerParameter!.Name;
        }
        else
        {
            parameterNameInRequest = GetAttributeNamePropertyOrParameterName(actionAndCallerParameterLinked);
        }

        var value = actionAndCallerParameterLinked.CallerParameter!.Value;
        if (value == null)
        {
            result.Add((parameterNameInRequest, value));
            return result;
        }

        if (GeneralHelpers.IsSimpleType(value.GetType()))
        {
            result.Add((parameterNameInRequest, value));
            return result;
        }

        if (value is IEnumerable)
        {
            result.Add((parameterNameInRequest, value));
            return result;
        }

        var routeDataDictionaryTemp = ConvertObjectToDictionaryWithNestedProperties(value);
        foreach (var innerAttribute in routeDataDictionaryTemp)
        {
            result.Add(($"{parameterNameInRequest}.{innerAttribute.Key}", innerAttribute.Value));
        }

        return result;
    }

    private static IDictionary<string, object?> ConvertObjectToDictionaryWithNestedProperties(
        object obj)
    {
        var innerAttributes = GeneralHelpers.ToDictionary(obj);
        var routeDataDictionary = new Dictionary<string, object?>();
        foreach (var innerAttribute in innerAttributes.ToList())
        {
            if (innerAttribute.Value == null)
            {
                routeDataDictionary[innerAttribute.Key] = innerAttribute.Value;
                continue;
            }

            if (GeneralHelpers.IsSimpleType(innerAttribute.Value.GetType()))
            {
                routeDataDictionary[innerAttribute.Key] = innerAttribute.Value;
            }
            else if (innerAttribute.Value is IEnumerable)
            {
                routeDataDictionary[innerAttribute.Key] = innerAttribute.Value;
            }
            else
            {
                if (innerAttribute.Value is IEnumerable)
                {
                    routeDataDictionary[innerAttribute.Key] = innerAttribute.Value;
                    continue;
                }

                var innerDictionary = ConvertObjectToDictionaryWithNestedProperties(innerAttribute.Value);
                foreach (var innerProperty in innerDictionary)
                {
                    routeDataDictionary[$"{innerAttribute.Key}.{innerProperty.Key}"] = innerProperty.Value;
                }
            }
        }

        return routeDataDictionary;
    }
}