﻿using Ridge.Parameters.ActionParams;
using Ridge.Parameters.CallerParams;

namespace Ridge.Parameters.ActionAndCallerParams;

/// <summary>
///     Represents controller and caller parameter linked together.
/// </summary>
public class ActionAndCallerParameterLinked
{
    /// <summary>
    ///     Creates new <see cref="ActionAndCallerParameterLinked" />
    /// </summary>
    /// <param name="actionParameter">Parameter from controller.</param>
    /// <param name="callerParameter">Parameter from caller.</param>
    /// <param name="wasParameterAddedOrTransformed">True if ridge source generator transformed or added the parameter.</param>
    public ActionAndCallerParameterLinked(
        ActionParameter? actionParameter,
        CallerParameter? callerParameter,
        bool wasParameterAddedOrTransformed)
    {
        ActionParameter = actionParameter;
        CallerParameter = callerParameter;
        WasParameterAddedOrTransformed = wasParameterAddedOrTransformed;
    }

    /// <summary>
    ///     Parameter from controller.
    /// </summary>
    public ActionParameter? ActionParameter { get; }

    /// <summary>
    ///     Parameter from caller.
    /// </summary>
    public CallerParameter? CallerParameter { get; }

    /// <summary>
    ///     True if ridge source generator transformed or added the parameter.
    /// </summary>
    public bool WasParameterAddedOrTransformed { get; }

    /// <summary>
    ///     True if parameter exists in controller.
    ///     Parameter might not exist in controller when it was added using source generator.
    /// </summary>
    /// <returns>True if parameter exists in controller.</returns>
    public bool DoesParameterExistInController()
    {
        return ActionParameter != null;
    }

    /// <summary>
    ///     True if parameter exists in caller.
    ///     Parameter might not exist in caller when it was removed using source generator.
    /// </summary>
    /// <returns>True if parameter exists in caller.</returns>
    public bool DoesParameterExistsInCaller()
    {
        return CallerParameter != null;
    }
}