using System;

namespace Ridge;

/// <summary>
///     Add to controller to indicate that class can be tested using strongly typed integration tests.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateStronglyTypedCallerForTesting : Attribute
{
}
