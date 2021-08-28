namespace Ridge.Serialization
{
    internal class SystemJsonSerializer : IRidgeSerializer
    {
        public TResult Deserialize<TResult>(string data)
        {
            return System.Text.Json.JsonSerializer.Deserialize<TResult>(data);
        }

        public string? Serialize(object? obj)
        {
            return System.Text.Json.JsonSerializer.Serialize(obj);
        }

        public string? GetSerializerName()
        {
            return "System.Text.Json";
        }
    }
}
