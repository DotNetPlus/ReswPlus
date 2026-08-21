using System;
using System.Collections.Generic;
using System.Linq;
using ReswPlus.SourceGenerator.ClassGenerators;
using ReswPlus.SourceGenerator.Plurals;
using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Checks the plural rules ReswPlus ships against the ones Unicode CLDR publishes.
/// </summary>
/// <remarks>
/// The providers are written from CLDR's conditions, and this checks that reading against the other half of
/// what CLDR publishes: the quantities it states select each category. Those samples are data rather than a
/// second reading of the syntax, so they catch a rule that was misread in a way both the code that emits a
/// provider and the code that checks it would otherwise agree on.
/// <para>
/// Every language a plural form is given is replayed, not one per form, so a language handed the rules of a
/// language CLDR declines differently fails here too.
/// </para>
/// </remarks>
public class CldrConformance
{
    /// <summary>
    /// Every language ReswPlus maps to a plural form, and that CLDR publishes rules for.
    /// </summary>
    public static TheoryData<string> Languages =>
        [.. PluralFormsRetriever.PluralFormsForTesting
            .SelectMany(form => form.Languages)
            .Where(language => CldrLanguages.RulesOf(language) is not null)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(language => language, StringComparer.Ordinal)];

    [Theory]
    [MemberData(nameof(Languages))]
    public void TheProviderOfALanguageSelectsTheCategoryCldrSelects(string language)
    {
        var form = PluralFormsRetriever.RetrievePluralFormForLanguage(language);

        Assert.NotNull(form);

        var provider = PluralProviderHost.GetProvider(form!.Id);
        var mistakes = new List<string>();

        foreach (var rule in CldrLanguages.RulesOf(language)!)
        {
            foreach (var (quantity, literal) in CldrSamples.Read(rule.Published))
            {
                var selected = provider(quantity);

                if (!string.Equals(selected, rule.Category, StringComparison.Ordinal))
                {
                    mistakes.Add($"    {literal}: CLDR selects '{rule.Category}', {form.Id}Provider selects '{selected}'");
                }
            }
        }

        Assert.True(
            mistakes.Count == 0,
            $"The '{form.Id}' plural form disagrees with CLDR {CldrPublishedRules.Version} for '{language}':" +
            $"{Environment.NewLine}{string.Join(Environment.NewLine, mistakes)}");
    }

    [Theory]
    [MemberData(nameof(Languages))]
    public void TheCategoriesOfALanguageAreTheOnesCldrDeclares(string language)
    {
        var form = PluralFormsRetriever.RetrievePluralFormForLanguage(language);

        Assert.NotNull(form);

        var declared = form!.Categories.Select(category => category.ToString().ToUpperInvariant()).OrderBy(name => name, StringComparer.Ordinal);
        var published = CldrLanguages.RulesOf(language)!.Select(rule => rule.Category).OrderBy(name => name, StringComparer.Ordinal);

        // A category ReswPlus declares but CLDR doesn't is a form the diagnostics ask translators to write and
        // the runtime never looks up. One CLDR declares but ReswPlus doesn't is a quantity that silently reads
        // the wording of another.
        Assert.Equal(published, declared);
    }
}
