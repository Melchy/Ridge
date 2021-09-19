using Ridge.Interceptor;
using System.Threading.Tasks;

namespace Ridge.Transformers
{
    /// <summary>
    ///     Transformers allows you to transform request data before URL is generated.
    /// </summary>
    public interface IActionInfoTransformer
    {
        /// <summary>
        ///     This method is called before http request is created.
        /// </summary>
        /// <param name="actionInfo">Information about action which should be transformed by your transformer.</param>
        /// <param name="invocationInfo">Information about method which was called.</param>
        /// <returns></returns>
        public Task TransformAsync(
            IActionInfo actionInfo,
            InvocationInfo invocationInfo);
    }
}
