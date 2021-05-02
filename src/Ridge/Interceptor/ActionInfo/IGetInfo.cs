using Ridge.Transformers;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Ridge.Interceptor.ActionInfo
{
    internal interface IGetInfo
    {
        public Task<(string url, Dtos.ActionInfo actionArgumentsInfo)> GetInfo<T>(
            IEnumerable<object?> arguments,
            MethodInfo methodInfo,
            ActionInfoTransformersCaller actionInfoTransformersCaller);
    }
}
