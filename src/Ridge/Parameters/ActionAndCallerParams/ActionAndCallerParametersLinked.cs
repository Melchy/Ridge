using System.Collections;
using System.Collections.Generic;

namespace Ridge.Parameters.ActionAndCallerParams;

/// <summary>
///     Collection of <see cref="ActionAndCallerParameterLinked" />.
/// </summary>
public class ActionAndCallerParametersLinked : IEnumerable<ActionAndCallerParameterLinked>
{
    private readonly IEnumerable<ActionAndCallerParameterLinked> _actionAndCallerParametersLinked;

    /// <summary>
    ///     Crate new <see cref="ActionAndCallerParameterLinked" />.
    /// </summary>
    /// <param name="actionAndCallerParametersLinked"></param>
    public ActionAndCallerParametersLinked(
        IEnumerable<ActionAndCallerParameterLinked> actionAndCallerParametersLinked)
    {
        _actionAndCallerParametersLinked = actionAndCallerParametersLinked;
    }

    /// <inheritdoc />
    public IEnumerator<ActionAndCallerParameterLinked> GetEnumerator()
    {
        return _actionAndCallerParametersLinked.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
