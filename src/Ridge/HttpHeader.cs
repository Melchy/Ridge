namespace Ridge;

/// <summary>
///     Represents http header
/// </summary>
public class HttpHeader
{
    /// <summary>
    ///     Header key
    /// </summary>
    public string Key { get; }

    /// <summary>
    ///     Header value
    /// </summary>
    public string Value { get; }

    /// <summary>
    ///     Creates <see cref="HttpHeader" />
    /// </summary>
    /// <param name="key">Header key</param>
    /// <param name="value">Header value</param>
    public HttpHeader(
        string key,
        string value)
    {
        Key = key;
        Value = value;
    }
}
