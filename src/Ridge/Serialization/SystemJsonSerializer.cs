using System.Text.Json;

namespace Ridge.Serialization;

/// <summary>
///     SystemJson implementation of serializer.
/// </summary>
public class SystemJsonSerializer : IRequestResponseSerializer
{
    /// <inheritdoc />
    public TResult Deserialize<TResult>(
        string data)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        return JsonSerializer.Deserialize<TResult>(data, options)!;
    }

    /// <inheritdoc />
    public string? Serialize(
        object? obj)
    {
        return JsonSerializer.Serialize(obj);
    }
}
