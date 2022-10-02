using System.Collections.Generic;
using System.Reflection;

namespace Ridge.Interceptor
{
    /// <summary>
    ///     Represents invocation information about method.
    /// </summary>
    public class MethodInvocationInfo
    {
        /// <summary>
        ///     Arguments of the invoked method.
        /// </summary>
        public IEnumerable<object?> Arguments { get; }

        /// <summary>
        ///     Method info of the invoked method.
        /// </summary>
        public MethodInfo MethodInfo { get; }

        internal MethodInvocationInfo(
            IEnumerable<object?> arguments,
            MethodInfo methodInfo)
        {
            Arguments = arguments;
            MethodInfo = methodInfo;
        }
    }
}
