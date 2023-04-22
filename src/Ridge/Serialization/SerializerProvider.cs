using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using Ridge.Setup;
using System;

namespace Ridge.Serialization;

internal class SerializerProvider
{
    private readonly IActionResultExecutor<JsonResult>? _actionResultExecutor;
    private readonly RidgeOptions _ridgeOptions;

    public SerializerProvider(
        IOptions<RidgeOptions> ridgeOptions,
        IActionResultExecutor<JsonResult> actionResultExecutor)
    {
        ArgumentNullException.ThrowIfNull(ridgeOptions);
        _actionResultExecutor = actionResultExecutor;
        _ridgeOptions = ridgeOptions.Value;
    }

    public IRequestResponseSerializer GetSerializer()
    {
        if (_ridgeOptions.RequestResponseSerializer != null)
        {
            return _ridgeOptions.RequestResponseSerializer;
        }

        // if the type is Microsoft.AspNetCore.Mvc.Infrastructure.SystemTextJsonResultExecutor then system json is used.
        // SystemTextJsonResultExecutor is internal therefore we can not use typeof()
        if (_actionResultExecutor?.GetType().Name == "SystemTextJsonResultExecutor")
        {
            return new SystemJsonSerializer();
        }

        if (_actionResultExecutor?.GetType().Name == "NewtonsoftJsonResultExecutor")
        {
            return new JsonNetSerializer();
        }

        return new SystemJsonSerializer();
    }
}
