﻿//HintName: Test_TestCaller.g.cs
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Ridge source generator
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable

using Ridge.Caller;
using Ridge.LogWriter;
using Ridge.Pipeline.Public;
using Ridge.Serialization;
using Ridge.Transformers;
using Ridge.Response;
using Ridge.ActionInfo;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
namespace TestNamespace.Controller
{

/// <summary>
/// Strongly typed api client for tests.
/// </summary>
public class TestCaller
{
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogWriter? _logWriter;
    private readonly IRequestResponseSerializer? _ridgeSerializer;
    private readonly RequestBuilder _requestBuilder = new();

    /// <summary>
    ///     Create controller factory.
    /// </summary>
    /// <param name="httpClient">HttpClient used to call the server.</param>
    /// <param name="serviceProvider">ServiceProvider used to gather information about the server.</param>
    /// <param name="logWriter">
    ///     Used to log requests and responses from server.
    ///     Use <see cref="XunitLogWriter" /> or <see cref="NunitLogWriter" /> or <see cref="NunitProgressLogWriter"/> or implement custom <see cref="ILogWriter" />
    /// </param>
    /// <param name="ridgeSerializer">
    ///     Serializer used to serialize and deserialize requests.
    ///     Serializer is by default chosen based on asp.net settings. If you need custom serializer implement
    ///     <see cref="IRequestResponseSerializer" />.
     /// </param>
    public TestCaller(
        HttpClient httpClient,
        IServiceProvider serviceProvider,
        ILogWriter? logWriter = null,
        IRequestResponseSerializer? ridgeSerializer = null)
    {
        _httpClient = httpClient;
        _serviceProvider = serviceProvider;
        _logWriter = logWriter;
        _ridgeSerializer = ridgeSerializer;
    }

    /// <summary>
    ///     Add <see cref="IHttpRequestPipelinePart"/> which will be used to transform <see cref="HttpRequestMessage"/>s.
    /// </summary>
    /// <param name="httpRequestPipelineParts"><see cref="IHttpRequestPipelinePart"/> to add.</param>
    public void AddHttpRequestPipelineParts(
        params IHttpRequestPipelinePart[] httpRequestPipelineParts)
    {
        _requestBuilder.AddHttpRequestPipelineParts(httpRequestPipelineParts);
    }

    /// <summary>
    ///     Adds <see cref="IActionInfoTransformer"/> which will be later used to transform <see cref="IActionInfo"/>s.
    /// </summary>
    /// <param name="actionInfoTransformers"><see cref="IActionInfoTransformer"/>  to add.</param>
    public void AddActionInfoTransformers(
        params IActionInfoTransformer[] actionInfoTransformers)
    {
        _requestBuilder.AddActionInfoTransformers(actionInfoTransformers);
    }

    /// <summary>
    ///     Adds headers to the requests. This method actually adds <see cref="IActionInfoTransformer"/>
    ///     which then adds the header to requests.
    /// </summary>
    /// <param name="headers">Headers to add.</param>
    public void AddHeaders(params (string Key, string? Value)[] headers)
    {
        _requestBuilder.AddHeaders(headers);
    }

    /// <summary>
    ///     Adds <see cref="AuthenticationHeaderValue"/> to the requests. This method actually adds <see cref="IActionInfoTransformer"/>
    ///     which then adds the header to requests.
    /// </summary>
    /// <param name="authenticationHeaderValue"><see cref="AuthenticationHeaderValue"/> to add.</param>
    public void AddAuthenticationHeaderValue(
        AuthenticationHeaderValue authenticationHeaderValue)
    {
        _requestBuilder.AddAuthenticationHeaderValue(authenticationHeaderValue);
    }
    

    /// <summary>
    ///     Calls <see cref="TestNamespace.Controller.Test.Foo" />.
    /// </summary>
    public async Task<HttpCallResponse<string>> Call_Foo(
            System.Threading.Tasks.Task<string> @a,
            bool @b,
            int addedParameter,

            int? addedParameterOptional = default,
        IEnumerable<(string Key, string? Value)>? headers = null,
            AuthenticationHeaderValue? authenticationHeaderValue = null,
            IEnumerable<IActionInfoTransformer>? actionInfoTransformers = null,
            IEnumerable<IHttpRequestPipelinePart>? httpRequestPipelineParts = null
        )
    {
        var methodName = nameof(TestNamespace.Controller.Test.Foo);
        var controllerType = typeof(TestNamespace.Controller.Test);
        var arguments = new List<object?>()
        {
            @a,
            @b,
        };

        var requestBuilder = _requestBuilder.CreateNewBuilderByCopyingExisting();
        requestBuilder.AddHeaders(headers);
        requestBuilder.AddAuthenticationHeaderValue(authenticationHeaderValue);
        requestBuilder.AddHttpRequestPipelineParts(httpRequestPipelineParts);
        requestBuilder.AddActionInfoTransformers(actionInfoTransformers);
        var caller = new ActionCaller(requestBuilder,
            _logWriter,
            _httpClient,
            _serviceProvider,
            _ridgeSerializer);

        var methodInfo = controllerType.GetMethod(methodName, new Type[] {

        typeof(System.Threading.Tasks.Task<string>),
        typeof(bool),
        });

        if (methodInfo == null)
        {
            throw new InvalidOperationException($"Method with name {methodName} not found in class {controllerType.FullName}.");
        }
    
        return await caller.CallAction<string>(arguments, methodInfo);
    }


    /// <summary>
    ///     Calls <see cref="TestNamespace.Controller.Test.Foo1" />.
    /// </summary>
    public async Task<HttpCallResponse<string>> Call_Foo1(
            int addedParameter,

            int? addedParameterOptional = default,
        IEnumerable<(string Key, string? Value)>? headers = null,
            AuthenticationHeaderValue? authenticationHeaderValue = null,
            IEnumerable<IActionInfoTransformer>? actionInfoTransformers = null,
            IEnumerable<IHttpRequestPipelinePart>? httpRequestPipelineParts = null
        )
    {
        var methodName = nameof(TestNamespace.Controller.Test.Foo1);
        var controllerType = typeof(TestNamespace.Controller.Test);
        var arguments = new List<object?>()
        {
        };

        var requestBuilder = _requestBuilder.CreateNewBuilderByCopyingExisting();
        requestBuilder.AddHeaders(headers);
        requestBuilder.AddAuthenticationHeaderValue(authenticationHeaderValue);
        requestBuilder.AddHttpRequestPipelineParts(httpRequestPipelineParts);
        requestBuilder.AddActionInfoTransformers(actionInfoTransformers);
        var caller = new ActionCaller(requestBuilder,
            _logWriter,
            _httpClient,
            _serviceProvider,
            _ridgeSerializer);

        var methodInfo = controllerType.GetMethod(methodName, new Type[] {

        });

        if (methodInfo == null)
        {
            throw new InvalidOperationException($"Method with name {methodName} not found in class {controllerType.FullName}.");
        }
    
        return await caller.CallAction<string>(arguments, methodInfo);
    }


    /// <summary>
    ///     Calls <see cref="TestNamespace.Controller.Test.Foo2" />.
    /// </summary>
    public async Task<HttpCallResponse<string>> Call_Foo2(
            String renamed,
            String renamed1,
            int addedParameter,

            int? addedParameterOptional = default,
        IEnumerable<(string Key, string? Value)>? headers = null,
            AuthenticationHeaderValue? authenticationHeaderValue = null,
            IEnumerable<IActionInfoTransformer>? actionInfoTransformers = null,
            IEnumerable<IHttpRequestPipelinePart>? httpRequestPipelineParts = null
        )
    {
        var methodName = nameof(TestNamespace.Controller.Test.Foo2);
        var controllerType = typeof(TestNamespace.Controller.Test);
        var arguments = new List<object?>()
        {
            renamed,
            renamed,
        };

        var requestBuilder = _requestBuilder.CreateNewBuilderByCopyingExisting();
        requestBuilder.AddHeaders(headers);
        requestBuilder.AddAuthenticationHeaderValue(authenticationHeaderValue);
        requestBuilder.AddHttpRequestPipelineParts(httpRequestPipelineParts);
        requestBuilder.AddActionInfoTransformers(actionInfoTransformers);
        var caller = new ActionCaller(requestBuilder,
            _logWriter,
            _httpClient,
            _serviceProvider,
            _ridgeSerializer);

        var methodInfo = controllerType.GetMethod(methodName, new Type[] {

        typeof(int),
        typeof(int),
        });

        if (methodInfo == null)
        {
            throw new InvalidOperationException($"Method with name {methodName} not found in class {controllerType.FullName}.");
        }
    
        return await caller.CallAction<string>(arguments, methodInfo);
    }


    /// <summary>
    ///     Calls <see cref="TestNamespace.Controller.Test.Foo3" />.
    /// </summary>
    public async Task<HttpCallResponse> Call_Foo3(
            int addedParameter,

            int? addedParameterOptional = default,
        IEnumerable<(string Key, string? Value)>? headers = null,
            AuthenticationHeaderValue? authenticationHeaderValue = null,
            IEnumerable<IActionInfoTransformer>? actionInfoTransformers = null,
            IEnumerable<IHttpRequestPipelinePart>? httpRequestPipelineParts = null
        )
    {
        var methodName = nameof(TestNamespace.Controller.Test.Foo3);
        var controllerType = typeof(TestNamespace.Controller.Test);
        var arguments = new List<object?>()
        {
        };

        var requestBuilder = _requestBuilder.CreateNewBuilderByCopyingExisting();
        requestBuilder.AddHeaders(headers);
        requestBuilder.AddAuthenticationHeaderValue(authenticationHeaderValue);
        requestBuilder.AddHttpRequestPipelineParts(httpRequestPipelineParts);
        requestBuilder.AddActionInfoTransformers(actionInfoTransformers);
        var caller = new ActionCaller(requestBuilder,
            _logWriter,
            _httpClient,
            _serviceProvider,
            _ridgeSerializer);

        var methodInfo = controllerType.GetMethod(methodName, new Type[] {

        });

        if (methodInfo == null)
        {
            throw new InvalidOperationException($"Method with name {methodName} not found in class {controllerType.FullName}.");
        }
    
        return await caller.CallAction(arguments, methodInfo);
    }


    /// <summary>
    ///     Calls <see cref="TestNamespace.Controller.Test.Foo4" />.
    /// </summary>
    public async Task<HttpCallResponse<int>> Call_Foo4(
            int addedParameter,

            int? addedParameterOptional = default,
        IEnumerable<(string Key, string? Value)>? headers = null,
            AuthenticationHeaderValue? authenticationHeaderValue = null,
            IEnumerable<IActionInfoTransformer>? actionInfoTransformers = null,
            IEnumerable<IHttpRequestPipelinePart>? httpRequestPipelineParts = null
        )
    {
        var methodName = nameof(TestNamespace.Controller.Test.Foo4);
        var controllerType = typeof(TestNamespace.Controller.Test);
        var arguments = new List<object?>()
        {
        };

        var requestBuilder = _requestBuilder.CreateNewBuilderByCopyingExisting();
        requestBuilder.AddHeaders(headers);
        requestBuilder.AddAuthenticationHeaderValue(authenticationHeaderValue);
        requestBuilder.AddHttpRequestPipelineParts(httpRequestPipelineParts);
        requestBuilder.AddActionInfoTransformers(actionInfoTransformers);
        var caller = new ActionCaller(requestBuilder,
            _logWriter,
            _httpClient,
            _serviceProvider,
            _ridgeSerializer);

        var methodInfo = controllerType.GetMethod(methodName, new Type[] {

        });

        if (methodInfo == null)
        {
            throw new InvalidOperationException($"Method with name {methodName} not found in class {controllerType.FullName}.");
        }
    
        return await caller.CallAction<int>(arguments, methodInfo);
    }


    /// <summary>
    ///     Calls <see cref="TestNamespace.Controller.Test.Foo5" />.
    /// </summary>
    public async Task<HttpCallResponse<TestNamespace.Controller.Foo<int>>> Call_Foo5(
            int addedParameter,

            int? addedParameterOptional = default,
        IEnumerable<(string Key, string? Value)>? headers = null,
            AuthenticationHeaderValue? authenticationHeaderValue = null,
            IEnumerable<IActionInfoTransformer>? actionInfoTransformers = null,
            IEnumerable<IHttpRequestPipelinePart>? httpRequestPipelineParts = null
        )
    {
        var methodName = nameof(TestNamespace.Controller.Test.Foo5);
        var controllerType = typeof(TestNamespace.Controller.Test);
        var arguments = new List<object?>()
        {
        };

        var requestBuilder = _requestBuilder.CreateNewBuilderByCopyingExisting();
        requestBuilder.AddHeaders(headers);
        requestBuilder.AddAuthenticationHeaderValue(authenticationHeaderValue);
        requestBuilder.AddHttpRequestPipelineParts(httpRequestPipelineParts);
        requestBuilder.AddActionInfoTransformers(actionInfoTransformers);
        var caller = new ActionCaller(requestBuilder,
            _logWriter,
            _httpClient,
            _serviceProvider,
            _ridgeSerializer);

        var methodInfo = controllerType.GetMethod(methodName, new Type[] {

        });

        if (methodInfo == null)
        {
            throw new InvalidOperationException($"Method with name {methodName} not found in class {controllerType.FullName}.");
        }
    
        return await caller.CallAction<TestNamespace.Controller.Foo<int>>(arguments, methodInfo);
    }


    /// <summary>
    ///     Calls <see cref="TestNamespace.Controller.Test.Foo6" />.
    /// </summary>
    public async Task<HttpCallResponse<TestNamespace.Controller.Foo<int>>> Call_Foo6(
            int addedParameter,

            int? addedParameterOptional = default,
        IEnumerable<(string Key, string? Value)>? headers = null,
            AuthenticationHeaderValue? authenticationHeaderValue = null,
            IEnumerable<IActionInfoTransformer>? actionInfoTransformers = null,
            IEnumerable<IHttpRequestPipelinePart>? httpRequestPipelineParts = null
        )
    {
        var methodName = nameof(TestNamespace.Controller.Test.Foo6);
        var controllerType = typeof(TestNamespace.Controller.Test);
        var arguments = new List<object?>()
        {
        };

        var requestBuilder = _requestBuilder.CreateNewBuilderByCopyingExisting();
        requestBuilder.AddHeaders(headers);
        requestBuilder.AddAuthenticationHeaderValue(authenticationHeaderValue);
        requestBuilder.AddHttpRequestPipelineParts(httpRequestPipelineParts);
        requestBuilder.AddActionInfoTransformers(actionInfoTransformers);
        var caller = new ActionCaller(requestBuilder,
            _logWriter,
            _httpClient,
            _serviceProvider,
            _ridgeSerializer);

        var methodInfo = controllerType.GetMethod(methodName, new Type[] {

        typeof(object),
        });

        if (methodInfo == null)
        {
            throw new InvalidOperationException($"Method with name {methodName} not found in class {controllerType.FullName}.");
        }
    
        return await caller.CallAction<TestNamespace.Controller.Foo<int>>(arguments, methodInfo);
    }


    /// <summary>
    ///     Calls <see cref="TestNamespace.Controller.Test.Foo7" />.
    /// </summary>
    public async Task<HttpCallResponse> Call_Foo7(
            int addedParameter,

            int? addedParameterOptional = default,
        IEnumerable<(string Key, string? Value)>? headers = null,
            AuthenticationHeaderValue? authenticationHeaderValue = null,
            IEnumerable<IActionInfoTransformer>? actionInfoTransformers = null,
            IEnumerable<IHttpRequestPipelinePart>? httpRequestPipelineParts = null
        )
    {
        var methodName = nameof(TestNamespace.Controller.Test.Foo7);
        var controllerType = typeof(TestNamespace.Controller.Test);
        var arguments = new List<object?>()
        {
        };

        var requestBuilder = _requestBuilder.CreateNewBuilderByCopyingExisting();
        requestBuilder.AddHeaders(headers);
        requestBuilder.AddAuthenticationHeaderValue(authenticationHeaderValue);
        requestBuilder.AddHttpRequestPipelineParts(httpRequestPipelineParts);
        requestBuilder.AddActionInfoTransformers(actionInfoTransformers);
        var caller = new ActionCaller(requestBuilder,
            _logWriter,
            _httpClient,
            _serviceProvider,
            _ridgeSerializer);

        var methodInfo = controllerType.GetMethod(methodName, new Type[] {

        });

        if (methodInfo == null)
        {
            throw new InvalidOperationException($"Method with name {methodName} not found in class {controllerType.FullName}.");
        }
    
        return await caller.CallAction(arguments, methodInfo);
    }


    /// <summary>
    ///     Calls <see cref="TestNamespace.Controller.Test.Foo8" />.
    /// </summary>
    public async Task<HttpCallResponse<int>> Call_Foo8(
            int addedParameter,

            int? addedParameterOptional = default,
        IEnumerable<(string Key, string? Value)>? headers = null,
            AuthenticationHeaderValue? authenticationHeaderValue = null,
            IEnumerable<IActionInfoTransformer>? actionInfoTransformers = null,
            IEnumerable<IHttpRequestPipelinePart>? httpRequestPipelineParts = null
        )
    {
        var methodName = nameof(TestNamespace.Controller.Test.Foo8);
        var controllerType = typeof(TestNamespace.Controller.Test);
        var arguments = new List<object?>()
        {
        };

        var requestBuilder = _requestBuilder.CreateNewBuilderByCopyingExisting();
        requestBuilder.AddHeaders(headers);
        requestBuilder.AddAuthenticationHeaderValue(authenticationHeaderValue);
        requestBuilder.AddHttpRequestPipelineParts(httpRequestPipelineParts);
        requestBuilder.AddActionInfoTransformers(actionInfoTransformers);
        var caller = new ActionCaller(requestBuilder,
            _logWriter,
            _httpClient,
            _serviceProvider,
            _ridgeSerializer);

        var methodInfo = controllerType.GetMethod(methodName, new Type[] {

        });

        if (methodInfo == null)
        {
            throw new InvalidOperationException($"Method with name {methodName} not found in class {controllerType.FullName}.");
        }
    
        return await caller.CallAction<int>(arguments, methodInfo);
    }


    /// <summary>
    ///     Calls <see cref="TestNamespace.Controller.Test.Foo9" />.
    /// </summary>
    public async Task<HttpCallResponse> Call_Foo9(
            int addedParameter,

            int? addedParameterOptional = default,
        IEnumerable<(string Key, string? Value)>? headers = null,
            AuthenticationHeaderValue? authenticationHeaderValue = null,
            IEnumerable<IActionInfoTransformer>? actionInfoTransformers = null,
            IEnumerable<IHttpRequestPipelinePart>? httpRequestPipelineParts = null
        )
    {
        var methodName = nameof(TestNamespace.Controller.Test.Foo9);
        var controllerType = typeof(TestNamespace.Controller.Test);
        var arguments = new List<object?>()
        {
        };

        var requestBuilder = _requestBuilder.CreateNewBuilderByCopyingExisting();
        requestBuilder.AddHeaders(headers);
        requestBuilder.AddAuthenticationHeaderValue(authenticationHeaderValue);
        requestBuilder.AddHttpRequestPipelineParts(httpRequestPipelineParts);
        requestBuilder.AddActionInfoTransformers(actionInfoTransformers);
        var caller = new ActionCaller(requestBuilder,
            _logWriter,
            _httpClient,
            _serviceProvider,
            _ridgeSerializer);

        var methodInfo = controllerType.GetMethod(methodName, new Type[] {

        });

        if (methodInfo == null)
        {
            throw new InvalidOperationException($"Method with name {methodName} not found in class {controllerType.FullName}.");
        }
    
        return await caller.CallAction(arguments, methodInfo);
    }


    /// <summary>
    ///     Calls <see cref="TestNamespace.Controller.Test.Foo20" />.
    /// </summary>
    public async Task<HttpCallResponse> Call_Foo20(
            int[] @b,
            int addedParameter,

            int? addedParameterOptional = default,
        IEnumerable<(string Key, string? Value)>? headers = null,
            AuthenticationHeaderValue? authenticationHeaderValue = null,
            IEnumerable<IActionInfoTransformer>? actionInfoTransformers = null,
            IEnumerable<IHttpRequestPipelinePart>? httpRequestPipelineParts = null
        )
    {
        var methodName = nameof(TestNamespace.Controller.Test.Foo20);
        var controllerType = typeof(TestNamespace.Controller.Test);
        var arguments = new List<object?>()
        {
            @b,
        };

        var requestBuilder = _requestBuilder.CreateNewBuilderByCopyingExisting();
        requestBuilder.AddHeaders(headers);
        requestBuilder.AddAuthenticationHeaderValue(authenticationHeaderValue);
        requestBuilder.AddHttpRequestPipelineParts(httpRequestPipelineParts);
        requestBuilder.AddActionInfoTransformers(actionInfoTransformers);
        var caller = new ActionCaller(requestBuilder,
            _logWriter,
            _httpClient,
            _serviceProvider,
            _ridgeSerializer);

        var methodInfo = controllerType.GetMethod(methodName, new Type[] {

        typeof(int[]),
        });

        if (methodInfo == null)
        {
            throw new InvalidOperationException($"Method with name {methodName} not found in class {controllerType.FullName}.");
        }
    
        return await caller.CallAction(arguments, methodInfo);
    }


    /// <summary>
    ///     Calls <see cref="TestNamespace.Controller.Test.EventFpp" />.
    /// </summary>
    public async Task<HttpCallResponse> Call_EventFpp(
            string @event,
            int addedParameter,

            int? addedParameterOptional = default,
        IEnumerable<(string Key, string? Value)>? headers = null,
            AuthenticationHeaderValue? authenticationHeaderValue = null,
            IEnumerable<IActionInfoTransformer>? actionInfoTransformers = null,
            IEnumerable<IHttpRequestPipelinePart>? httpRequestPipelineParts = null
        )
    {
        var methodName = nameof(TestNamespace.Controller.Test.EventFpp);
        var controllerType = typeof(TestNamespace.Controller.Test);
        var arguments = new List<object?>()
        {
            @event,
        };

        var requestBuilder = _requestBuilder.CreateNewBuilderByCopyingExisting();
        requestBuilder.AddHeaders(headers);
        requestBuilder.AddAuthenticationHeaderValue(authenticationHeaderValue);
        requestBuilder.AddHttpRequestPipelineParts(httpRequestPipelineParts);
        requestBuilder.AddActionInfoTransformers(actionInfoTransformers);
        var caller = new ActionCaller(requestBuilder,
            _logWriter,
            _httpClient,
            _serviceProvider,
            _ridgeSerializer);

        var methodInfo = controllerType.GetMethod(methodName, new Type[] {

        typeof(string),
        });

        if (methodInfo == null)
        {
            throw new InvalidOperationException($"Method with name {methodName} not found in class {controllerType.FullName}.");
        }
    
        return await caller.CallAction(arguments, methodInfo);
    }

}
}