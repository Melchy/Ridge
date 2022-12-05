using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Ridge.GeneratorAttributes;
using RidgeSourceGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VerifyNUnit;
using VerifyTests;

namespace RidgeSourceGeneratorTest;

public class SourceGeneratorInProcessTests
{
    [Test]
    public Task TestSourceGeneratorInProcess()
    {
        var source = """
                    using Microsoft.AspNetCore.Mvc;
                    using System.Threading.Tasks;

                    namespace TestNamespace.Controller;
                    
                    [Ridge.AddParameterToCaller(typeof(int?), "addedParameterOptional", Optional = true)]
                    [Ridge.AddParameterToCaller(typeof(int), "addedParameter", Optional = false)]
                    [Ridge.TransformParameterInCaller(fromType: typeof(int), toType: typeof(string), GeneratedParameterName = ""renamed"")]
                    [AddParameterToCaller()]
                    [Ridge.AddParameterToCaller(typeof(int))]
                    [Ridge.AddParameterToCaller(typeof(int), )]
                    [Ridge.AddParameterToCaller(typeof(int)]
                    [Ridge.AddParameterToCaller(typeof(int), "addedParameterZ", Op)]
                    [Ridge.AddParameterToCaller(typeof(int), "addedParameterX", Optional = fae)]
                    [Ridge.AddParameterToCaller(typeof(int), "addedParameterY", Optional = fae)
                    [Ridge.GenerateCaller()]
                    public class Test
                    {
                        public Test(){}
                        public Task<string> Foo(Task<string> a, bool b){ var c = 1; return Task.FromResult("asd");}
                        public Task<string> Foo1(){ var a = 1; return Task.FromResult("asd");}
                        public Task<string> Foo2(int b = 1, int c = 2){ var a = 1; return Task.FromResult("asd");}
                        public Task<IActionResult> Foo3(){return Task.FromResult("asd");}
                        public Task<ActionResult<int>> Foo4(){return Task.FromResult(1);}
                        public Foo<int> Foo5(){return new Foo<int>();}
                        public Task<Foo<int>> Foo6([FromServices] object a){return Task.FromResult(new Foo<int>());}
                        public IActionResult Foo7(){return 1;}
                        public ActionResult<int> Foo8(){return 1;}
                        public void Foo9(){}
                        public void Foo20(params int[] b){ var a = 1;}
                        public void EventFoo(string @event){}
                        public void EventFoo2(string){}
                        public void EventFoo3(sstring foo){}
                        public void EventFoo4(,,){}
                        public void EventFoo5(string a, string a){}
                        public void EventFoo6()
                        public void EventFoo7(
                        public void EventFoo8(string a
                        public void EventFoo9(string a,
                        public void EventFoo10(string,
                        public void EventFoo11(string,)
                        public void EventFoo11(string a,){}
                    }
                    public class Foo<Ttype>
                    {
                        
                    }

                    public enum FooEnum
                    {
                        a = 0,
                    }
                    """;
    
        
        return TestHelper.Verify(source);
    }
}

[SetUpFixture]
public static class SetUp
{
    [OneTimeSetUp]
    public static void SetUpVerifySourceGenerator()
    {
        VerifySourceGenerators.Enable();
    }
}

public static class TestHelper
{
    public static Task Verify(
        string source)
    {
        // Parse the provided string into a C# syntax tree
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        List<PortableExecutableReference> references = new List<PortableExecutableReference>()
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(GenerateCaller).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ActionResult).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IActionResult).Assembly.Location),
        };

        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location) ?? throw new InvalidOperationException("dll not found");

        /* 
        * Adding some necessary .NET assemblies
        * These assemblies couldn't be loaded correctly via the same construction as above,
        * in specific the System.Runtime.
        */
        references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")));

        // Create a Roslyn compilation for the syntax tree.
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[] {syntaxTree},
            references: references);

        // test code for errors
        // var emitResult = compilation.Emit(new MemoryStream());

        // Create an instance of our EnumGenerator incremental source generator
        var generator = new ControllerCallerGenerator();

        // The GeneratorDriver is used to run our generator against a compilation
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the source generator!
        driver = driver.RunGenerators(compilation);

        // Use verify to snapshot test the source generator output!
        return Verifier.Verify(driver);
    }
}
