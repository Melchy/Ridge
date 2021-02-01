using Ridge.Interceptor.ActionInfo.Dtos;
using System.Collections.Generic;
using System.Reflection;

namespace Ridge.Interceptor.ActionInfo
{
    public interface IGetInfo
    {
        public ActionInfoDto GetInfo<T>(
            IEnumerable<object> arguments,
            MethodInfo methodInfo);
    }
}
