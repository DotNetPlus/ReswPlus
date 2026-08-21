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
        foreach (var group in form.Languages.GroupBy(CldrLanguages.RulesOf, CldrLanguages.RuleComparer.Instance))
        {
            if (group.Key is null)
            {
                continue;
            }

            foreach (var quantity in CldrQuantities.Sweep)
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
            .Where(language => CldrLanguages.RulesOf(language) is null)
            .OrderBy(language => language, StringComparer.Ordinal);

        Assert.Empty(unknown);
    }

}
