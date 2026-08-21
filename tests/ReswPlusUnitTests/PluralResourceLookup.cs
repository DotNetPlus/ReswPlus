using System.Collections.Generic;
using ReswPlus.SourceGenerator.ClassGenerators;
using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Tests for how the generated code selects and looks up the plural forms of a resource.
/// </summary>
public class PluralResourceLookup
{
    /// <summary>
    /// A Polish resource set, whose language declares every form these tests need.
    /// </summary>
    private static Dictionary<string, string> PolishFiles => new()
    {
        ["FileCount_One"] = "{0} plik",
        ["FileCount_Few"] = "{0} pliki",
        ["FileCount_Many"] = "{0} plików",
        ["FileCount_Other"] = "{0} pliku"
    };

    [Fact]
    public void TheFormOfTheQuantityIsLookedUp()
    {
        var host = ResourceLoaderExtensionHost.Create("pl");

        ResourceLoaderExtensionHost.WithUICulture("pl", () =>
        {
            Assert.Equal("{0} plik", host.GetPlural(PolishFiles, "FileCount", 1));
            Assert.Equal("{0} pliki", host.GetPlural(PolishFiles, "FileCount", 2));
            Assert.Equal("{0} plików", host.GetPlural(PolishFiles, "FileCount", 5));
        });
    }

    [Fact]
    public void AFormThatIsNotDeclaredFallsBackToTheOtherForm()
    {
        var host = ResourceLoaderExtensionHost.Create("pl");

        // A translation that only declares the two forms English needs used to render nothing at all for the
        // quantities selecting the forms it left out.
        var values = new Dictionary<string, string>
        {
            ["FileCount_One"] = "{0} plik",
            ["FileCount_Other"] = "{0} pliku"
        };

        ResourceLoaderExtensionHost.WithUICulture("pl", () =>
        {
            Assert.Equal("{0} pliku", host.GetPlural(values, "FileCount", 2));
            Assert.Equal("{0} pliku", host.GetPlural(values, "FileCount", 5));
        });
    }

    [Fact]
    public void AFormThatIsNotDeclaredFallsBackWhenTheLookupThrows()
    {
        var host = ResourceLoaderExtensionHost.Create("pl");

        var values = new Dictionary<string, string>
        {
            ["FileCount_One"] = "{0} plik",
            ["FileCount_Other"] = "{0} pliku"
        };

        ResourceLoaderExtensionHost.WithUICulture("pl", () =>
        {
            Assert.Equal("{0} pliku", host.GetPlural(values, "FileCount", 2, throwWhenMissing: true));
        });
    }

    [Fact]
    public void AResourceWithNoFormAtAllStillReturnsAnEmptyString()
    {
        var host = ResourceLoaderExtensionHost.Create("pl");

        ResourceLoaderExtensionHost.WithUICulture("pl", () =>
        {
            Assert.Equal("", host.GetPlural(new Dictionary<string, string>(), "Missing", 2));
        });
    }

    [Fact]
    public void TheEmptyStateIsUsedForZero()
    {
        var host = ResourceLoaderExtensionHost.Create("en");

        var values = new Dictionary<string, string>
        {
            ["FileCount_None"] = "No files",
            ["FileCount_One"] = "{0} file",
            ["FileCount_Other"] = "{0} files"
        };

        ResourceLoaderExtensionHost.WithUICulture("en", () =>
        {
            Assert.Equal("No files", host.GetPlural(values, "FileCount", 0, supportNoneState: true));
        });
    }

    [Fact]
    public void AnEmptyStateThatIsNotDeclaredFallsBackToTheSelectedForm()
    {
        var host = ResourceLoaderExtensionHost.Create("en");

        // The empty state is declared in the default language, which is what makes the generator ask for it,
        // so a translation that leaves it out used to render nothing for zero.
        var values = new Dictionary<string, string>
        {
            ["FileCount_One"] = "{0} file",
            ["FileCount_Other"] = "{0} files"
        };

        ResourceLoaderExtensionHost.WithUICulture("en", () =>
        {
            Assert.Equal("{0} files", host.GetPlural(values, "FileCount", 0, supportNoneState: true));
        });
    }

    [Theory]
    // A negative quantity selects the same form as its positive counterpart, because CLDR defines its
    // operands over the absolute value of the quantity.
    [InlineData("ca", -1, "ONE")]
    [InlineData("fr", -1, "ONE")]
    [InlineData("pl", -2, "FEW")]
    [InlineData("sl", -3, "FEW")]
    [InlineData("lv", -10, "ZERO")]
    public void TheFormIsSelectedOnTheAbsoluteValue(string language, double number, string expected)
    {
        var providerId = PluralFormsRetriever.RetrievePluralFormForLanguage(language)!.Id;
        var host = ResourceLoaderExtensionHost.Create("en", providerId);

        var values = new Dictionary<string, string>
        {
            ["Count_One"] = "ONE",
            ["Count_Two"] = "TWO",
            ["Count_Few"] = "FEW",
            ["Count_Many"] = "MANY",
            ["Count_Zero"] = "ZERO",
            ["Count_Other"] = "OTHER"
        };

        ResourceLoaderExtensionHost.WithUICulture("en", () =>
        {
            Assert.Equal(expected, host.GetPlural(values, "Count", number));
        });
    }
}
