﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Ridge.Serialization
{
    internal static class SerializerProvider
    {
        public static IRidgeSerializer GetSerializer(
            IServiceProvider serviceProvider,
            IRidgeSerializer? customSerializer)
        {
            if (customSerializer != null)
            {
                return customSerializer;
            }

            var actionResultExecutor = serviceProvider.GetService<IActionResultExecutor<JsonResult>>();
            // if the type is Microsoft.AspNetCore.Mvc.Infrastructure.SystemTextJsonResultExecutor then system json is used.
            // SystemTextJsonResultExecutor is internal therefore we can not use typeof()
            if (actionResultExecutor.GetType().Name == "SystemTextJsonResultExecutor")
            {
                return new SystemJsonSerializer();
            }
            else if (actionResultExecutor.GetType().Name == "NewtonsoftJsonResultExecutor")
            {
                return new NewtonsoftJsonSerializer();
            }
            else
            {
                throw new InvalidOperationException(
                    "Could not detect correct serializer. " +
                    "Please provide custom implementation of IRidgeSerializer. " +
                    "For more information see documentation");
            }
        }
    }
}