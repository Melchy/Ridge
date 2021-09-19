using System.Collections.Generic;

namespace Ridge.Interceptor
{
    public interface IActionInfo
    {
        /// <summary>
        ///     Request body. Null if no body is present.
        /// </summary>
        public object? Body { get; set; }

        /// <summary>
        ///     Dictionary containing query and route parameters.
        ///     This dictionary also contains controller name and area name.
        /// </summary>
        public IDictionary<string, object?> RouteParams { get; set; }

        /// <summary>
        ///     Format of body - JSON or application/x-www-url-formencoded when FromForm is used
        /// </summary>
        public string BodyFormat { get; set; }

        /// <summary>
        ///     Header which will be used in request
        /// </summary>
        public IDictionary<string, object?> Headers { get; set; }

        /// <summary>
        ///     Http method of request
        /// </summary>
        public string HttpMethod { get; set; }
    }
}
