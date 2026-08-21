using System;
using System.Collections.Concurrent;
using System.Linq;
using Xunit;
using ReswPlus.SourceGenerator.ClassGenerators;
using ReswPlus.SourceGenerator.Plurals;

namespace ReswPlusUnitTests;

/// <summary>
/// Compiles the pluralization providers the generator emits, and runs them.
/// </summary>
/// <remarks>
/// The source is taken from a real run of the generator rather than by calling the emitter directly, so that
/// what these tests measure is what a project would actually compile. Deriving it here instead would leave the
/// one line that hands a provider its rules untested: every provider could be emitted with no rules at all,
/// answering <c>OTHER</c> for every quantity in every language, and nothing would say so.
/// </remarks>
internal static class PluralProviderHost
{
    /// <summary>
    /// The templates every provider depends on.
    /// </summary>
    private static readonly string[] SharedTemplates =
    [
        "Plurals.IPluralProvider",
        "Plurals.PluralTypeEnum",
        "Utils.IntExt",
        "Utils.DoubleExt"
    ];

    /// <summary>
    /// A resource declaring the forms of a pluralized string, which is what makes the generator emit plural
    /// support at all.
    /// </summary>
    private static readonly string PluralResource = ReswTestHelpers.CreateResw(
        ("FileCount_One", "one file", null),
        ("FileCount_Other", "{0} files", null));

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
            var sources = SharedTemplates.Select(PluralTemplates.Read).Concat([Emitted(id)]);

            var assembly = PluralTemplates.Compile($"ReswPlusPlurals.{id}", sources);
            var type = assembly.GetTypes().Single(candidate => candidate.Name == $"{id}Provider");
            var instance = Activator.CreateInstance(type, nonPublic: true);
            var computePlural = type.GetMethod("ComputePlural")!;

            return number => computePlural.Invoke(instance, [number])!.ToString()!;
        });
    }

    /// <summary>
    /// Runs the generator over a project written in a language of a plural form, and returns the provider it
    /// emitted for that form.
    /// </summary>
    /// <param name="providerId">The identifier of the provider, without the <c>Provider</c> suffix.</param>
    /// <returns>The source of the provider, as the generator wrote it.</returns>
    private static string Emitted(string providerId)
    {
        var form = PluralFormsRetriever.PluralFormsForTesting.FirstOrDefault(candidate => candidate.Id == providerId);

        // The fallback provider is emitted for every project, so any language reaches it.
        var language = form is null ? "en-US" : form.Languages[0];
        var run = ReswGeneratorHarness.Run([ReswGeneratorHarness.File(language, PluralResource)]);
        var hintName = $"{providerId}Provider.g.cs";

        Assert.True(
            run.Sources.ContainsKey(hintName),
            $"The generator emitted no '{hintName}' for a project written in '{language}'. It emitted: " +
            string.Join(", ", run.Sources.Keys.OrderBy(name => name, StringComparer.Ordinal)));

        return run.Sources[hintName];
    }
}
