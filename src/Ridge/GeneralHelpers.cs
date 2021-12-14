using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Ridge
{
    internal static class GeneralHelpers
    {
        public static object CreateObjectWithoutCallingConstructor(
            Type type)
        {
            return FormatterServices.GetUninitializedObject(type);
        }

        public static bool HasAttribute<TAttribute>(
            ParameterInfo parameterInfo)
            where TAttribute : Attribute
        {
            return parameterInfo.GetCustomAttributes(typeof(TAttribute), true).Any();
        }

        public static bool HasAttribute<TAttribute>(
            MethodInfo methodInfo)
            where TAttribute : Attribute
        {
            var attributeType = typeof(TAttribute);
            return methodInfo.GetCustomAttributes(attributeType, true).Any();
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
#if NET6_0_OR_GREATER
                type == typeof(DateOnly) ||
                type == typeof(TimeOnly) ||
#endif
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

        public static IEnumerable<MethodInfo> GetPublicMethods(
            Type type)
        {
            return type.GetMethods(
                    BindingFlags.Instance |
                    BindingFlags.Public)
                .Where(x => !x.Name.StartsWith("set", StringComparison.InvariantCulture) && !x.Name.StartsWith("get", StringComparison.InvariantCulture))
                .Where(x => x.IsPublic);
        }

        public static IDictionary<string, string>? ToKeyValue(
            object? metaToken)
        {
            if (metaToken == null)
            {
                return null;
            }

            var token = metaToken as JToken;
            if (token == null)
            {
                return ToKeyValue(JObject.FromObject(metaToken));
            }

            if (token.HasValues)
            {
                var contentData = new Dictionary<string, string>();
                foreach (var child in token.Children().ToList())
                {
                    var childContent = ToKeyValue(child);
                    if (childContent != null)
                    {
                        contentData = contentData.Concat(childContent)
                            .ToDictionary(k => k.Key, v => v.Value);
                    }
                }

                return contentData;
            }

            var jValue = token as JValue;
            if (jValue?.Value == null)
            {
                return null;
            }

            var value = jValue.Type == JTokenType.Date ? jValue.ToString("o", CultureInfo.InvariantCulture) : jValue.ToString(CultureInfo.InvariantCulture);

            return new Dictionary<string, string> { { token.Path, value } };
        }

        public static Type GetReturnTypeOrGenericArgumentOfTask(
            MethodInfo methodInfo)
        {
            var actionReturnType = methodInfo.ReturnType;
            if (typeof(Task).IsAssignableFrom(methodInfo.ReturnType))
            {
                actionReturnType = methodInfo.ReturnType.GetGenericArguments()[0];
            }

            return actionReturnType;
        }

        public static PropertyInfo GetProperty(
            Type type,
            string propertyName)
        {
            var property = type.GetProperty(propertyName, BindingFlagsAny.Get());
            if (property == null)
            {
                throw new InvalidOperationException($"Property with name {propertyName} not found on type {type.Name}");
            }

            return property;
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

        [SuppressMessage("", "CA1508", Justification = "false positive")]
        public static object CreateInstance(
            Type type,
            params object?[] args)
        {
            try
            {
                object? instance = Activator.CreateInstance(type,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    args,
                    null);
                if (instance == null)
                {
                    throw new InvalidOperationException("Nullable value type can not be proxied.");
                }

                return instance;
            }
            catch (MissingMethodException)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Type {0} does not have constructor with {1} arguments or some arguments have incorrect types.", type.Name, args.Length));
            }
        }

        public static Dictionary<TKey, TValue> MergeDictionaries<TKey, TValue>(
            params IDictionary<TKey, TValue>[] dictionaries)
            where TKey : notnull
        {
            return dictionaries.SelectMany(x => x)
                .ToDictionary(x => x.Key, y => y.Value);
        }

        public static class BindingFlagsAny
        {
            public static BindingFlags Get()
            {
                return BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            }
        }
    }
}
