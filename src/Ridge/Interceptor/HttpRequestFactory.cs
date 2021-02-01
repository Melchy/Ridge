using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Ridge.Interceptor
{
    public static class HttpRequestFactory
    {
        public static HttpRequestMessage Create(
            string httpMethod,
            string url,
            object? contentData,
            Guid callId,
            string contentType,
            IDictionary<string, object?> headerParams)
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
                httpMethodObject == HttpMethod.Put ||
                httpMethodObject == HttpMethod.Patch)
            {
                request.Content = CreateContent(contentType, contentData);
            }

            foreach (var headerParam in headerParams)
            {
                request.Headers.Add(headerParam.Key, headerParam.Value?.ToString());
            }

            request.Headers.Add("callId", callId.ToString());
            return request;
        }

        private static ByteArrayContent CreateContent(string content, object? obj)
        {
            if (content == "application/json")
            {
                return new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, content);
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
