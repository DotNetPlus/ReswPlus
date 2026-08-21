using System.Collections.Generic;
using System.Linq;

namespace ReswPlusUnitTests;

/// <summary>
/// Finds the rules CLDR publishes for a language ReswPlus maps.
/// </summary>
internal static class CldrLanguages
{
    /// <summary>
    /// The languages CLDR publishes rules for under a name ReswPlus does not use for them.
    /// </summary>
    /// <remarks>
    /// ReswPlus maps the deprecated ISO 639 codes as well as the current ones, because a resource folder or a
    /// Windows display language can still be named with them. CLDR publishes its rules under the current code
    /// only, so they are looked up under it.
    /// </remarks>
    private static readonly IReadOnlyDictionary<string, string> DeprecatedCodes = new Dictionary<string, string>
    {
        ["bh"] = "bho",
        ["in"] = "id",
        ["iw"] = "he",
        ["ji"] = "yi",
        ["jw"] = "jv",
        ["mo"] = "ro",
        ["sh"] = "sr",
        ["tl"] = "fil",
    };

    /// <summary>
    /// Gets the rules CLDR publishes for a language, under its current code.
    /// </summary>
    /// <param name="language">The language, as ReswPlus maps it.</param>
    /// <returns>The rules, or <see langword="null"/> when CLDR publishes none.</returns>
    public static IReadOnlyList<CldrPublishedRules.Rule>? RulesOf(string language)
    {
        var code = DeprecatedCodes.TryGetValue(language, out var current) ? current : language;

        return CldrPublishedRules.Cardinal.TryGetValue(code, out var rules) ? rules : null;
    }

    /// <summary>
    /// Compares two sets of rules by what they say.
    /// </summary>
    public sealed class RuleComparer : IEqualityComparer<IReadOnlyList<CldrPublishedRules.Rule>?>
    {
        public static readonly RuleComparer Instance = new();

        public bool Equals(IReadOnlyList<CldrPublishedRules.Rule>? x, IReadOnlyList<CldrPublishedRules.Rule>? y)
        {
            return ReferenceEquals(x, y) || (x is not null && y is not null && x.SequenceEqual(y));
        }

        public int GetHashCode(IReadOnlyList<CldrPublishedRules.Rule>? rules)
        {
            var hash = 17;

            foreach (var rule in rules ?? [])
            {
                hash = (hash * 31) + rule.GetHashCode();
            }

            return hash;
        }
    }
}
