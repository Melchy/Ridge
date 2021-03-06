﻿using System.Collections.Generic;

namespace Ridge.Interceptor
{
    public interface IReadOnlyActionInfo
    {
        /// <summary>
        /// Request body. Null if no body is present.
        /// </summary>
        public object? Body { get; }
        /// <summary>
        /// Dictionary containing query and route parameters.
        /// This dictionary also contains controller name and area name.
        /// </summary>
        public IDictionary<string, object?> RouteParams { get; }
        /// <summary>
        /// Format of body - JSON or application/x-www-url-formencoded when FromForm is used
        /// </summary>
        public string BodyFormat { get; }
        /// <summary>
        /// Header which will be used in request
        /// </summary>
        public IReadOnlyDictionary<string, object?> Headers { get; }
        /// <summary>
        /// Http method of request
        /// </summary>
        public string HttpMethod { get; }
    }
}
