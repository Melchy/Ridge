using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Ridge;
using RidgeSourceGenerator;

namespace RidgeSourceGeneratorTest;

public class Tests
{
    // TODO rename test
    // TODO fix formatovani vygenerovaneho kodu
    [Test]
    public Task Test1()
    {
        var source = @"
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace TestNamespace.Controller;
[Ridge.GenerateStronglyTypedCallerForTesting]
public class Test
{
    public Test(){}
    public Task<string> Foo(Task<string> a, bool b){ var c = 1; return Task.FromResult(""asd"");}
    public Task<string> Foo1(){ var a = 1; return Task.FromResult(""asd"");}
    public Task<string> Foo2(int b = 1){ var a = 1; return Task.FromResult(""asd"");}
    public Task<IActionResult> Foo3(){return Task.FromResult(""asd"");}
    public Task<ActionResult<int>> Foo4(){return Task.FromResult(1);}
    public Foo<int> Foo5(){return new Foo<int>();}
    public Task<Foo<int>> Foo6(){return Task.FromResult(new Foo<int>());}
    public IActionResult Foo7(){return 1;}
    public ActionResult<int> Foo8(){return 1;}
    public void Foo9(){}
    public void Foo20(params int[] b){ var a = 1;}
}
public class Foo<Ttype>
    {
        
    }
";
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
            MetadataReference.CreateFromFile(typeof(GenerateTestClassForThisControllerAttribute).Assembly.Location),
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
        var generator = new ControllerGenerator();

        // The GeneratorDriver is used to run our generator against a compilation
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the source generator!
        driver = driver.RunGenerators(compilation);

        // Use verify to snapshot test the source generator output!
        return Verifier.Verify(driver);
    }
}
