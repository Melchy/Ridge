using Ridge.Interceptor.ActionInfo.Dtos;
using Ridge.Interceptor.InterceptorFactory;
using Ridge.Middlewares;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Ridge.Interceptor.ActionInfo
{
    internal interface IGetInfo
    {
        public ActionInfoDto GetInfo<T>(
            IEnumerable<object> arguments,
            MethodInfo methodInfo,
            PreCallMiddlewareCaller preCallMiddlewareCaller);
    }
}
