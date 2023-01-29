using Ridge.GeneratorAttributes;
using Ridge.Parameters.ActionAndCallerParams;
using Ridge.Parameters.ActionParams;
using Ridge.Parameters.CallerParams;
using Ridge.Parameters.CustomParams;
using System.Collections.Generic;
using System.Linq;

namespace Ridge.Parameters;

/// <summary>
///     This class can be used to get controller and action parameters.
/// </summary>
public class ParameterProvider
{
    private readonly IEnumerable<RawParameterAndTransformationInfo> _parameters;
    private readonly IEnumerable<CustomParameter> _customParameters;

    /// <summary>
    ///     Crate new <see cref="ParameterProvider" />.
    /// </summary>
    /// <param name="parameters">Raw parameters from generator.</param>
    /// <param name="customParameters">Custom parameters.</param>
    public ParameterProvider(
        IEnumerable<RawParameterAndTransformationInfo> parameters,
        IEnumerable<CustomParameter> customParameters)
    {
        _parameters = parameters;
        _customParameters = customParameters;
    }

    /// <summary>
    ///     Get collection of custom parameters passed to caller.
    /// </summary>
    /// <returns>Collection of custom parameters.</returns>
    public CustomParameters GetCustomParameters()
    {
        return new CustomParameters(_customParameters);
    }

    /// <summary>
    ///     Get collection of Action and Caller parameters linked.
    /// </summary>
    /// <returns>Action and caller parameters linked.</returns>
    public ActionAndCallerParametersLinked GetControllerAndCallerParametersLinked()
    {
        var controllerAndCallerParametersLinked = _parameters.Select(x =>
        {
            ActionParameter? actionParameter = null;
            CallerParameter? callerParameter;
            if (!(x.OriginalParameterInfo == null || x.OriginalName == null || x.OriginalType == null))
            {
                actionParameter = new ActionParameter(x.OriginalParameterInfo, x.OriginalName, x.OriginalType);
            }

            if (x.WasDeleted)
            {
                callerParameter = null;
            }
            else
            {
                callerParameter = new CallerParameter(x.NameInCaller, x.TypeInCaller, x.PassedValue, x.AddedOrTransformedParameterMapping ?? ParameterMapping.None);
            }

            return new ActionAndCallerParameterLinked(actionParameter, callerParameter, x.IsTransformedOrAdded);
        });

        return new ActionAndCallerParametersLinked(controllerAndCallerParametersLinked);
    }

    /// <summary>
    ///     Get parameters of called action.
    /// </summary>
    /// <returns>Parameters of called action</returns>
    public ActionParameters GetaActionParameters()
    {
        var actionParameters = _parameters.Select(x =>
        {
            ActionParameter? actionParameter = null;
            if (!(x.OriginalParameterInfo == null || x.OriginalName == null || x.OriginalType == null))
            {
                actionParameter = new ActionParameter(x.OriginalParameterInfo, x.OriginalName, x.OriginalType);
            }

            return actionParameter;
        });

        return new ActionParameters(actionParameters.Where(x => x != null)!);
    }

    /// <summary>
    ///     Get parameters passed to caller.
    /// </summary>
    /// <returns>Parameters passed to caller</returns>
    public CallerParameters GetCallerParameters()
    {
        var callerParameters = _parameters.Select(x =>
        {
            CallerParameter? callerParameter;
            if (x.WasDeleted)
            {
                callerParameter = null;
            }
            else
            {
                callerParameter = new CallerParameter(x.NameInCaller, x.TypeInCaller, x.PassedValue, x.AddedOrTransformedParameterMapping ?? ParameterMapping.None);
            }

            return callerParameter;
        });

        return new CallerParameters(callerParameters.Where(x => x != null)!);
    }
}
