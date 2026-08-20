using System;
using System.Collections.Generic;
using System.Globalization;
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
            foreach (var (quantity, literal) in ReadSamples(rule))
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

    /// <summary>
    /// Reads the quantities CLDR publishes as selecting a rule.
    /// </summary>
    /// <param name="rule">The rule as CLDR publishes it, sample lists included.</param>
    /// <returns>Each sample quantity, with the literal CLDR writes it as.</returns>
    /// <remarks>
    /// The decimal samples that a <see cref="double"/> cannot carry faithfully are left out. CLDR distinguishes
    /// <c>1.0</c> from <c>1</c>, and some rules read the number of decimals of the quantity, but the generated
    /// members take a <see cref="double"/>, which does not carry the trailing zeros the two differ by. Those
    /// samples are not something the providers can be asked to get right.
    /// </remarks>
    private static IEnumerable<(double Quantity, string Literal)> ReadSamples(string rule)
    {
        foreach (var section in rule.Split('@').Skip(1))
        {
            var space = section.IndexOf(' ');

            if (space < 0)
            {
                continue;
            }

            var kind = section.Substring(0, space);

            if (kind is not ("integer" or "decimal"))
            {
                continue;
            }

            foreach (var item in section.Substring(space + 1).Split(','))
            {
                var sample = item.Trim();

                // CLDR closes an open ended sample list with an ellipsis, and writes the quantities of the
                // compact notations -- "1c6" for one million -- that a plain double carries no trace of.
                if (sample.Length == 0 || sample == "\u2026" || sample.IndexOf('c') >= 0)
                {
                    continue;
                }

                foreach (var literal in Expand(sample))
                {
                    var quantity = double.Parse(literal, CultureInfo.InvariantCulture);

                    // The shortest representation that round trips is what the providers read the number of
                    // decimals of a quantity from, so a literal it doesn't reproduce is one a double cannot
                    // carry.
                    if (quantity.ToString("R", CultureInfo.InvariantCulture) == literal)
                    {
                        yield return (quantity, literal);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Expands a CLDR sample, which is either a single quantity or a range of them.
    /// </summary>
    /// <param name="sample">The sample to expand.</param>
    /// <returns>The literals the sample stands for.</returns>
    private static IEnumerable<string> Expand(string sample)
    {
        var separator = sample.IndexOf('~');

        if (separator < 0)
        {
            return [sample];
        }

        var from = sample.Substring(0, separator);
        var to = sample.Substring(separator + 1);

        // A range steps by one unit of the last decimal it is written with, so '0.0~0.9' stands for ten
        // quantities and '0~9' for ten others.
        var dot = from.IndexOf('.');
        var decimals = dot < 0 ? 0 : from.Length - dot - 1;
        var step = Math.Pow(10, -decimals);
        var format = decimals == 0 ? "0" : "0." + new string('0', decimals);

        var start = double.Parse(from, CultureInfo.InvariantCulture);
        var end = double.Parse(to, CultureInfo.InvariantCulture);
        var literals = new List<string>();

        for (var value = start; value <= end + (step / 2); value += step)
        {
            literals.Add(value.ToString(format, CultureInfo.InvariantCulture));
        }

        return literals;
    }
}
