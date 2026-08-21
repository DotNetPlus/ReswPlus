using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ReswPlus.SourceGenerator.ClassGenerators;
using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Checks every language ReswPlus maps against the rules CLDR gives that language.
/// </summary>
/// <remarks>
/// <see cref="CldrConformance"/> replays the quantities CLDR publishes through one language of every plural
/// form. That catches a form whose rules were transcribed wrong, but it cannot catch two other things, and
/// both have shipped before.
/// <para>
/// The first is a quantity CLDR publishes no sample for. Its sample lists are illustrative, not exhaustive:
/// they are thin around the boundaries the rules actually turn on, so a rule can be wrong for the quantities
/// nobody wrote down. This sweeps a range wide enough to cross every boundary the rules are written in terms
/// of, and reads the rule itself rather than a list of examples.
/// </para>
/// <para>
/// The second is a language handed to the wrong form. A form can be a faithful copy of the rules of the
/// language it was pinned against and still be given to a language CLDR declines differently, and replaying
/// that form's samples would show nothing at all.
/// </para>
/// </remarks>
public class CldrDrift
{
    /// <summary>
    /// The quantities every language is checked over.
    /// </summary>
    /// <remarks>
    /// The rules are written in terms of the last digits of a quantity and the digits after its decimal point,
    /// so the sweep is built to cross every one of those boundaries rather than to be large: every integer to
    /// 1200 exhausts the hundreds and thousands the rules are written modulo, the values around a million cover
    /// the rules that single it out, and the decimal grid crosses one and two decimals against every value the
    /// rules read the decimals of a quantity as.
    /// </remarks>
    private static readonly double[] Quantities = BuildQuantities();

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
    /// The plural forms ReswPlus ships, by the identifier of the provider implementing them.
    /// </summary>
    public static TheoryData<string> Forms =>
        [.. PluralFormsRetriever.PluralFormsForTesting.Select(form => form.Id)];

    /// <summary>
    /// Checks that reading a rule and reading the quantities published with it reach the same answer.
    /// </summary>
    /// <remarks>
    /// The sweep below trusts <see cref="CldrRule"/> to say what a rule means, so a misreading of the syntax
    /// would quietly excuse the very rules it is meant to check. This is what stops that: CLDR's own samples
    /// state the answer for thousands of quantities, and nothing about them depends on reading the syntax.
    /// </remarks>
    [Theory]
    [MemberData(nameof(Languages))]
    public void ReadingARuleAgreesWithTheQuantitiesPublishedWithIt(string language)
    {
        var rules = CldrPublishedRules.Cardinal[language];
        var mistakes = new List<string>();

        foreach (var (category, published) in CldrPluralRules.Cardinal[language])
        {
            foreach (var (quantity, literal) in CldrSamples.Read(published))
            {
                var selected = CldrRule.Select(rules, quantity);

                if (!string.Equals(selected, category, StringComparison.Ordinal))
                {
                    mistakes.Add($"    {literal}: published under '{category}', read as '{selected}'");
                }
            }
        }

        Assert.True(
            mistakes.Count == 0,
            $"Reading the rules of '{language}' disagrees with the quantities CLDR publishes for them:"
                + $"{Environment.NewLine}{string.Join(Environment.NewLine, mistakes)}");
    }

    /// <summary>
    /// The languages the rules of a plural form are pinned for.
    /// </summary>
    public static TheoryData<string> Languages => [.. CldrPluralRules.Cardinal.Keys];

    /// <summary>
    /// Checks that the two pinned copies of CLDR's rules say the same thing.
    /// </summary>
    /// <remarks>
    /// The rules are pinned twice, with the sample lists in <see cref="CldrPluralRules"/> and without them in
    /// <see cref="CldrPublishedRules"/>, and one can be refreshed from a new CLDR release without the other.
    /// </remarks>
    [Theory]
    [MemberData(nameof(Languages))]
    public void TheTwoPinnedCopiesOfTheRulesOfALanguageAgree(string language)
    {
        var withSamples = CldrPluralRules.Cardinal[language]
            .Select(rule => (rule.Key, Condition: rule.Value.Split('@')[0].Trim()));

        var withoutSamples = CldrPublishedRules.Cardinal[language]
            .ToDictionary(rule => rule.Category, rule => rule.Condition, StringComparer.Ordinal);

        foreach (var (category, condition) in withSamples)
        {
            Assert.True(
                withoutSamples.TryGetValue(category, out var pinned),
                $"'{language}' has a '{category}' rule in {nameof(CldrPluralRules)} and none in {nameof(CldrPublishedRules)}.");

            Assert.Equal(condition, pinned);
        }
    }

    /// <summary>
    /// Checks that every language a plural form is given selects what CLDR says it should.
    /// </summary>
    [Theory]
    [MemberData(nameof(Forms))]
    public void EveryLanguageOfAFormSelectsWhatCldrSelects(string formId)
    {
        var form = PluralFormsRetriever.PluralFormsForTesting.Single(candidate => candidate.Id == formId);
        var provider = PluralProviderHost.GetProvider(formId);
        var mistakes = new List<string>();

        // Languages CLDR declines identically are checked once: the sweep is the same work for each of them,
        // and a form is given up to ninety languages.
        foreach (var group in form.Languages.GroupBy(RulesOf, RuleComparer.Instance))
        {
            if (group.Key is null)
            {
                continue;
            }

            foreach (var quantity in Quantities)
            {
                var selected = provider(quantity);
                var published = CldrRule.Select(group.Key, quantity);

                if (!string.Equals(selected, published, StringComparison.Ordinal))
                {
                    mistakes.Add(
                        $"    {quantity.ToString("R", CultureInfo.InvariantCulture)} in "
                            + $"{string.Join(", ", group.OrderBy(language => language, StringComparer.Ordinal))}: "
                            + $"CLDR selects '{published}', {formId}Provider selects '{selected}'");
                    break;
                }
            }
        }

        Assert.True(
            mistakes.Count == 0,
            $"The '{formId}' plural form disagrees with CLDR {CldrPluralRules.Version} for languages it is given:"
                + $"{Environment.NewLine}{string.Join(Environment.NewLine, mistakes)}");
    }

    /// <summary>
    /// Checks that CLDR still publishes rules for every language ReswPlus maps.
    /// </summary>
    /// <remarks>
    /// A language CLDR has no rules for is one this test silently stops checking, so it is worth failing over
    /// rather than skipping: either the code is one CLDR renamed, and belongs in <see cref="DeprecatedCodes"/>,
    /// or it is one nothing can say the plural rules of.
    /// </remarks>
    [Fact]
    public void EveryLanguageMappedIsOneCldrPublishesRulesFor()
    {
        var unknown = PluralFormsRetriever.PluralFormsForTesting
            .SelectMany(form => form.Languages)
            .Where(language => RulesOf(language) is null)
            .OrderBy(language => language, StringComparer.Ordinal);

        Assert.Empty(unknown);
    }

    /// <summary>
    /// Gets the rules CLDR publishes for a language, under its current code.
    /// </summary>
    /// <param name="language">The language, as ReswPlus maps it.</param>
    /// <returns>The rules, or <see langword="null"/> when CLDR publishes none.</returns>
    private static IReadOnlyList<CldrPublishedRules.Rule>? RulesOf(string language)
    {
        var code = DeprecatedCodes.TryGetValue(language, out var current) ? current : language;

        return CldrPublishedRules.Cardinal.TryGetValue(code, out var rules) ? rules : null;
    }

    private static double[] BuildQuantities()
    {
        var quantities = new List<double>();

        for (var value = 0; value <= 1200; value++)
        {
            quantities.Add(value);
        }

        quantities.AddRange([9999, 10000, 100000, 1000000, 1000001, 1100000, 2000000, 123456]);

        for (var whole = 0; whole <= 30; whole++)
        {
            // Built from the text of the quantity rather than by arithmetic: 4 + 94/100d is not the double
            // nearest 4.94, and a rule reads the decimals of a quantity off the shortest text that round trips
            // to it, so arithmetic would sweep quantities carrying sixteen decimals instead of two.
            for (var hundredths = 1; hundredths <= 99; hundredths++)
            {
                quantities.Add(Parse($"{whole}.{hundredths:00}"));
            }

            for (var tenths = 1; tenths <= 9; tenths++)
            {
                quantities.Add(Parse($"{whole}.{tenths}"));
            }
        }

        return [.. quantities];

        static double Parse(string literal) => double.Parse(literal, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Compares two sets of rules by what they say.
    /// </summary>
    private sealed class RuleComparer : IEqualityComparer<IReadOnlyList<CldrPublishedRules.Rule>?>
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
