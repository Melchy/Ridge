﻿using Ridge.Interceptor;
using Ridge.Middlewares.Public;
using System.Threading.Tasks;

namespace Ridge.Middlewares.DefaulMiddlewares
{
    public class FinalPreCallMiddleware : PreCallMiddleware
    {
        public override void Invoke(
            PreCallMiddlewareDelegate next,
            IInvocationInformation invocationInformation)
        {
            return;
        }
    }
}