using Ridge.AspNetCore.GeneratorAttributes;
using Ridge.AspNetCore.Parameters;
using Ridge.Parameters.ActionAndClientParams;
using Ridge.Parameters.ActionParams;
using Ridge.Parameters.AdditionalParams;
using Ridge.Parameters.ClientParams;
using System.Collections.Generic;
using System.Linq;

namespace Ridge.Parameters;

/// <summary>
///     This class can be used to get action and client parameters.
/// </summary>
public class ParameterProvider
{
    private readonly IEnumerable<RawParameterAndTransformationInfo> _parameters;
    private readonly IEnumerable<AdditionalParameter> _additionalParameters;

    /// <summary>
    ///     Crate new <see cref="ParameterProvider" />.
    /// </summary>
    /// <param name="parameters">Raw parameters from generator.</param>
    /// <param name="additionalParameters">Additional parameters.</param>
    public ParameterProvider(
        IEnumerable<RawParameterAndTransformationInfo> parameters,
        IEnumerable<AdditionalParameter> additionalParameters)
    {
        _parameters = parameters;
        _additionalParameters = additionalParameters;
    }

    /// <summary>
    ///     Get collection of additional parameters passed to client.
    /// </summary>
    /// <returns>Collection of additional parameters.</returns>
    public AdditionalParameters GetAdditionalParameters()
    {
        return new AdditionalParameters(_additionalParameters);
    }

    /// <summary>
    ///     Get collection of Action and Client parameters linked.
    /// </summary>
    /// <returns>Action and client parameters linked.</returns>
    public ActionAndClientParametersLinked GetActionAndClientParametersLinked()
    {
        var actionAndClientParametersLinked = _parameters.Select(x =>
        {
            ActionParameter? actionParameter = null;
            ClientParameter? clientParameter;
            if (!(x.OriginalParameterInfo == null || x.OriginalName == null || x.OriginalType == null))
            {
                actionParameter = new ActionParameter(x.OriginalParameterInfo, x.OriginalName, x.OriginalType);
            }

            if (x.WasDeleted)
            {
                clientParameter = null;
            }
            else
            {
                clientParameter = new ClientParameter(x.NameInClient, x.TypeInClient, x.PassedValue, x.AddedOrTransformedParameterMapping ?? ParameterMapping.None);
            }

            return new ActionAndClientParameterLinked(actionParameter, clientParameter, x.IsTransformedOrAdded);
        });

        return new ActionAndClientParametersLinked(actionAndClientParametersLinked);
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
    ///     Get parameters passed to client.
    /// </summary>
    /// <returns>Parameters passed to client</returns>
    public ClientParameters GetClientParameters()
    {
        var clientParameters = _parameters.Select(x =>
        {
            ClientParameter? clientParameter;
            if (x.WasDeleted)
            {
                clientParameter = null;
            }
            else
            {
                clientParameter = new ClientParameter(x.NameInClient, x.TypeInClient, x.PassedValue, x.AddedOrTransformedParameterMapping ?? ParameterMapping.None);
            }

            return clientParameter;
        });

        return new ClientParameters(clientParameters.Where(x => x != null)!);
    }
}
