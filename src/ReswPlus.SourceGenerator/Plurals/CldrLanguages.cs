using System.Collections.Generic;
using System.Linq;

namespace ReswPlus.SourceGenerator.Plurals;

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
    /// Gets the rules a plural form implements.
    /// </summary>
    /// <param name="languages">The languages the form is given.</param>
    /// <returns>The rules of the first of them CLDR publishes rules for.</returns>
    /// <remarks>
    /// A form is given several languages, and CLDR may write their rules differently while meaning the same
    /// thing -- Catalan reads 'i = 1 and v = 0' where Spanish reads 'n = 1', which pick out the same quantities
    /// once a quantity is a <see cref="double"/>. Any of them can therefore stand for the form, and the tests
    /// check every language against the rules of its own language rather than trusting that.
    /// </remarks>
    public static IReadOnlyList<CldrPublishedRules.Rule> RulesOfForm(IEnumerable<string> languages)
    {
        return languages.Select(RulesOf).FirstOrDefault(rules => rules is not null) ?? [];
    }

    /// <summary>
    /// Gets the language whose rules stand for a plural form.
    /// </summary>
    /// <param name="languages">The languages the form is given.</param>
    /// <returns>The first of them CLDR publishes rules for, or <see langword="null"/> when it publishes none.</returns>
    /// <remarks>
    /// This is the language <see cref="RulesOfForm"/> read the rules of, and it is named in the generated code
    /// so that a reader can find the rules it was written from.
    /// </remarks>
    public static string? LanguageOfForm(IEnumerable<string> languages)
    {
        return languages.FirstOrDefault(language => RulesOf(language) is not null);
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
