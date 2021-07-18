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
using System.Threading.Tasks;
using TestWebApplication;
using TestWebApplication.Controllers.Examples;

namespace RidgeExamples
{
    public class ExampleTests
    {
        //...
        [Test]
        public async Task ExampleTest()
        {
            // Create webApplicationFactory
            // https://docs.microsoft.com/cs-cz/aspnet/core/test/integration-tests?view=aspnetcore-5.0
            var webApplicationFactory = new WebApplicationFactory<Startup>();
            var client = webApplicationFactory.CreateClient();
            // Create controller factory using ridge package
            var controllerFactory = new ControllerFactory(client, webApplicationFactory.Services, new NunitLogWriter());


            // Create instance of controller using controllerFactory.
            // This is where the magic happens. Ridge replaces controller implementation
            // with custom code which transforms method calls to http calls.
            var testController = controllerFactory.CreateController<ExamplesController>();
            // Make standard method call which will be transformed into Http call.
            var response = testController.ReturnGivenNumber(10);
            // Equivalent call using WebAppFactory would look like this:
            // var result = await client.GetFromJsonAsync<int>("/Test/ReturnGivenNumber?input=10");


            //Assert httpResponseMessage
            var httpResponseMessage = response.HttpResponseMessage();
            var content = await httpResponseMessage.Content.ReadAsStringAsync();
            Assert.AreEqual(10, int.Parse(content));
            Assert.True(httpResponseMessage.IsSuccessStatusCode);

            //You can use our extension methods to simplify assertion.


            //Instead of Assert.True(response.HttpResponseMessage.IsSuccessStatusCode) Use:
            Assert.True(response.IsSuccessStatusCode());
            // Instead of
            // var content = await httpResponseMessage.Content.ReadAsStringAsync();
            // Assert.AreEqual(10, int.Parse(content));
            // Use:
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
                    new ComplexObject()
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
