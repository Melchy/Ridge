using Ridge.Interceptor;
using Ridge.LogWriter;
using Ridge.Pipeline.Public;
using Ridge.Serialization;
using Ridge.Transformers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Application2Tests
{
    public class Tests
    {
        //TODO fix
        // [Test]
        // public async Task ArgumentsWithoutAttributesAreSupported()
        // {
        //     using var application = CreateApplication();
        //     var testController = application.ControllerFactory.CreateController<TestController>();
        //     var complexObject = new ComplexObject()
        //     {
        //         Str = "foo",
        //         NestedComplexObject = new NestedComplexObject()
        //         {
        //             Integer = 1,
        //             Str = "br",
        //         },
        //     };
        //     var response = await testController.ArgumentsWithoutAttributes(complexObject,
        //         1,
        //         2);
        //     response.GetResult().ComplexObject.Should().BeEquivalentTo(complexObject);
        //     response.GetResult().FromQuery.Should().Be(2);
        //     response.GetResult().FromRoute.Should().Be(1);
        // }
    }


    public class Foo
    {
        public RequestBuilder _requestBuilder { get; set; } = new();

        public Foo(
            HttpClient httpClient,
            IServiceProvider serviceProvider,
            ILogWriter? logWriter = null,
            IRidgeSerializer? ridgeSerializer = null)
        {

        }

        /// <summary>
        ///     Adds <see cref="" IHttpRequestPipelinePart"" /> which can transform request after url is constructed.
        /// </summary>
        /// <param name="" httpRequestPipelineParts""></param>
        public void AddHttpRequestPipelineParts(
            IEnumerable<IHttpRequestPipelinePart> httpRequestPipelineParts)
        {
            _requestBuilder.AddHttpRequestPipelineParts(httpRequestPipelineParts);
        }

        /// <summary>
        ///     Adds <see cref="" IActionInfoTransformer"" /> which can transform request before url is constructed.
        /// </summary>
        /// <param name="" actionInfoTransformers""></param>
        public void AddActionInfoTransformer(
            IEnumerable<IActionInfoTransformer> actionInfoTransformers)
        {
            _requestBuilder.AddActionInfoTransformers(actionInfoTransformers);
        }

        /// <summary>
        ///     Adds multiple headers using <see cref="" IActionInfoTransformer"" />.
        /// </summary>
        /// <param name="" headers""></param>
        public void AddHeaders(
            IEnumerable<KeyValuePair<string, string?>> headers)
        {
            _requestBuilder.AddHeaders(headers);
        }

        /// <summary>
        ///     Adds pipeline part which sets Authorization.
        /// </summary>
        /// <param name="" authenticationHeaderValue""></param>
        public void AddAuthenticationHeaderValue(
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            _requestBuilder.AddAuthenticationHeaderValue(authenticationHeaderValue);
        }
    }
}
