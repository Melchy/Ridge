﻿using System.Text;

namespace RidgeSourceGenerator;

public static class ControllerGenerationHelper
{
    private const string Header = @"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Ridge source generator
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
";

    public static string GenerateExtensionClass(
        StringBuilder sb,
        ControllerToGenerate controllerToGenerate,
        IEnumerable<MethodToGenerate> generatedMethods,
        CancellationToken cancellationToken)
    {
        var className = $"{controllerToGenerate.Name}Caller";

        sb.Append(Header);
        sb.Append(@"
#nullable enable
#pragma warning disable CS0419

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
");
        if (!string.IsNullOrEmpty(controllerToGenerate.Namespace))
        {
            sb.Append(@"namespace ").AppendLine(controllerToGenerate.Namespace);
            sb.AppendLine("{");
        }

        cancellationToken.ThrowIfCancellationRequested();

        sb.Append(@"
/// <summary>
/// Generated Api client for tests. Calls <see cref=""");
        sb.Append(controllerToGenerate.Namespace);
        sb.Append(".");
        sb.Append(controllerToGenerate.Name);
        sb.AppendLine("\" />");
        sb.Append(@"/// </summary>
public partial class ");
        sb.Append(className);
        sb.Append(" : IControllerCaller");
        sb.Append(@"
{
    ");

        foreach (var methodToGenerate in generatedMethods)
        {
            sb.Append(methodToGenerate.GenerateMethod(cancellationToken));
        }

        sb.AppendLine("}");

        if (!string.IsNullOrEmpty(controllerToGenerate.Namespace))
        {
            sb.AppendLine("}");
        }

        cancellationToken.ThrowIfCancellationRequested();
        sb.AppendLine("#pragma warning restore CS0419");
        return sb.ToString();
    }
}