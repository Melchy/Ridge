using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Ridge.Interceptor
{
    /// <summary>
    ///     Contains information which will be used to call server endpoint.
    /// </summary>
    public interface IActionInfo
    {
        /// <summary>
        ///     Request body. Null if no body is present.
        /// </summary>
        public object? Body { get; set; }

        /// <summary>
        ///     Dictionary containing query and route parameters, area name and controller name.
        ///     Those parameters will be passed to <see cref="LinkGenerator"/>
        ///     to method <see cref="LinkGeneratorRouteValuesAddressExtensions.GetPathByRouteValues(LinkGenerator, string, object, PathString, FragmentString, LinkOptions)"/>
        ///     with route name "".
        /// </summary>
        public IDictionary<string, object?> RouteParams { get; set; }

        /// <summary>
        ///     Format of body - JSON or application/x-www-url-formencoded when FromForm is used.
        /// </summary>
        public string BodyFormat { get; set; }

        /// <summary>
        ///     Headers which will be used in request.
        /// </summary>
        public HttpRequestHeaders Headers { get; set; }

        /// <summary>
        ///     Http method of the request.
        /// </summary>
        public string HttpMethod { get; set; }
    }
}
