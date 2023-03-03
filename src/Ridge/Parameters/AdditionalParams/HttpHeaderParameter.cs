namespace Ridge.Parameters.AdditionalParams;

/// <summary>
///     Represents http header.
/// </summary>
public class HttpHeaderParameter : AdditionalParameter
{
    /// <summary>
    ///     Header key.
    /// </summary>
    public string Key => Name;

    /// <summary>
    ///     Value.
    /// </summary>
    public new string? Value
    {
        get => (string?)base.Value;
        set => base.Value = value;
    }

    /// <summary>
    ///     Create <see cref="HttpHeaderParameter" />.
    /// </summary>
    /// <param name="key">Header key.</param>
    /// <param name="value">Header value.</param>
    public HttpHeaderParameter(
        string key,
        string value)
        : base(key, value)
    {
    }
}
