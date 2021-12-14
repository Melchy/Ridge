using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Ridge.CallResult.Controller.Extensions;
using Ridge.Interceptor;
using Ridge.Interceptor.InterceptorFactory;
using Ridge.LogWriter;
using Ridge.Pipeline.Public;
using Ridge.Transformers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TestWebApplication;
using TestWebApplication.Controllers.Examples;

namespace RidgeExamples
{
    public class ExampleTests
    {
        [Test]
        public async Task TestUsingWebApplicationFactory()
        {
            var webApplicationFactory = new WebApplicationFactory<Startup>();
            var client = webApplicationFactory.CreateClient();

            var result = await client.GetFromJsonAsync<int>("/ReturnGivenNumber?input=10");

            Assert.AreEqual(10, result);
        }


        [Test]
        public void TestUsingRidge()
        {
            var webApplicationFactory = new WebApplicationFactory<Startup>();
            var client = webApplicationFactory.CreateClient();
            var controllerFactory = new ControllerFactory(
                client,
                webApplicationFactory.Services,
                new NunitLogWriter());

            var testController = controllerFactory.CreateController<ExamplesController>();
            // Ridge transforms method call to httpRequest
            var response = testController.ReturnGivenNumber(10);

            Assert.True(response.IsSuccessStatusCode());
            Assert.AreEqual(10, response.GetResult());
        }


        [Test]
        public async Task ThrowExceptionTest()
        {
            var webApplicationFactory = new WebApplicationFactory<Startup>();
            var client = webApplicationFactory.CreateClient();
            var controllerFactory = new ControllerFactory(client, webApplicationFactory.Services);

            var testController = controllerFactory.CreateController<ExamplesController>();
            try
            {
                _ = testController.ThrowException();
            }
            catch (InvalidOperationException e)
            {
                Assert.AreEqual("Exception throw", e.Message);
            }
        }

        [Test]
        public void ComplexTest()
        {
            var webAppFactory = new WebApplicationFactory<Startup>();
            var client = webAppFactory.CreateClient();
            var controllerFactory = new ControllerFactory(client,
                webAppFactory.Services,
                new NunitLogWriter()); // add writer which writes generated requests to test output.
            // Register transformer which allows us to work with custom model binder
            controllerFactory.AddActionInfoTransformer(new CustomModelBinderTransformer());
            // add httpRequestTransformation which allows us to transform final http request
            controllerFactory.AddHttpRequestPipelinePart(new HttpRequestTransformationPipelinePart());
            var testController = controllerFactory.CreateController<ExamplesController>();
            var response = testController.ComplexExample(
                complexObjectFromQuery: new ComplexObject()
                {
                    Str = "str",
                    NestedComplexObject = new NestedComplexObject()
                    {
                        Integer = 1,
                        Str = "string",
                    },
                },
                listOfSimpleTypesFromQuery: new List<string>()
                {
                    "foo", "bar",
                },
                complexObjectsFromBody: new List<ComplexObject>()
                {
                    new()
                    {
                        Str = "str",
                        NestedComplexObject = new NestedComplexObject()
                        {
                            Integer = 5,
                            Str = "bar",
                        },
                    },
                },
                fromRoute: 1,
                examplesController: null); // this value won`t be used

            Assert.AreEqual("str", response.GetResult().ComplexObjectFromQuery.Str);
            Assert.AreEqual("string", response.GetResult().ComplexObjectFromQuery.NestedComplexObject.Str);
            Assert.AreEqual("foo", response.GetResult().ListOfSimpleTypesFromQuery.First());
            Assert.AreEqual(5, response.GetResult().ComplexObjectsFromBody.First().NestedComplexObject.Integer);
            Assert.AreEqual(1, response.GetResult().FromRoute);
        }


        [Test]
        public void CustomModelBinderTest()
        {
            var webAppFactory = new WebApplicationFactory<Startup>();
            var client = webAppFactory.CreateClient();
            var controllerFactory = new ControllerFactory(client, webAppFactory.Services);
            // Register action transformer which allows us to work with custom model binder
            controllerFactory.AddActionInfoTransformer(new CustomModelBinderTransformer());
            var testController = controllerFactory.CreateController<ExamplesController>();
            var response = testController.CustomModelBinderExample("exampleValue");

            Assert.AreEqual("exampleValue", response.GetResult());
        }

        [Test]
        public void HttpRequestPipelineTest()
        {
            var webAppFactory = new WebApplicationFactory<Startup>();
            var client = webAppFactory.CreateClient();
            var controllerFactory = new ControllerFactory(client, webAppFactory.Services);

            controllerFactory.AddHttpRequestPipelinePart(new HttpRequestTransformationPipelinePart());

            var testController = controllerFactory.CreateController<ExamplesController>();
            var response = testController.CallThatNeedsHeaders();

            Assert.True(response.IsSuccessStatusCode());
        }
    }

    public class CustomModelBinderTransformer : IActionInfoTransformer
    {
        public Task TransformAsync(
            IActionInfo actionInfo, // IActionInfo contains information about request
            InvocationInfo invocationInfo) // invocation info contains information about method that was called
        {
            // set route parameter "thisIsBoundedUsingCustomBinder" to value of first argument passed to method
            actionInfo.RouteParams.Add("thisIsBoundedUsingCustomBinder", invocationInfo.Arguments.First());
            return Task.CompletedTask;
        }
    }

    public class HttpRequestTransformationPipelinePart : IHttpRequestPipelinePart
    {
        public async Task<HttpResponseMessage> InvokeAsync(
            Func<Task<HttpResponseMessage>> next,
            HttpRequestMessage httpRequestMessage,
            IReadOnlyActionInfo actionInfo,
            InvocationInfo invocationInfo)
        {
            // transform http request
            httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await next();
            // we could even transform response
            //response.Content = new StringContent("foo");
            return response;
        }
    }
}
