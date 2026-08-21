using System;
using System.Linq;
using ReswPlus.SourceGenerator.ClassGenerators;
using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Tests for the mapping from a language to the plural rules the generator emits for it.
/// </summary>
public class PluralLanguageMapping
{
    [Theory]
    // These languages have a single plural form, and are mapped explicitly so that reaching the default branch
    // of the generated selector always means ReswPlus has no rules for the language.
    [InlineData("zh")]
    [InlineData("ja")]
    [InlineData("ko")]
    [InlineData("vi")]
    [InlineData("id")]
    [InlineData("ms")]
    [InlineData("th")]
    public void SingleFormLanguagesAreMappedExplicitly(string language)
    {
        Assert.Empty(PluralFormsRetriever.RetrieveLanguagesWithoutPluralForm([language]));

        var pluralForm = Assert.Single(PluralFormsRetriever.RetrievePluralFormsForLanguages([language]));

        Assert.Equal([ReswPlus.SourceGenerator.ClassGenerators.PluralCategory.Other], pluralForm.Categories);
    }

    [Theory]
    // A language ReswPlus has no rules for silently loses every form other than 'Other', so it is reported.
    [InlineData("xx")]
    [InlineData("zz")]
    public void UnknownLanguagesAreReported(string language)
    {
        Assert.Equal([language], PluralFormsRetriever.RetrieveLanguagesWithoutPluralForm([language]));
    }

    [Fact]
    public void KnownLanguagesAreNotReported()
    {
        Assert.Empty(PluralFormsRetriever.RetrieveLanguagesWithoutPluralForm(["en", "fr", "pl", "ar", "zh"]));
    }

    [Fact]
    public void UnknownLanguagesAreReportedOnlyOnce()
    {
        Assert.Equal(["xx"], PluralFormsRetriever.RetrieveLanguagesWithoutPluralForm(["xx", "en", "xx"]));
    }

    [Theory]
    // The Romance languages that have a 'many' category share the rules that can return it.
    [InlineData("es", "ca")]
    [InlineData("ca", "it")]
    // Portuguese declines differently in Portugal than everywhere else, and CLDR publishes the two separately.
    // The whole tag is what tells them apart, so 'pt-PT' takes its own rules and a bare 'pt' -- with it 'pt-BR'
    // -- takes the ones CLDR gives it, which are the rules of French.
    [InlineData("pt-PT", "ca")]
    [InlineData("pt", "fr")]
    [InlineData("pt-BR", "fr")]
    // A tag no rules are held for falls back on the rules of its language.
    [InlineData("fr-CA", "fr")]
    // The casing and the separator a tag is written with don't change which rules it gets.
    [InlineData("PT_pt", "pt-PT")]
    // Languages that decline alike share one set of rules, and so one generated class.
    [InlineData("en", "de")]
    // Marathi selects 'one' for exactly 1, so 0 and 0.5 are 'other' there, the same as English.
    [InlineData("mr", "en")]
    // Samburu is 'saq'. It was once listed as 'sag', which is not a language code, so its rules never applied.
    [InlineData("saq", "en")]
    // Fulah selects 'one' for an integer part of 0 or 1, the same as Armenian.
    [InlineData("ff", "hy")]
    public void LanguagesThatDeclineAlikeShareOneSetOfRules(string language, string other)
    {
        var form = PluralFormsRetriever.RetrievePluralFormForLanguage(language);
        var otherForm = PluralFormsRetriever.RetrievePluralFormForLanguage(other);

        Assert.NotNull(form);
        Assert.NotNull(otherForm);
        Assert.Equal(otherForm!.Id, form!.Id);
    }

    [Theory]
    // European Portuguese is the case the whole tag exists for: CLDR declines it differently from the 'pt' that
    // Brazil follows, and they disagree about zero.
    [InlineData("pt-PT", "pt")]
    [InlineData("pt-PT", "pt-BR")]
    // Languages that merely look similar are not merged.
    [InlineData("en", "fr")]
    [InlineData("pl", "ru")]
    public void LanguagesThatDeclineDifferentlyDoNotShareRules(string language, string other)
    {
        var form = PluralFormsRetriever.RetrievePluralFormForLanguage(language);
        var otherForm = PluralFormsRetriever.RetrievePluralFormForLanguage(other);

        Assert.NotNull(form);
        Assert.NotNull(otherForm);
        Assert.NotEqual(otherForm!.Id, form!.Id);
    }

    [Fact]
    public void OneClassIsGeneratedForEachSetOfRules()
    {
        var forms = PluralFormsRetriever.PluralFormsForTesting.ToList();

        // Far fewer classes than languages: the point of grouping by the rules themselves rather than by a list
        // someone maintains.
        Assert.True(
            forms.Sum(form => form.Languages.Length) > forms.Count * 4,
            $"{forms.Count} forms for {forms.Sum(form => form.Languages.Length)} languages.");

        // And no two of them are the same rules under two names.
        var duplicates = forms
            .GroupBy(form => form.Source, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => string.Join(", ", group.Select(form => form.Id)))
            .ToArray();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void NoLanguageIsMappedToMoreThanOnePluralForm()
    {
        // A language listed under two plural forms would emit two 'case' labels for it in the generated
        // selector, which doesn't compile.
        var duplicates = PluralFormsRetriever.PluralFormsForTesting
            .SelectMany(pluralForm => pluralForm.Languages)
            .GroupBy(language => language)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void EveryPluralFormHasATemplate()
    {
        foreach (var pluralForm in PluralFormsRetriever.PluralFormsForTesting)
        {
            // Compiling it is what proves the template exists and is valid C#.
            _ = PluralProviderHost.GetProvider(pluralForm.Id);
        }
    }
}
