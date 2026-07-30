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
    [InlineData("id")]
    [InlineData("ms")]
    [InlineData("th")]
    public void SingleFormLanguagesAreMappedExplicitly(string language)
    {
        Assert.Empty(PluralFormsRetriever.RetrieveLanguagesWithoutPluralForm([language]));

        var pluralForm = Assert.Single(PluralFormsRetriever.RetrievePluralFormsForLanguages([language]));

        Assert.Equal("Other", pluralForm.Id);
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
    // The Romance languages that have a 'many' category are mapped to the providers that can return it.
    [InlineData("es", "OnlyOneOrMillions")]
    [InlineData("ca", "OnlyOneOrMillions")]
    [InlineData("it", "OnlyOneOrMillions")]
    // Portuguese shares its 'one' rule with French rather than with the other Romance languages: CLDR gives
    // it 'i = 0..1', so 0 and 1.5 are 'one' there and 'other' in Catalan, Italian and Spanish.
    [InlineData("pt", "ZeroToTwoExcludedOrMillions")]
    [InlineData("fr", "ZeroToTwoExcludedOrMillions")]
    // The languages that share their rules with them keep the providers that don't.
    [InlineData("en", "OnlyOne")]
    [InlineData("de", "OnlyOne")]
    [InlineData("hy", "ZeroToTwoExcluded")]
    // Fulah selects 'one' for an integer part of 0 or 1, the same as Armenian, and not for 0 to 1 inclusive.
    [InlineData("ff", "ZeroToTwoExcluded")]
    // Marathi selects 'one' for exactly 1, so 0 and 0.5 are 'other' there.
    [InlineData("mr", "OnlyOne")]
    // Samburu is 'saq'. It was listed as 'sag', which is not a language code, so its rules never applied.
    [InlineData("saq", "OnlyOne")]
    // Vietnamese has had two plural forms since CLDR 34, so it is not one of the single-form languages.
    [InlineData("vi", "ZeroToOne")]
    public void LanguagesUseTheExpectedProvider(string language, string expectedProviderId)
    {
        var pluralForm = Assert.Single(PluralFormsRetriever.RetrievePluralFormsForLanguages([language]));

        Assert.Equal(expectedProviderId, pluralForm.Id);
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
