using System.Collections.Generic;
using ReswPlus.SourceGenerator;
using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Tests for the language whose plural rules the generated code selects.
/// </summary>
/// <remarks>
/// Windows resolves the resources of an app against the app runtime language list, while the .NET UI culture
/// comes from the display languages of the user. The two are independent, so a resource can be resolved in one
/// language while its plural form is selected with the rules of another. The
/// <c>ReswPlusUseApplicationLanguages</c> property makes a project read both from the same place.
/// </remarks>
public class PluralLanguageSource
{
    /// <summary>
    /// The rules of a language selecting a form for two, and of one that doesn't.
    /// </summary>
    /// <remarks>
    /// Polish has a <c>few</c> form for two while English only has <c>one</c> and <c>other</c>, so the form
    /// two selects says which of the two languages the rules were taken from.
    /// </remarks>
    private static readonly (string Language, string ProviderId)[] EnglishAndPolish =
        [("en", "OnlyOne"), ("pl", "Polish")];

    private static readonly Dictionary<string, string> Forms = new()
    {
        ["FileCount_One"] = "one",
        ["FileCount_Few"] = "few",
        ["FileCount_Many"] = "many",
        ["FileCount_Other"] = "other"
    };

    [Fact]
    public void TheUICultureIsUsedByDefault()
    {
        var host = ResourceLoaderExtensionHost.Create(EnglishAndPolish);

        host.SetApplicationLanguages("pl-PL");

        ResourceLoaderExtensionHost.WithUICulture("en-US", () =>
        {
            Assert.Equal("other", host.GetPlural(Forms, "FileCount", 2));
        });
    }

    [Fact]
    public void TheApplicationLanguagesAreUsedWhenOptedIn()
    {
        var host = ResourceLoaderExtensionHost.Create(EnglishAndPolish, useApplicationLanguages: true);

        host.SetApplicationLanguages("pl-PL");

        // The resources of the app resolve to Polish here, so its rules are the ones that have to apply, even
        // though the user is running the system in English.
        ResourceLoaderExtensionHost.WithUICulture("en-US", () =>
        {
            Assert.Equal("few", host.GetPlural(Forms, "FileCount", 2));
        });
    }

    [Fact]
    public void TheMostPreferredApplicationLanguageWins()
    {
        var host = ResourceLoaderExtensionHost.Create(EnglishAndPolish, useApplicationLanguages: true);

        host.SetApplicationLanguages("pl-PL", "en-US");

        Assert.Equal("few", host.GetPlural(Forms, "FileCount", 2));
    }

    [Fact]
    public void TheUICultureIsTheFallbackWhenThereAreNoApplicationLanguages()
    {
        var host = ResourceLoaderExtensionHost.Create(EnglishAndPolish, useApplicationLanguages: true);

        // An app running outside of a package has no runtime language list to read.
        host.SetApplicationLanguages();

        ResourceLoaderExtensionHost.WithUICulture("pl-PL", () =>
        {
            Assert.Equal("few", host.GetPlural(Forms, "FileCount", 2));
        });
    }

    [Fact]
    public void AnUnusableApplicationLanguageFallsBackToTheUICulture()
    {
        var host = ResourceLoaderExtensionHost.Create(EnglishAndPolish, useApplicationLanguages: true);

        host.SetApplicationLanguages("");

        ResourceLoaderExtensionHost.WithUICulture("pl-PL", () =>
        {
            Assert.Equal("few", host.GetPlural(Forms, "FileCount", 2));
        });
    }

    [Fact]
    public void OnlyTheUICultureIsReadByDefault()
    {
        var resolver = PluralLanguageResolvers.GetResolver(useApplicationLanguages: false);

        Assert.Contains("CultureInfo.CurrentUICulture.TwoLetterISOLanguageName", resolver);

        // Nothing of the app runtime language list should be emitted into a project that didn't opt in.
        Assert.DoesNotContain("ApplicationLanguages", resolver);
    }
}
