using System.Linq;
using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Tests the plural code the generator emits for a project, driven from its resource folders.
/// </summary>
/// <remarks>
/// The rest of the plural tests hand a language straight to the runtime template or to the mapping table. This
/// starts where a project starts -- a folder holding a <c>.resw</c> file -- and goes through the whole
/// pipeline, which is the only way to catch the halves of it disagreeing about what a folder name means.
/// </remarks>
public class PluralGeneration
{
    /// <summary>
    /// A resource declaring the forms of a pluralized string.
    /// </summary>
    private static string PluralResource =>
        ReswTestHelpers.CreateResw(
            ("FileCount_One", "one file", null),
            ("FileCount_Many", "{0} million files", null),
            ("FileCount_Other", "{0} files", null));

    [Fact]
    public void AFolderNamingARegionGetsTheRulesCldrPublishesForThatRegion()
    {
        var run = ReswGeneratorHarness.Run(
        [
            ReswGeneratorHarness.File("en-US", PluralResource),
            ReswGeneratorHarness.File("pt-PT", PluralResource)
        ]);

        run.AssertCompiles();

        // European Portuguese declines the way Catalan and Italian do, not the way CLDR declines bare 'pt'.
        Assert.Contains("OnlyOneOrMillionsProvider.g.cs", run.Sources.Keys);
        Assert.Contains("case \"pt-pt\":", run.Sources["ResourceLoaderExtension.g.cs"]);
    }

    [Fact]
    public void AFolderNamingARegionWithNoRulesOfItsOwnFallsBackToItsLanguage()
    {
        var run = ReswGeneratorHarness.Run(
        [
            ReswGeneratorHarness.File("en-US", PluralResource),
            ReswGeneratorHarness.File("pt-BR", PluralResource)
        ]);

        run.AssertCompiles();

        // CLDR publishes no rules for 'pt-BR', so it takes the ones it publishes for 'pt', which are the rules
        // of French. The tag is matched at run time, so it is the language that appears in the selector.
        Assert.Contains("ZeroToTwoExcludedOrMillionsProvider.g.cs", run.Sources.Keys);

        var selector = run.Sources["ResourceLoaderExtension.g.cs"];

        Assert.Contains("case \"pt\":", selector);
        Assert.DoesNotContain("case \"pt-br\":", selector);
    }

    [Fact]
    public void AProjectWhoseLanguagesHaveNoRulesOfTheirOwnStillCompilesCleanly()
    {
        // Nothing is known about 'zxx', so the project gets no plural rules at all and every quantity takes the
        // one form there is. The generated selector then has nothing to select between, which it has to say
        // without leaving an empty switch behind: that is a warning, and consumers build with warnings as
        // errors.
        var run = ReswGeneratorHarness.Run([ReswGeneratorHarness.File("zxx", PluralResource)]);

        run.AssertCompiles();

        Assert.DoesNotContain("switch (culture)", run.Sources["ResourceLoaderExtension.g.cs"]);
    }

    [Fact]
    public void OnlyTheProvidersTheLanguagesOfTheProjectNeedAreEmitted()
    {
        var run = ReswGeneratorHarness.Run(
        [
            ReswGeneratorHarness.File("en-US", PluralResource),
            ReswGeneratorHarness.File("pl", PluralResource)
        ]);

        run.AssertCompiles();

        var providers = run.Sources.Keys.Where(name => name.EndsWith("Provider.g.cs")).ToArray();

        // The fallback provider, the interface, and one provider each for English and Polish.
        Assert.Contains("PolishProvider.g.cs", providers);
        Assert.Contains("OnlyOneProvider.g.cs", providers);
        Assert.Contains("OtherProvider.g.cs", providers);
        Assert.DoesNotContain("ArabicProvider.g.cs", providers);
    }

    [Fact]
    public void AProviderCarriesTheRuleItWasWrittenFrom()
    {
        var run = ReswGeneratorHarness.Run(
        [
            ReswGeneratorHarness.File("en-US", PluralResource),
            ReswGeneratorHarness.File("pl", PluralResource)
        ]);

        // The condition CLDR publishes is kept beside the code it turned into, so that a reader of the
        // generated file can check one against the other without leaving it.
        Assert.Contains(
            "// few: v = 0 and i % 10 = 2..4 and i % 100 != 12..14",
            run.Sources["PolishProvider.g.cs"]);
    }
}
