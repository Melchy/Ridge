namespace Ridge.Parameters.AdditionalParams;

/// <summary>
///     Represents body.
/// </summary>
public class BodyParameter : AdditionalParameter
{
    /// <summary>
    ///     Create <see cref="BodyParameter" />.
    /// </summary>
    /// <param name="value">Body value.</param>
    public BodyParameter(
        object? value)
        : base("custom body parameter", value)
    {
    }
}
