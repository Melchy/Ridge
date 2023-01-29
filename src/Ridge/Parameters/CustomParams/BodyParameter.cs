namespace Ridge.Parameters.CustomParams;

/// <summary>
///     Represents body.
/// </summary>
public class BodyParameter : CustomParameter
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
