namespace Ridge.AspNetCore.Serialization;

/// <summary>
///     Serializer which is used by ridge to serialize and deserialize request.
///     Default serializer is chosen automatically based on the settings of asp.net core.
/// </summary>
public interface IRequestResponseSerializer
{
    /// <summary>
    ///     Deserialize data.
    /// </summary>
    /// <param name="data"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public TResult Deserialize<TResult>(
        string data);

    /// <summary>
    ///     Serialize data.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public string? Serialize(
        object? obj);
}
