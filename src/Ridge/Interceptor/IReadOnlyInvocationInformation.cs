using System.Collections.Generic;

namespace Ridge.Interceptor
{
    public interface IReadOnlyInvocationInformation
    {
        public object? Body { get; }
        public IDictionary<string, object?> RouteParams { get; }
        public string BodyFormat { get; }
        public IReadOnlyDictionary<string, object?> HeaderParams { get; }
        public IEnumerable<object?> Arguments { get; }
    }
}
