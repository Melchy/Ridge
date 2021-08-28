using Newtonsoft.Json;
using Ridge.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
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
            IDictionary<string, object?> headerParams,
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

            foreach (var headerParam in headerParams)
            {
                request.Headers.Add(headerParam.Key, headerParam.Value?.ToString());
            }

            request.Headers.Add("ridgeCallId", callId.ToString());
            return request;
        }

        private static ByteArrayContent CreateContent(string content, object? obj, IRidgeSerializer serializer)
        {
            if (content == "application/json")
            {
                return new StringContent(serializer.Serialize(obj), Encoding.UTF8, content);
            }
            else if(content == "application/x-www-form-urlencoded")
            {
                return new FormUrlEncodedContent(GeneralHelpers.ToKeyValue(obj));
            }
            else
            {
                throw new InvalidOperationException($"Unsupported content type {content}");
            }
        }
    }
}
