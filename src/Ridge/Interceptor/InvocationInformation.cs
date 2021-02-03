using System.Collections.Generic;

namespace Ridge.Interceptor
{
    public interface IInvocationInformation
    {
        public object? Body { get; set; }
        public IDictionary<string, object?> RouteParams { get; set;}
        public string BodyFormat { get; set;}
        public IDictionary<string, object?> HeaderParams { get; set;}
        public IEnumerable<object?> Arguments { get; }
    }
}
