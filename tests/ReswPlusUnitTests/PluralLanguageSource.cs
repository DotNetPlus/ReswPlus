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
        var resolver = PluralLanguageResolvers.GetResolver(useApplicationLanguages: false, AppType.WindowsAppSDK);

        Assert.Contains("CultureInfo.CurrentUICulture.TwoLetterISOLanguageName", resolver);

        // Nothing of the app runtime language list should be emitted into a project that didn't opt in.
        Assert.DoesNotContain("ApplicationLanguages", resolver);
    }

    [Fact]
    public void OnlyAWindowsAppSDKProjectReadsTheLanguageOverride()
    {
        var windowsAppSDK = PluralLanguageResolvers.GetResolver(useApplicationLanguages: true, AppType.WindowsAppSDK);
        var uwp = PluralLanguageResolvers.GetResolver(useApplicationLanguages: true, AppType.UWP);

        // The override that works outside of an app package only exists in the Windows App SDK, and reading it
        // from a UWP project wouldn't compile.
        Assert.Contains("Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride", windowsAppSDK);
        Assert.DoesNotContain("Microsoft.Windows.Globalization", uwp);

        Assert.Contains("Windows.Globalization.ApplicationLanguages.Languages", uwp);
    }

    [Fact]
    public void TheLanguageOverrideOfAnUnpackagedAppIsUsed()
    {
        var host = ResourceLoaderExtensionHost.Create(EnglishAndPolish, useApplicationLanguages: true);

        // An unpackaged app has no runtime language list, and the Windows App SDK keeps its override to
        // itself instead of publishing it there.
        host.SetApplicationLanguages();
        host.SetPrimaryLanguageOverride("pl-PL");

        ResourceLoaderExtensionHost.WithUICulture("en-US", () =>
        {
            Assert.Equal("few", host.GetPlural(Forms, "FileCount", 2));
        });
    }

    [Fact]
    public void TheLanguageOverrideWinsOverTheApplicationLanguages()
    {
        var host = ResourceLoaderExtensionHost.Create(EnglishAndPolish, useApplicationLanguages: true);

        host.SetApplicationLanguages("en-US");
        host.SetPrimaryLanguageOverride("pl-PL");

        Assert.Equal("few", host.GetPlural(Forms, "FileCount", 2));
    }

    [Fact]
    public void TheRulesFollowALanguageChange()
    {
        var host = ResourceLoaderExtensionHost.Create(EnglishAndPolish, useApplicationLanguages: true);

        host.SetApplicationLanguages("en-US");

        Assert.Equal("other", host.GetPlural(Forms, "FileCount", 2));

        // An app can change its language while it runs, and the resources follow it, so the rules have to too.
        host.SetApplicationLanguages("pl-PL");

        Assert.Equal("few", host.GetPlural(Forms, "FileCount", 2));
    }

    [Theory]
    [InlineData(AppType.UWP)]
    [InlineData(AppType.WindowsAppSDK)]
    public void TheApplicationLanguagesAreReadByEveryKindOfApp(AppType appType)
    {
        var host = ResourceLoaderExtensionHost.Create(EnglishAndPolish, useApplicationLanguages: true, appType);

        host.SetApplicationLanguages("pl-PL");

        ResourceLoaderExtensionHost.WithUICulture("en-US", () =>
        {
            Assert.Equal("few", host.GetPlural(Forms, "FileCount", 2));
        });
    }

    [Theory]
    // A tag is matched whole first, then shortened one subtag at a time until something is held for it.
    [InlineData("pl")]
    [InlineData("pl-PL")]
    [InlineData("PL-pl")]
    // A valid tag that CultureInfo rejects still has a readable language.
    [InlineData("pl-rozaj-biske-1994")]
    public void TheLanguageIsReadFromTheTagItself(string languageTag)
    {
        var host = ResourceLoaderExtensionHost.Create(EnglishAndPolish, useApplicationLanguages: true);

        host.SetApplicationLanguages(languageTag);

        ResourceLoaderExtensionHost.WithUICulture("en-US", () =>
        {
            Assert.Equal("few", host.GetPlural(Forms, "FileCount", 2));
        });
    }

    /// <summary>
    /// The rules of a language whose tag changes meaning when lower cased by the wrong culture.
    /// </summary>
    private static readonly (string Language, string ProviderId)[] Icelandic = [("is", "Icelandic")];

    [Fact]
    public void ATagIsMatchedUnderACultureThatLowerCasesLettersDifferently()
    {
        // Turkish lower cases 'I' to a dotless 'ı', so a tag folded with the culture of the moment rather than
        // with the invariant one would turn 'IS-IS' into 'ıs-ıs' and match nothing for the rest of the run.
        var host = ResourceLoaderExtensionHost.Create(Icelandic, useApplicationLanguages: true);

        host.SetApplicationLanguages("IS-IS");

        ResourceLoaderExtensionHost.WithUICulture("tr-TR", () =>
        {
            Assert.Equal("one", host.GetPlural(Forms, "FileCount", 21));
        });
    }

    /// <summary>
    /// The rules of Portuguese, which CLDR publishes separately for Portugal.
    /// </summary>
    /// <remarks>
    /// 'pt-PT' selects <c>one</c> for exactly one, while everywhere else Portuguese selects it for anything
    /// below two, so a count of zero is what tells the two apart.
    /// </remarks>
    private static readonly (string Language, string ProviderId)[] Portuguese =
        [("pt-PT", "OnlyOneOrMillions"), ("pt", "ZeroToTwoExcludedOrMillions")];

    [Theory]
    // Portugal declines zero as 'other'...
    [InlineData("pt-PT", "other")]
    // ...while Brazil, and Portuguese with no region at all, decline it as 'one'.
    [InlineData("pt-BR", "one")]
    [InlineData("pt", "one")]
    // A region nothing is held for falls back on the language, not on the other region's rules.
    [InlineData("pt-AO", "one")]
    public void TheRegionOfATagSelectsItsOwnRulesWhenCldrPublishesThem(string languageTag, string expected)
    {
        var host = ResourceLoaderExtensionHost.Create(Portuguese, useApplicationLanguages: true);

        host.SetApplicationLanguages(languageTag);

        ResourceLoaderExtensionHost.WithUICulture("en-US", () =>
        {
            Assert.Equal(expected, host.GetPlural(Forms, "FileCount", 0));
        });
    }
}
