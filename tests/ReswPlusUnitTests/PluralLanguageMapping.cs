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
    [InlineData("pt", "OnlyOneOrMillions")]
    [InlineData("fr", "ZeroToTwoExcludedOrMillions")]
    // The languages that share their rules with them keep the providers that don't.
    [InlineData("en", "OnlyOne")]
    [InlineData("de", "OnlyOne")]
    [InlineData("hy", "ZeroToTwoExcluded")]
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
