namespace Ridge.Serialization
{
    public interface IRidgeSerializer
    {
        public TResult Deserialize<TResult>(
            string data);

        public string? Serialize(
            object? obj);

        public string? GetSerializerName();
    }
}
