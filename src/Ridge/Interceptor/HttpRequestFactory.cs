using Ridge.Serialization;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Ridge.Interceptor
{
    internal static class HttpRequestFactory
    {
        public static HttpRequestMessage Create(
            string httpMethod,
            string url,
            object? contentData,
            Guid callId,
            string contentType,
            HttpRequestHeaders headers,
            IRidgeSerializer serializer)
        {
            var httpMethodObject = new HttpMethod(httpMethod);
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(url, UriKind.RelativeOrAbsolute),
                Method = httpMethodObject,
            };
            if (httpMethodObject == HttpMethod.Post ||
                httpMethodObject == HttpMethod.Get ||
                httpMethodObject == HttpMethod.Delete ||
                httpMethodObject == HttpMethod.Put)
            {
                request.Content = CreateContent(contentType, contentData, serializer);
            }

            foreach (var headerParam in headers)
            {
                request.Headers.Add(headerParam.Key, headerParam.Value.Select(x => x));
            }

            request.Headers.Add("ridgeCallId", callId.ToString());
            return request;
        }

        private static ByteArrayContent? CreateContent(
            string contentType,
            object? content,
            IRidgeSerializer serializer)
        {
            if (contentType == "application/json")
            {
                var serializedContent = serializer.Serialize(content);
                return new StringContent(serializedContent!, Encoding.UTF8, contentType);
            }

            if (contentType == "application/x-www-form-urlencoded")
            {
                var contentAsDictionary = GeneralHelpers.ToKeyValue(content);
                return new FormUrlEncodedContent(contentAsDictionary!);
            }

            throw new InvalidOperationException($"Unsupported content type {contentType}");
        }
    }
}
