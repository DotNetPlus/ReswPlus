using System;
using System.Collections.Concurrent;
using System.Linq;

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
            var sources = SharedTemplates
                .Concat([$"Plurals.{id}Provider"])
                .Select(PluralTemplates.Read);

            var assembly = PluralTemplates.Compile($"ReswPlusPlurals.{id}", sources);
            var type = assembly.GetTypes().Single(candidate => candidate.Name == $"{id}Provider");
            var instance = Activator.CreateInstance(type, nonPublic: true);
            var computePlural = type.GetMethod("ComputePlural")!;

            return number => computePlural.Invoke(instance, [number])!.ToString()!;
        });
    }
}
