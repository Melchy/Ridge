using Ridge.Parameters.ActionParams;
using Ridge.Parameters.ClientParams;
using System.Diagnostics.CodeAnalysis;

namespace Ridge.Parameters.ActionAndClientParams;

/// <summary>
///     Represents action and client parameter linked together.
/// </summary>
public class ActionAndClientParameterLinked
{
    /// <summary>
    ///     Creates new <see cref="ActionAndClientParameterLinked" />
    /// </summary>
    /// <param name="actionParameter">Parameter from action.</param>
    /// <param name="clientParameter">Parameter from client.</param>
    /// <param name="wasParameterAddedOrTransformed">True if ridge source generator transformed or added the parameter.</param>
    public ActionAndClientParameterLinked(
        ActionParameter? actionParameter,
        ClientParameter? clientParameter,
        bool wasParameterAddedOrTransformed)
    {
        ActionParameter = actionParameter;
        ClientParameter = clientParameter;
        WasParameterAddedOrTransformed = wasParameterAddedOrTransformed;
    }

    /// <summary>
    ///     Parameter from action.
    /// </summary>
    public ActionParameter? ActionParameter { get; }

    /// <summary>
    ///     Parameter from client.
    /// </summary>
    public ClientParameter? ClientParameter { get; }

    /// <summary>
    ///     True if ridge source generator transformed or added the parameter.
    /// </summary>
    public bool WasParameterAddedOrTransformed { get; }

    /// <summary>
    ///     True if parameter exists in action.
    ///     Parameter might not exist in action when it was added using source generator.
    /// </summary>
    /// <returns>True if parameter exists in action.</returns>
    [MemberNotNullWhen(true, nameof(ActionParameter))]
    [MemberNotNullWhen(false, nameof(ClientParameter))]
    public bool DoesParameterExistInAction()
    {
        return ActionParameter != null;
    }

    /// <summary>
    ///     True if parameter exists in client.
    ///     Parameter might not exist in client when it was removed using source generator.
    /// </summary>
    /// <returns>True if parameter exists in client.</returns>
    [MemberNotNullWhen(true, nameof(ClientParameter))]
    [MemberNotNullWhen(false, nameof(ActionParameter))]
    public bool DoesParameterExistsInClient()
    {
        return ClientParameter != null;
    }
}
