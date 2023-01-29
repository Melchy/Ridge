using System.Collections;
using System.Collections.Generic;

namespace Ridge.Parameters.ActionAndCallerParams;

/// <summary>
///     Collection of <see cref="ActionAndCallerParameterLinked" />.
/// </summary>
public class ActionAndCallerParametersLinked : IEnumerable<ActionAndCallerParameterLinked>
{
    private readonly IEnumerable<ActionAndCallerParameterLinked> _controllerAndCallerParametersLinked;

    /// <summary>
    ///     Crate new <see cref="ActionAndCallerParameterLinked" />.
    /// </summary>
    /// <param name="controllerAndCallerParametersLinked"></param>
    public ActionAndCallerParametersLinked(
        IEnumerable<ActionAndCallerParameterLinked> controllerAndCallerParametersLinked)
    {
        _controllerAndCallerParametersLinked = controllerAndCallerParametersLinked;
    }

    /// <inheritdoc />
    public IEnumerator<ActionAndCallerParameterLinked> GetEnumerator()
    {
        return _controllerAndCallerParametersLinked.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
