namespace Ridge.Parameters.CustomParams;

/// <summary>
///     Represents query parameter.
/// </summary>
public class QueryOrRouteParameter : CustomParameter
{
    /// <summary>
    ///     Create <see cref="QueryOrRouteParameter" />.
    /// </summary>
    /// <param name="name">Query/Route parameter name.</param>
    /// <param name="value">Query/Route parameter value.</param>
    public QueryOrRouteParameter(
        string name,
        object? value)
        : base(name, value)
    {
    }
}
