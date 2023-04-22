using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Ridge;

internal static class GeneralHelpers
{
    public static bool HasAttribute<TAttribute>(
        ParameterInfo parameterInfo)
        where TAttribute : Attribute
    {
        return parameterInfo.GetCustomAttributes(typeof(TAttribute), true).Any();
    }

    public static IDictionary<string, object?> ToDictionary(
        object source)
    {
        var bindingAttr = BindingFlags.Instance | BindingFlags.Public;
        var fields = source.GetType().GetFields(bindingAttr);
        var properties = source.GetType().GetProperties(bindingAttr);
        var dictionaryOfProperties = properties.ToDictionary(
            propInfo => propInfo.Name,
            propInfo => propInfo.GetValue(source, null)
        );
        var dictionaryOfFields = fields.ToDictionary(
            propInfo => propInfo.Name,
            propInfo => propInfo.GetValue(source)
        );
        var propertiesAndFields = dictionaryOfProperties.Concat(dictionaryOfFields);
        return propertiesAndFields
           .ToDictionary(pair => pair.Key, pair => pair.Value)!;
    }

    public static bool IsSimpleType(
        Type type)
    {
        if (type.IsPrimitive)
        {
            return true;
        }

        if (type == typeof(string) ||
            type == typeof(decimal) ||
            type == typeof(DateTime) ||
            type == typeof(DateTimeOffset) ||
            type == typeof(TimeSpan) ||
            type == typeof(Guid) ||
            type == typeof(DateOnly) ||
            type == typeof(TimeOnly) ||
            type.IsEnum)
        {
            return true;
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
            IsSimpleType(type.GetGenericArguments()[0]))
        {
            return true;
        }

        return Convert.GetTypeCode(type) != TypeCode.Object;
    }

    public static bool IsOrImplements(
        Type child,
        Type parentOrGivenObject)
    {
        if (parentOrGivenObject.IsGenericTypeDefinition) //open generic types can not be checked using IsAssignableFrom
        {
            return ImplementsOpenGenericType(child, parentOrGivenObject);
        }

        return parentOrGivenObject.IsAssignableFrom(child);
    }

    private static bool ImplementsOpenGenericType(
        Type type,
        Type generic)
    {
        var toCheck = type;
        while (toCheck != null && toCheck != typeof(object))
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == cur)
            {
                return true;
            }

            toCheck = toCheck.BaseType;
        }

        return false;
    }

    public static Dictionary<TKey, TValue> MergeDictionaries<TKey, TValue>(
        params IDictionary<TKey, TValue>[] dictionaries)
        where TKey : notnull
    {
        var mergedDictionaries = new Dictionary<TKey, TValue>();

        foreach (var keyValue in dictionaries.SelectMany(x => x))
        {
            mergedDictionaries[keyValue.Key] = keyValue.Value;
        }

        return mergedDictionaries;
    }
}
