using System.Text.Json;

namespace Ridge.Serialization
{
    internal class SystemJsonSerializer : IRidgeSerializer
    {
        public TResult Deserialize<TResult>(
            string data)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            return JsonSerializer.Deserialize<TResult>(data, options);
        }

        public string? Serialize(
            object? obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        public string? GetSerializerName()
        {
            return "System.Text.Json";
        }
    }
}
