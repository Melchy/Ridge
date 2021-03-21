using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
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
            // Create webAppFactory
            // https://docs.microsoft.com/cs-cz/aspnet/core/test/integration-tests?view=aspnetcore-5.0
            var webAppFactory = new WebApplicationFactory<Startup>();
            var client = webAppFactory.CreateClient();
            // Create controller factory
            var controllerFactory = new ControllerFactory(client, webAppFactory.Services);
            // Create instance of controller using controllerFactory
            var testController = controllerFactory.CreateController<ExamplesController>();
            // Make standard method call which will be transformed into Http call.
            var response = await testController.ReturnGivenNumber(10);
            Assert.AreEqual(10, response.Result);
            // Equivalent call would look like this:
            // var result = await client.GetFromJsonAsync<int>("/Test/ReturnGivenNumber?input=10");
        }

        [Test]
        public async Task ComplexTest()
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
            var response = await testController.ComplexExample(
                complexObjectFromQuery: new ComplexObject()
                {
                    Str = "str",
                    NestedComplexObject = new NestedComplexObject()
                    {
                        Integer = 1,
                        Str = "string"
                    },
                },
                listOfSimpleTypesFromQuery:new List<string>()
                {
                    "foo", "bar"
                },
                complexObjectsFromBody:new List<ComplexObject>()
                {
                    new ComplexObject()
                    {
                        Str = "str",
                        NestedComplexObject = new NestedComplexObject()
                        {
                            Integer = 5,
                            Str = "bar"
                        }
                    }
                },
                fromRoute: 1,
                // this value is used only because we added CustomModelBinderTransformer
                customModelBinder: "customModelBinder",
                examplesController:null); // this value wont be used

            Assert.AreEqual("str", response.Result.ComplexObjectFromQuery.Str);
            Assert.AreEqual("string", response.Result.ComplexObjectFromQuery.NestedComplexObject.Str);
            Assert.AreEqual("foo", response.Result.ListOfSimpleTypesFromQuery.First());
            Assert.AreEqual(5, response.Result.ComplexObjectsFromBody.First().NestedComplexObject.Integer);
            Assert.AreEqual(1, response.Result.FromRoute);
            Assert.AreEqual("customModelBinder", response.Result.CustomModelBinder);
        }
    }

    public class CustomModelBinderTransformer : IActionInfoTransformer
    {
        public Task TransformAsync(
            IActionInfo actionInfo, //action info contains information about request
            InvocationInfo invocationInfo)
        {
            // using ElementAt is not recommended.
            // Better way would be to wrap customModelBinder argument in custom class and search for that class
            actionInfo.RouteParams.Add("boundFromCustomModelBinder", invocationInfo.Arguments.ElementAt(4));
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
            httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
            var response = await next();
            // we could even transform response
            //response.Content = new StringContent("foo");
            return response;
        }
    }
}
