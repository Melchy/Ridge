using Ridge.ActionInfo;
using Ridge.Interceptor;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ridge.Transformers
{
    /// <summary>
    ///     Calls all the registered <see cref="IActionInfoTransformer" /> to transform <see cref="IActionInfo" />.
    /// </summary>
    public class ActionInfoTransformersCaller
    {
        private readonly List<IActionInfoTransformer> _transformers;

        /// <summary>
        /// </summary>
        /// <param name="transformers">Used to transform <see cref="IActionInfo" /></param>
        public ActionInfoTransformersCaller(
            List<IActionInfoTransformer> transformers)
        {
            _transformers = transformers;
        }

        /// <summary>
        ///     Calls all the registered <see cref="IActionInfoTransformer" /> to transform <see cref="IActionInfo" />.
        /// </summary>
        /// <param name="actionInfo"><see cref="ActionInfo" /> to transform</param>
        /// <param name="methodInvocationInfo">
        ///     <see cref="MethodInvocationInfo" /> can be used to gather information about the call
        ///     in transformers.
        /// </param>
        public async Task Call(
            IActionInfo actionInfo,
            MethodInvocationInfo methodInvocationInfo)
        {
            foreach (var actionInfoTransformer in _transformers)
            {
                await actionInfoTransformer.TransformAsync(actionInfo, methodInvocationInfo);
            }
        }
    }
}
