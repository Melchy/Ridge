using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Reflection;

namespace Ridge
{
    // Copy of Microsoft.AspNetCore.Mvc.ApplicationModels.DefaultPageApplicationModelPartsProvider
    // DefaultPageApplicationModelPartsProvider is in assembly which requires .net core 3 (can not be used on .net standard)
    internal class RazorPageMethodHelpers
    {
        /// <summary>
        ///     Determines if the specified <paramref name="methodInfo" /> is a handler.
        /// </summary>
        /// <param name="methodInfo">The <see cref="MethodInfo" />.</param>
        /// <returns><c>true</c> if the <paramref name="methodInfo" /> is a handler. Otherwise <c>false</c>.</returns>
        /// <remarks>
        ///     Override this method to provide custom logic to determine which methods are considered handlers.
        /// </remarks>
        private static bool IsHandler(
            MethodInfo methodInfo)
        {
            // The SpecialName bit is set to flag members that are treated in a special way by some compilers
            // (such as property accessors and operator overloading methods).
            if (methodInfo.IsSpecialName)
            {
                return false;
            }

            // Overridden methods from Object class, e.g. Equals(Object), GetHashCode(), etc., are not valid.
            if (methodInfo.GetBaseDefinition().DeclaringType == typeof(object))
            {
                return false;
            }

            if (methodInfo.IsStatic)
            {
                return false;
            }

            if (methodInfo.IsAbstract)
            {
                return false;
            }

            if (methodInfo.IsConstructor)
            {
                return false;
            }

            if (methodInfo.IsGenericMethod)
            {
                return false;
            }

            if (!methodInfo.IsPublic)
            {
                return false;
            }

            if (methodInfo.IsDefined(typeof(NonHandlerAttribute)))
            {
                return false;
            }

            // Exclude the whole hierarchy of Page.
            var declaringType = methodInfo.DeclaringType;
            if (declaringType == typeof(Page) ||
                declaringType == typeof(PageBase) ||
                declaringType == typeof(RazorPageBase))
            {
                return false;
            }

            // Exclude methods declared on PageModel
            if (declaringType == typeof(PageModel))
            {
                return false;
            }

            return true;
        }

        public static bool IsMethodValidHandler(
            MethodInfo methodInfo)
        {
            return TryParseHandlerMethod(methodInfo, out _, out _);
        }

        public static (string httpMethod, string handlerName) GetHttpMethodAndHandlerName(
            MethodInfo methodInfo)
        {
            if (TryParseHandlerMethod(methodInfo, out var httpMethod, out var handlerName))
            {
                return (httpMethod!, handlerName!);
            }

            throw new InvalidOperationException($"{methodInfo.Name} is not valid handler.");
        }

        public static bool TryParseHandlerMethod(
            MethodInfo method,
            out string? httpMethod,
            out string? handler)
        {
            var methodName = method.Name;
            httpMethod = null;
            handler = null;

            if (!IsHandler(method))
            {
                return false;
            }

            // Handler method names always start with "On"
            if (!methodName.StartsWith("On") || methodName.Length <= "On".Length)
            {
                return false;
            }

            // Now we parse the method name according to our conventions to determine the required HTTP method
            // and optional 'handler name'.
            //
            // Valid names look like:
            //  - OnGet
            //  - OnPost
            //  - OnFooBar
            //  - OnTraceAsync
            //  - OnPostEditAsync

            var start = "On".Length;
            var length = methodName.Length;
            if (methodName.EndsWith("Async", StringComparison.Ordinal))
            {
                length -= "Async".Length;
            }

            if (start == length)
            {
                // There are no additional characters. This is "On" or "OnAsync".
                return false;
            }

            // The http method follows "On" and is required to be at least one character. We use casing
            // to determine where it ends.
            var handlerNameStart = start + 1;
            for (; handlerNameStart < length; handlerNameStart++)
            {
                if (char.IsUpper(methodName[handlerNameStart]))
                {
                    break;
                }
            }

            httpMethod = methodName.Substring(start, handlerNameStart - start);

            // The handler name follows the http method and is optional. It includes everything up to the end
            // excluding the "Async" suffix (if present).
            handler = handlerNameStart == length ? null : methodName.Substring(handlerNameStart, length - handlerNameStart);
            return true;
        }
    }
}
