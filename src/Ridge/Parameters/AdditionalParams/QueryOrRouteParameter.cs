namespace Ridge.Parameters.AdditionalParams;

/// <summary>
///     Represents query parameter.
/// </summary>
public class QueryOrRouteParameter : AdditionalParameter
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
