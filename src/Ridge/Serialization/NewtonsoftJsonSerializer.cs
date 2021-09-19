using Newtonsoft.Json;

namespace Ridge.Serialization
{
    internal class NewtonsoftJsonSerializer : IRidgeSerializer
    {
        public TResult Deserialize<TResult>(
            string data)
        {
            return JsonConvert.DeserializeObject<TResult>(data);
        }

        public string? Serialize(
            object? obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public string? GetSerializerName()
        {
            return "NewtonsoftJson";
        }
    }
}
