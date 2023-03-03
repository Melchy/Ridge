using System.Collections;
using System.Collections.Generic;

namespace Ridge.Parameters.ActionAndClientParams;

/// <summary>
///     Collection of <see cref="ActionAndClientParameterLinked" />.
/// </summary>
public class ActionAndClientParametersLinked : IEnumerable<ActionAndClientParameterLinked>
{
    private readonly IEnumerable<ActionAndClientParameterLinked> _actionAndClientParametersLinked;

    /// <summary>
    ///     Crate new <see cref="ActionAndClientParameterLinked" />.
    /// </summary>
    /// <param name="actionAndClientParametersLinked"></param>
    public ActionAndClientParametersLinked(
        IEnumerable<ActionAndClientParameterLinked> actionAndClientParametersLinked)
    {
        _actionAndClientParametersLinked = actionAndClientParametersLinked;
    }

    /// <inheritdoc />
    public IEnumerator<ActionAndClientParameterLinked> GetEnumerator()
    {
        return _actionAndClientParametersLinked.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
