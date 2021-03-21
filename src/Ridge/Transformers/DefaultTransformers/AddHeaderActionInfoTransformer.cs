using Ridge.Interceptor;
using System.Threading.Tasks;

namespace Ridge.Transformers.DefaultTransformers
{
    public class AddHeaderActionInfoTransformer : IActionInfoTransformer
    {
        private readonly string _key;
        private readonly object? _value;

        public AddHeaderActionInfoTransformer(string key, object? value)
        {
            _key = key;
            _value = value;
        }

        public async Task TransformAsync(
            IActionInfo actionInfo,
            InvocationInfo invocationInfo)
        {
            actionInfo.Headers.Add(_key,_value);
        }
    }
}
