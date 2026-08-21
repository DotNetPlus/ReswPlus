using System;
using System.Collections.Concurrent;
using System.Linq;
using ReswPlus.SourceGenerator.ClassGenerators;
using ReswPlus.SourceGenerator.Plurals;

namespace ReswPlusUnitTests;

/// <summary>
/// Writes the pluralization providers the generator writes, compiles them, and runs them.
/// </summary>
/// <remarks>
/// The providers are written from CLDR's rules into the consumer's compilation rather than kept as files, so
/// there is nothing to read: the tests ask for the same source the generator would emit, and compile it the
/// same way. Compiling it here is what makes it possible to test the rules that actually ship, instead of a
/// copy of them that could drift.
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
            var form = PluralFormsRetriever.PluralFormsForTesting.FirstOrDefault(candidate => candidate.Id == id);
            var rules = form is null ? [] : CldrLanguages.RulesOfForm(form.Languages);
            var sources = SharedTemplates.Select(PluralTemplates.Read).Concat([CldrEmitter.Emit($"{id}Provider", rules)]);

            var assembly = PluralTemplates.Compile($"ReswPlusPlurals.{id}", sources);
            var type = assembly.GetTypes().Single(candidate => candidate.Name == $"{id}Provider");
            var instance = Activator.CreateInstance(type, nonPublic: true);
            var computePlural = type.GetMethod("ComputePlural")!;

            return number => computePlural.Invoke(instance, [number])!.ToString()!;
        });
    }
}
