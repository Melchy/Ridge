using Ridge.Interceptor;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ridge.Transformers
{
    internal class ActionInfoTransformersCaller
    {
        private readonly List<IActionInfoTransformer> _transformers;

        public ActionInfoTransformersCaller(
            List<IActionInfoTransformer> transformers)
        {
            _transformers = transformers;
        }

        public async Task Call(
            IActionInfo actionInfo,
            InvocationInfo invocationInfo)
        {
            foreach (var actionInfoTransformer in _transformers)
            {
                await actionInfoTransformer.TransformAsync(actionInfo, invocationInfo);
            }
        }
    }
}
