using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Runs the code the generator writes, rather than only compiling it.
/// </summary>
/// <remarks>
/// Everything else asserts that the generated sources build. That catches a member that does not compile, and
/// nothing about a member that compiles and is wrong: a format whose arguments are handed over in the wrong
/// order, or a macro written with the wrong specifier, reads perfectly well and ships.
/// </remarks>
public class GeneratedCodeExecution
{
    private static string Resources => ReswTestHelpers.CreateResw(
        ("Plain", "A plain string", null),
        ("Formatted", "Hello {0}, you are {1}", "#Format[String name, Int age]"),
        ("Literal", "{0} - {1}", "#Format[String name, \"a literal\"]"),
        ("FileCount_One", "one file", null),
        ("FileCount_Other", "{0} files", "#Format[Int count]"));

    [Fact]
    public void APlainResourceReadsItsValue()
    {
        var strings = Generated();

        Assert.Equal("A plain string", Read(strings, "Plain"));
    }

    [Fact]
    public void AFormattedResourceIsGivenItsArgumentsInTheOrderItDeclaresThem()
    {
        var strings = Generated();

        // The order matters and is invisible to a compiler: 'Hello {0}, you are {1}' with (name, age) swapped
        // compiles just as well and reads "Hello 30, you are Alice".
        Assert.Equal("Hello Alice, you are 30", Invoke(strings, "Formatted", "Alice", 30));
    }

    [Fact]
    public void ALiteralDeclaredInAFormatIsPassedThrough()
    {
        var strings = Generated();

        Assert.Equal("Alice - a literal", Invoke(strings, "Literal", "Alice"));
    }

    /// <summary>
    /// Generates the code of a project holding <see cref="Resources"/>, compiles it, and seeds the resources
    /// the generated code will look up.
    /// </summary>
    /// <returns>The generated strongly typed class.</returns>
    private static Type Generated()
    {
        var run = ReswGeneratorHarness.Run([ReswGeneratorHarness.File("en-US", Resources)]);
        var assembly = run.LoadAssembly();

        // The resource loader lives in the stub the generated code was compiled against, not in the generated
        // assembly itself.
        var stub = Assembly.Load("ReswPlusTests.WindowsAppSdkStub");
        var loader = stub.GetType("Microsoft.Windows.ApplicationModel.Resources.ResourceLoader")!;
        var values = (Dictionary<string, string>)loader.GetField("Values", BindingFlags.Public | BindingFlags.Static)!
            .GetValue(null)!;

        values.Clear();

        foreach (var (key, value) in ReadDeclaredValues())
        {
            values[key] = value;
        }

        return assembly.GetTypes().Single(type => type.Name == "Resources");
    }

    /// <summary>
    /// The values the resource file declares, which the generated code looks up by key.
    /// </summary>
    private static IEnumerable<(string Key, string Value)> ReadDeclaredValues()
    {
        yield return ("Plain", "A plain string");
        yield return ("Formatted", "Hello {0}, you are {1}");
        yield return ("Literal", "{0} - {1}");
        yield return ("FileCount_One", "one file");
        yield return ("FileCount_Other", "{0} files");
    }

    private static string Read(Type strings, string name)
    {
        return (string)strings.GetProperty(name, BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
    }

    private static string Invoke(Type strings, string name, params object[] arguments)
    {
        var method = strings.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(candidate => candidate.Name == name && candidate.GetParameters().Length == arguments.Length);

        return (string)method.Invoke(null, arguments)!;
    }
}
