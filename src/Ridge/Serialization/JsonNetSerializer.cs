using Newtonsoft.Json;
using Ridge.AspNetCore.Serialization;

namespace Ridge.Serialization;

/// <summary>
///     JsonNet implementation of serializer.
/// </summary>
public class JsonNetSerializer : IRequestResponseSerializer
{
    /// <inheritdoc />
    public TResult Deserialize<TResult>(
        string data)
    {
        return JsonConvert.DeserializeObject<TResult>(data)!;
    }

    /// <inheritdoc />
    public string? Serialize(
        object? obj)
    {
        return JsonConvert.SerializeObject(obj);
    }
}
