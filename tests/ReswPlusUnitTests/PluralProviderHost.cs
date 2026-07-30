using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace ReswPlusUnitTests;

/// <summary>
/// Compiles the pluralization templates shipped by the generator and runs them.
/// </summary>
/// <remarks>
/// The plural providers are emitted verbatim into the consumer's compilation, so they are embedded resources
/// rather than code that the test project can reference. Compiling them here is what makes it possible to test
/// the rules that actually ship, instead of a copy of them that could drift.
/// </remarks>
internal static class PluralProviderHost
{
    private const string TemplateNamespace = "ReswPlus.SourceGenerator.Templates";

    /// <summary>
    /// The templates every provider depends on.
    /// </summary>
    private static readonly string[] SharedTemplates =
    [
        $"{TemplateNamespace}.Plurals.IPluralProvider.txt",
        $"{TemplateNamespace}.Plurals.PluralTypeEnum.txt",
        $"{TemplateNamespace}.Utils.IntExt.txt",
        $"{TemplateNamespace}.Utils.DoubleExt.txt"
    ];

    private static readonly ConcurrentDictionary<string, Func<double, string>> Providers = new();

    /// <summary>
    /// Gets the plural rules of a provider, as a function from a quantity to a CLDR plural category.
    /// </summary>
    /// <param name="providerId">The identifier of the provider, without the <c>Provider</c> suffix.</param>
    /// <returns>A function returning the name of the plural category selected for a quantity.</returns>
    public static Func<double, string> GetProvider(string providerId)
    {
        return Providers.GetOrAdd(providerId, static id =>
        {
            var type = Compile(id).GetTypes().Single(candidate => candidate.Name == $"{id}Provider");
            var instance = Activator.CreateInstance(type, nonPublic: true);
            var computePlural = type.GetMethod("ComputePlural")!;

            return number => computePlural.Invoke(instance, [number])!.ToString()!;
        });
    }

    /// <summary>
    /// Compiles a provider template, together with the templates it depends on, into an in-memory assembly.
    /// </summary>
    private static Assembly Compile(string providerId)
    {
        var sources = SharedTemplates
            .Concat([$"{TemplateNamespace}.Plurals.{providerId}Provider.txt"])
            .Select(resourceName => CSharpSyntaxTree.ParseText(ReadTemplate(resourceName), path: resourceName));

        var compilation = CSharpCompilation.Create(
            $"ReswPlusPlurals.{providerId}",
            sources,
            GetRuntimeReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var peStream = new MemoryStream();

        var result = compilation.Emit(peStream);

        if (!result.Success)
        {
            var errors = result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

            throw new InvalidOperationException(
                $"The '{providerId}' plural provider template does not compile:{Environment.NewLine}" +
                string.Join(Environment.NewLine, errors));
        }

        return Assembly.Load(peStream.ToArray());
    }

    /// <summary>
    /// Reads a template out of the embedded resources of the generator.
    /// </summary>
    private static string ReadTemplate(string resourceName)
    {
        var assembly = typeof(ReswPlus.SourceGenerator.ReswSourceGenerator).Assembly;

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"The generator doesn't embed a '{resourceName}' template.");

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    private static IEnumerable<MetadataReference> GetRuntimeReferences()
    {
        var trustedAssemblies = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trustedAssemblies
            .Split(Path.PathSeparator)
            .Where(path => path.Length > 0)
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path));
    }
}
