﻿using CommonExtensionMethods;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Ridge.Interceptor.ActionInfo.Dtos
{
    internal class ActionArgumentsInfo : IInvocationInformation, IReadOnlyInvocationInformation
    {
        public object? Body { get; set; }
        public IDictionary<string, object?> RouteParams { get; set; }
        public string BodyFormat { get; set; }
        public IDictionary<string, object?> HeaderParams { get; set; }
        public IEnumerable<object?> Arguments { get; }
        IReadOnlyDictionary<string, object?> IReadOnlyInvocationInformation.HeaderParams => new ReadOnlyDictionary<string, object?>(HeaderParams);

        private ActionArgumentsInfo(
            object? body,
            IDictionary<string, object?> routeParams,
            string bodyFormat,
            IDictionary<string, object?> headerParams,
            IEnumerable<object?> arguments)
        {
            Body = body;
            RouteParams = routeParams;
            BodyFormat = bodyFormat;
            HeaderParams = headerParams;
            Arguments = arguments;
        }

        public void AddArea(
            string area)
        {
            RouteParams["area"] = area;
        }

        public static ActionArgumentsInfo CreateActionInfo(IEnumerable<object?> methodArguments, MethodInfo methodInfo)
        {
            var methodParams = CreateParametersWithValues(methodArguments, methodInfo);
            var body = GetBody(methodParams);
            var routeParams = GetRouteAndQueryParamsFromMethodArguments(methodParams);
            var headerParams = GetHeadersFromMethodArguments(methodParams);
            var bodyFormat = GetBodyFormat(methodParams);
            return new ActionArgumentsInfo(body, routeParams, bodyFormat, headerParams, methodArguments);
        }

        private static IEnumerable<(ParameterInfo parameterReflection, object? parameterValue)> CreateParametersWithValues(IEnumerable<object?> methodArguments, MethodInfo methodInfo)
        {
            return methodInfo.GetParameters()
                .Zip(methodArguments, (parameterReflection, parameterValue) => (parameterReflection, parameterValue));
        }

        private static string GetBodyFormat(IEnumerable<(ParameterInfo parameterReflection, object? parameterValue)> methodParams)
        {
            var hasParameterWithAttributeFromForm = methodParams
                .Any(x => GeneralHelpers.HasAttribute<FromFormAttribute>(x.parameterReflection));
            if (hasParameterWithAttributeFromForm)
            {
                return "application/x-www-form-urlencoded";
            }
            else
            {
                return "application/json";
            }
        }

        private static IDictionary<string, object?> GetHeadersFromMethodArguments(IEnumerable<(ParameterInfo parameterReflection, object? value)> methodParams)
        {
            var fromHeadParams = methodParams.Where(x =>
                GeneralHelpers.HasAttribute<FromHeaderAttribute>(x.parameterReflection));
            var routeDataDictionary = new Dictionary<string, object?>();
            foreach (var attribute in fromHeadParams)
            {
                var parameterNameInRequest = GetAttributeNamePropertyOrParameterName(attribute.parameterReflection);
                if (attribute.value == null)
                {
                    routeDataDictionary[parameterNameInRequest] = attribute.value;
                    continue;
                }

                if (GeneralHelpers.IsSimpleType(attribute.value.GetType()))
                {
                    routeDataDictionary[parameterNameInRequest] = attribute.value;
                }
                else
                {
                    throw new InvalidOperationException($"FromHeader attribute does not support complex types. Parameter name: {attribute.parameterReflection.Name}.");
                }
            }
            return routeDataDictionary;
        }

        private static IDictionary<string, object?> GetRouteAndQueryParamsFromMethodArguments(
            IEnumerable<(ParameterInfo parameterReflection, object? value)> methodParams)
        {
            var formRouteDictionary = GetFromRouteParams(methodParams);
            var fromQueryDictionary = GetFromQueryParams(methodParams);
            if (formRouteDictionary.Keys.Intersect(fromQueryDictionary.Keys).Any())
            {
                throw new InvalidOperationException("Argument with FromRoute has same Name as argument with attribute FromQuery.");
            }
            return GeneralHelpers.MergeDictionaries(formRouteDictionary, fromQueryDictionary);
        }

        private static IDictionary<string, object?> GetFromRouteParams(IEnumerable<(ParameterInfo parameterReflection, object? value)> methodParams)
        {
            var fromRouteParams = methodParams.Where(x =>
                GeneralHelpers.HasAttribute<FromRouteAttribute>(x.parameterReflection));

            IDictionary<string, object?> routeDataDictionary = new Dictionary<string, object?>();
            foreach (var fromRouteParam in fromRouteParams)
            {
                var parameterNameInRequest = GetAttributeNamePropertyOrParameterName(fromRouteParam.parameterReflection);
                if (fromRouteParam.value == null)
                {
                    throw new InvalidOperationException($"Argument with [FromRoute] attribute can not contain null. Parameter containing null: {fromRouteParam.parameterReflection.Name}. " +
                                                        $"[FromRoute] can not contain null because route would not match any endpoint or it would match incorrect endpoint.");
                }

                if (GeneralHelpers.IsSimpleType(fromRouteParam.value.GetType()))
                {
                    routeDataDictionary[parameterNameInRequest] = fromRouteParam.value;
                }
                else
                {
                    throw new InvalidOperationException($"Complex arguments in [FromRoute] are not supported.  Parameter name: {fromRouteParam.parameterReflection.Name}.");
                }
            }

            return routeDataDictionary;
        }


        private static IDictionary<string, object?> GetFromQueryParams(IEnumerable<(ParameterInfo parameterReflection, object? Value)> methodParams)
        {
            var fromQueryParams = methodParams.Where(x =>
                GeneralHelpers.HasAttribute<FromQueryAttribute>(x.parameterReflection));

            IDictionary<string, object?> routeDataDictionary = new Dictionary<string, object?>();
            foreach (var attribute in fromQueryParams)
            {
                var parameterNameInRequest = GetAttributeNamePropertyOrParameterName(attribute.parameterReflection);
                if (attribute.Value == null)
                {
                    routeDataDictionary[parameterNameInRequest] = attribute.Value;
                    continue;
                }

                if (GeneralHelpers.IsSimpleType(attribute.Value.GetType()))
                {
                    routeDataDictionary[parameterNameInRequest] = attribute.Value;
                }
                else
                {

                    if (HandleIEnumerable(attribute.Value, routeDataDictionary, parameterNameInRequest))
                    {
                        continue;
                    }

                    var routeDataDictionaryTemp = ConvertObjectToDictionaryWithNestedProperties(attribute.Value);
                    foreach (var innerAttribute in routeDataDictionaryTemp)
                    {
                        routeDataDictionary[$"{parameterNameInRequest}.{innerAttribute.Key}"] = innerAttribute.Value;
                    }
                }
            }

            return routeDataDictionary;
        }

        private static bool HandleIEnumerable(
            object argumentValue,
            IDictionary<string, object?> routeDataDictionary,
            string parameterNameInRequest)
        {
            if (argumentValue is IEnumerable)
            {
                var genericArguments = argumentValue.GetType().GetGenericArguments();
                if (genericArguments.Length != 1)
                {
                    routeDataDictionary[parameterNameInRequest] = argumentValue;
                    return true;
                }
                var genericArgument = genericArguments[0];
                if (!GeneralHelpers.IsSimpleType(genericArgument))
                {
                    throw new InvalidOperationException($"IEnumerable with complex type is not supported");
                }

                routeDataDictionary[parameterNameInRequest] = argumentValue;
                return true;
            }

            return false;
        }

        public static IDictionary<string, object?> ConvertObjectToDictionaryWithNestedProperties(object obj)
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
                }else if (innerAttribute.Value is IEnumerable)
                {
                    routeDataDictionary[innerAttribute.Key] = innerAttribute.Value;
                }
                else
                {
                    if (HandleIEnumerable(innerAttribute.Value, routeDataDictionary, innerAttribute.Key))
                    {
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

        private static string GetAttributeNamePropertyOrParameterName(
            ParameterInfo parameter)
        {
            var modelNameProvider = (IModelNameProvider?)parameter.GetCustomAttributes(typeof(IModelNameProvider), true).FirstOrDefault();
            if (modelNameProvider == null)
            {
                throw new InvalidOperationException($"Parameter must have attribute implementing {nameof(IModelNameProvider)}");
            }

            if (modelNameProvider.Name.IsNullOrEmpty())
            {
                // This value can not be null. Null is only for return parameter
                return parameter.Name!;
            }
            else
            {
                return modelNameProvider.Name;
            }
        }


        private static object? GetBody(
            IEnumerable<(ParameterInfo parameterInfo, object? value)> methodParams)
        {
            var parametersWithFromForm = methodParams.Where(
                x => GeneralHelpers.HasAttribute<FromFormAttribute>(x.parameterInfo)).ToList();
            var parametersWithFromBody = methodParams.Where(
                x => GeneralHelpers.HasAttribute<FromBodyAttribute>(x.parameterInfo)).ToList();

            if (parametersWithFromBody.Count > 1)
            {
                throw new InvalidOperationException(
                    $"Action can not contain more than one {nameof(FromBodyAttribute)}");
            }

            if (parametersWithFromForm.Count > 1)
            {
                throw new InvalidOperationException(
                    $"Action can not contain more than one {nameof(FromFormAttribute)}");
            }

            if (parametersWithFromForm.Any() && parametersWithFromBody.Any())
            {
                throw new InvalidOperationException(
                    $"Action can not contain {nameof(FromFormAttribute)} and {nameof(FromBodyAttribute)}");
            }

            if (parametersWithFromBody.Any())
            {
                return parametersWithFromBody.First().value;
            }
            if (parametersWithFromForm.Any())
            {
                return parametersWithFromForm.First().value;
            }

            return new object();
        }
    }
}