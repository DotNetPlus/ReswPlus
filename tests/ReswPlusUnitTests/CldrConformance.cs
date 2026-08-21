using System;
using System.Collections.Generic;
using System.Linq;
using ReswPlus.SourceGenerator.ClassGenerators;
using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Checks the plural rules ReswPlus ships against the ones Unicode CLDR publishes.
/// </summary>
/// <remarks>
/// The rules are hand written, one C# file per plural form, and CLDR revises them between releases: a category
/// a language used to have is dropped, the bounds of another are moved. Nothing about a hand written rule
/// fails when that happens -- the generated code still compiles and still returns a plural form, just the wrong
/// one, in a language the team most likely does not read. This is what fails instead.
/// <para>
/// CLDR publishes, with every rule, the quantities that select it. Those sample lists are pinned in
/// <see cref="CldrPluralRules"/> and replayed here through the very providers the generator emits.
/// </para>
/// </remarks>
public class CldrConformance
{
    /// <summary>
    /// The languages CLDR rules are pinned for, one per plural form ReswPlus ships.
    /// </summary>
    public static TheoryData<string> Languages => [.. CldrPluralRules.Cardinal.Keys];

    [Theory]
    [MemberData(nameof(Languages))]
    public void TheProviderOfALanguageSelectsTheCategoryCldrSelects(string language)
    {
        var form = PluralFormsRetriever.RetrievePluralFormForLanguage(language);

        Assert.NotNull(form);

        var provider = PluralProviderHost.GetProvider(form!.Id);
        var mistakes = new List<string>();

        foreach (var (category, rule) in CldrPluralRules.Cardinal[language])
        {
            foreach (var (quantity, literal) in CldrSamples.Read(rule))
            {
                var selected = provider(quantity);

                if (!string.Equals(selected, category, StringComparison.Ordinal))
                {
                    mistakes.Add($"    {literal}: CLDR selects '{category}', {form.Id}Provider selects '{selected}'");
                }
            }
        }

        Assert.True(
            mistakes.Count == 0,
            $"The '{form.Id}' plural form disagrees with CLDR {CldrPluralRules.Version} for '{language}':" +
            $"{Environment.NewLine}{string.Join(Environment.NewLine, mistakes)}");
    }

    [Theory]
    [MemberData(nameof(Languages))]
    public void TheCategoriesOfALanguageAreTheOnesCldrDeclares(string language)
    {
        var form = PluralFormsRetriever.RetrievePluralFormForLanguage(language);

        Assert.NotNull(form);

        var declared = form!.Categories.Select(category => category.ToString().ToUpperInvariant()).OrderBy(name => name, StringComparer.Ordinal);
        var published = CldrPluralRules.Cardinal[language].Keys.OrderBy(name => name, StringComparer.Ordinal);

        // A category ReswPlus declares but CLDR doesn't is a form the diagnostics ask translators to write and
        // the runtime never looks up. One CLDR declares but ReswPlus doesn't is a quantity that silently reads
        // the wording of another.
        Assert.Equal(published, declared);
    }
}
