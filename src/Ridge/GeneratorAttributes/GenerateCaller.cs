using Ridge.Response;
using System;
using System.Net.Http;

namespace Ridge.GeneratorAttributes;

/// <summary>
///     Add to controller to indicate that class can be tested using strongly typed integration tests.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateCaller : Attribute
{
    /// <summary>
    ///     When this value is true then the generated callers return <see cref="HttpResponseMessage" />.
    ///     When false then the generated classes returns <see cref="HttpCallResponse{TResult}" /> or
    ///     <see cref="HttpCallResponse" />.
    /// </summary>
    public bool UseHttpResponseMessageAsReturnType { get; set; }
}
