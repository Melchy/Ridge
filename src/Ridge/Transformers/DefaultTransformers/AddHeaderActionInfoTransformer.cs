using Ridge.Interceptor;
using System.Threading.Tasks;

namespace Ridge.Transformers.DefaultTransformers
{
    /// <summary>
    ///     Adds <see cref="IActionInfoTransformer" /> which adds header to the request.
    /// </summary>
    public class AddHeaderActionInfoTransformer : IActionInfoTransformer
    {
        private readonly string _key;
        private readonly object? _value;

        /// <summary>
        ///     Create <see cref="AddHeaderActionInfoTransformer" />.
        /// </summary>
        /// <param name="key">Header key.</param>
        /// <param name="value">Header value.</param>
        public AddHeaderActionInfoTransformer(
            string key,
            object? value)
        {
            _key = key;
            _value = value;
        }

        /// <summary>
        ///     Adds header to invocation.
        /// </summary>
        /// <param name="actionInfo"></param>
        /// <param name="invocationInfo"></param>
        public async Task TransformAsync(
            IActionInfo actionInfo,
            InvocationInfo invocationInfo)
        {
            actionInfo.Headers.Add(_key, _value);
        }
    }
}
