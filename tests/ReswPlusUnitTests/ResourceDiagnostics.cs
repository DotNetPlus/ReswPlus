using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Tests for the diagnostics reported on the content of the <c>.resw</c> files.
/// </summary>
/// <remarks>
/// Every rule is covered in both directions: on input that is genuinely broken, and on the valid input that is
/// most likely to be mistaken for it. False positives are worse than missing diagnostics, because an analyzer
/// that reports valid resources gets disabled wholesale.
/// </remarks>
public class ResourceDiagnostics
{
    [Fact]
    public void PlaceholderMismatch_IsReportedWhenATranslationDropsAPlaceholder()
    {
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(("Greeting", "Hello {0}, you are {1}", "#Format[String name, Int age]"))),
            ("fr", ReswTestHelpers.CreateResw(("Greeting", "Bonjour {0}", null))));

        var diagnostic = Assert.Single(diagnostics);

        Assert.Equal("RESWP0006", diagnostic.Id);
        Assert.Contains(@"Strings\fr\Resources.resw", diagnostic.Location.GetLineSpan().Path);
    }

    [Fact]
    public void PlaceholderMismatch_IsNotReportedWhenATranslationReordersPlaceholders()
    {
        // Languages routinely need a different word order, which is exactly what indexed placeholders are for.
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(("Greeting", "Hello {0}, you are {1}", "#Format[String name, Int age]"))),
            ("fr", ReswTestHelpers.CreateResw(("Greeting", "Vous avez {1} ans, {0}", null))));

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void PlaceholderMismatch_IsNotReportedForEscapedBraces()
    {
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(("Greeting", "Hello {0}", "#Format[String name]"))),
            ("fr", ReswTestHelpers.CreateResw(("Greeting", "Bonjour {{{0}}}", null))));

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void PlaceholderMismatch_IsNotReportedForResourcesThatAreNeverFormatted()
    {
        // Without a #Format tag the value is returned verbatim, so the braces are literal text.
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(("Sample", "Use {0} as a placeholder", null))),
            ("fr", ReswTestHelpers.CreateResw(("Sample", "Utilisez un espace réservé", null))));

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void PlaceholderMismatch_IsNotReportedForResourcesMissingFromATranslation()
    {
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(("Greeting", "Hello {0}", "#Format[String name]"))),
            ("fr", ReswTestHelpers.CreateResw(("Other", "Autre", null))));

        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Id == "RESWP0006");
    }

    [Fact]
    public void PlaceholderMismatch_IsNotReportedWhenATranslationAddsAPlaceholder()
    {
        // English routinely spells the singular out while other languages still need the number, and
        // string.Format ignores the arguments a value doesn't reference, so this is valid.
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(
                ("FileCount_One", "One file", "#Format[Plural Int count]"),
                ("FileCount_Other", "{0} files", null))),
            ("fr", ReswTestHelpers.CreateResw(
                ("FileCount_One", "{0} fichier", null),
                ("FileCount_Other", "{0} fichiers", null))));

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void PlaceholderMismatch_IsReportedWhenATranslationSubstitutesTheWrongArgument()
    {
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(("Treat", "give {0} biscuits to {2}", "#Format[Int count, Variant petType, String petName]"))),
            ("fr", ReswTestHelpers.CreateResw(("Treat", "donnez {0} biscuits à {1}", null))));

        var diagnostic = Assert.Single(diagnostics);

        Assert.Equal("RESWP0006", diagnostic.Id);
        Assert.Contains("{2}", diagnostic.GetMessage());
    }

    [Fact]
    public void FormattingProblems_AreNotReportedForATagThatTheGeneratorCannotResolve()
    {
        // A Reference() pointing at a pluralized resource cannot be resolved, so the generator discards the whole
        // tag and emits the value unformatted: nothing about it can throw, and its braces are literal.
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(
                ("FileCount_One", "{0} file", "#Format[Plural Int count]"),
                ("FileCount_Other", "{0} files", null),
                ("Message", "{0} and {1} and {2}", "#Format[FileCount_One(), String other]"))));

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void UndeclaredFormatParameter_IsReportedWhenAValueUsesMoreParametersThanDeclared()
    {
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(("Greeting", "Hello {0}, you are {1}", "#Format[String name]"))));

        var diagnostic = Assert.Single(diagnostics);

        Assert.Equal("RESWP0007", diagnostic.Id);
        Assert.Contains("{1}", diagnostic.GetMessage());
    }

    [Fact]
    public void UndeclaredFormatParameter_IsReportedForTranslationsToo()
    {
        // The tag lives in the default language, but every translation is formatted with the same arguments.
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(("Greeting", "Hello {0}", "#Format[String name]"))),
            ("fr", ReswTestHelpers.CreateResw(("Greeting", "Bonjour {1}", null))));

        var diagnostic = Assert.Single(diagnostics);

        Assert.Equal("RESWP0007", diagnostic.Id);
    }

    [Fact]
    public void UndeclaredFormatParameter_IsNotReportedWhenAValueUsesFewerParametersThanDeclared()
    {
        // The empty state of a pluralized resource legitimately doesn't show the quantity.
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(
                ("FileCount_None", "No files", null),
                ("FileCount_One", "{0} file", "#Format[Plural Int count]"),
                ("FileCount_Other", "{0} files", null))));

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void UndeclaredFormatParameter_IsNotReportedForEscapedBraces()
    {
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(("Greeting", "Hello {0}, use {{1}} to escape", "#Format[String name]"))));

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void MissingPluralForms_AreReportedForALanguageThatNeedsMoreForms()
    {
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(
                ("FileCount_One", "{0} file", "#Format[Plural Int count]"),
                ("FileCount_Other", "{0} files", null))),
            ("pl", ReswTestHelpers.CreateResw(
                ("FileCount_One", "{0} plik", null),
                ("FileCount_Other", "{0} pliku", null))));

        var diagnostic = Assert.Single(diagnostics);

        Assert.Equal("RESWP0008", diagnostic.Id);
        Assert.Contains("'_Few'", diagnostic.GetMessage());
        Assert.Contains("'_Many'", diagnostic.GetMessage());
    }

    [Fact]
    public void MissingPluralForms_AreNotReportedWhenTheLanguageHasThemAll()
    {
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(
                ("FileCount_One", "{0} file", "#Format[Plural Int count]"),
                ("FileCount_Other", "{0} files", null))),
            ("pl", ReswTestHelpers.CreateResw(
                ("FileCount_One", "{0} plik", null),
                ("FileCount_Few", "{0} pliki", null),
                ("FileCount_Many", "{0} plików", null),
                ("FileCount_Other", "{0} pliku", null))));

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void MissingPluralForms_AreNotReportedForALanguageWithoutAPluralProvider()
    {
        // Without a known plural provider no form can be assumed to be required.
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(
                ("FileCount_One", "{0} file", "#Format[Plural Int count]"),
                ("FileCount_Other", "{0} files", null))),
            ("qqq", ReswTestHelpers.CreateResw(("FileCount_One", "{0}", null))));

        Assert.Empty(diagnostics);
    }

    [Theory]
    // The Romance languages have a 'many' category, but it only applies to exact multiples of a million.
    [InlineData("es")]
    [InlineData("ca")]
    [InlineData("it")]
    [InlineData("pt")]
    [InlineData("fr")]
    public void MissingPluralForms_AreNotReportedForACategoryAResourceDoesNotHaveToDefine(string language)
    {
        // Requiring '_Many' would warn about every resource set of these languages for a quantity almost none
        // of them display, and the lookup falls back to '_Other', which is the wording they already ship.
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(
                ("FileCount_One", "{0} file", "#Format[Plural Int count]"),
                ("FileCount_Other", "{0} files", null))),
            (language, ReswTestHelpers.CreateResw(
                ("FileCount_One", "{0} archivo", null),
                ("FileCount_Other", "{0} archivos", null))));

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void MissingPluralForms_AreStillReportedForACategoryAResourceHasToDefine()
    {
        // The exemption above is per category, so a language that genuinely needs '_Many' still gets told.
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(
                ("FileCount_One", "{0} file", "#Format[Plural Int count]"),
                ("FileCount_Other", "{0} files", null))),
            ("pl", ReswTestHelpers.CreateResw(
                ("FileCount_One", "{0} plik", null),
                ("FileCount_Few", "{0} pliki", null),
                ("FileCount_Other", "{0} pliku", null))));

        var diagnostic = Assert.Single(diagnostics);

        Assert.Equal("RESWP0008", diagnostic.Id);
        Assert.Contains("'_Many'", diagnostic.GetMessage());
    }

    [Fact]
    public void MissingPluralForms_AreReportedPerVariantOfAPluralizedResource()
    {
        var diagnostics = ReswTestHelpers.Analyze(
            "pl",
            ("pl", ReswTestHelpers.CreateResw(
                ("Treat_Variant1_One", "{0} smakołyk", "#Format[Plural Int count, Variant petType]"),
                ("Treat_Variant1_Few", "{0} smakołyki", null),
                ("Treat_Variant1_Many", "{0} smakołyków", null),
                ("Treat_Variant1_Other", "{0} smakołyku", null),
                ("Treat_Variant2_One", "{0} smakołyk", null),
                ("Treat_Variant2_Other", "{0} smakołyku", null))));

        var diagnostic = Assert.Single(diagnostics);

        Assert.Equal("RESWP0008", diagnostic.Id);
        Assert.Contains("Treat_Variant2", diagnostic.GetMessage());
    }

    [Fact]
    public void MissingPluralForms_AreNotReportedForAResourceThatOnlyExistsInATranslation()
    {
        // A resource left behind in a translation after the default language dropped it generates no member, so
        // its plural forms are never looked up.
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(("Welcome", "Welcome!", null))),
            ("pl", ReswTestHelpers.CreateResw(("Orphan_One", "{0} plik", null))));

        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Id == "RESWP0008");
    }

    [Fact]
    public void MissingPluralForms_DoNotIncludeZeroWhenTheResourceHasAnEmptyState()
    {
        // GetPlural short circuits a zero quantity to the _None form, and the Arabic provider only returns the
        // zero category for a quantity that is itself zero, so _Zero is unreachable here.
        var diagnostics = ReswTestHelpers.Analyze(
            "ar",
            ("ar", ReswTestHelpers.CreateResw(
                ("FileCount_None", "لا ملفات", "#Format[Plural Int count]"),
                ("FileCount_One", "{0}", null),
                ("FileCount_Two", "{0}", null),
                ("FileCount_Few", "{0}", null),
                ("FileCount_Many", "{0}", null),
                ("FileCount_Other", "{0}", null))));

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void MissingPluralForms_StillIncludeZeroForALanguageThatUsesItForOtherQuantities()
    {
        // The Latvian provider returns the zero category for quantities such as 11 or 20 as well, so the _None
        // form does not make _Zero unreachable.
        var diagnostics = ReswTestHelpers.Analyze(
            "lv",
            ("lv", ReswTestHelpers.CreateResw(
                ("FileCount_None", "Nav failu", "#Format[Plural Int count]"),
                ("FileCount_One", "{0} fails", null),
                ("FileCount_Other", "{0} faili", null))));

        var diagnostic = Assert.Single(diagnostics);

        Assert.Equal("RESWP0008", diagnostic.Id);
        Assert.Contains("'_Zero'", diagnostic.GetMessage());
    }

    [Fact]
    public void DuplicateResource_IsReportedForKeysThatOnlyDifferByCase()
    {
        // Resource lookup is case insensitive, so both members resolve to the same string at runtime.
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(
                ("Welcome", "Welcome!", null),
                ("welcome", "Welcome?", null))));

        var diagnostic = Assert.Single(diagnostics);

        Assert.Equal("RESWP0009", diagnostic.Id);
    }

    [Fact]
    public void DuplicateResource_IsReportedWhenAPlainResourceCollidesWithAPluralizedOne()
    {
        // This one doesn't even compile: both the property and the pluralized method are named 'FileCount'.
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(
                ("FileCount_One", "{0} file", "#Format[Plural Int count]"),
                ("FileCount_Other", "{0} files", null),
                ("FileCount", "Some files", null))));

        var diagnostic = Assert.Single(diagnostics);

        Assert.Equal("RESWP0009", diagnostic.Id);
        Assert.Contains("FileCount", diagnostic.GetMessage());
    }

    [Fact]
    public void DuplicateResource_IsNotReportedForDistinctResources()
    {
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(
                ("Welcome", "Welcome!", null),
                ("WelcomeBack", "Welcome back!", null),
                ("FileCount_One", "{0} file", "#Format[Plural Int count]"),
                ("FileCount_Other", "{0} files", null))));

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void InvalidFormatString_IsReportedForAMalformedFormattedValue()
    {
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(("Greeting", "Hello {0, you are {1}", "#Format[String name, Int age]"))));

        var diagnostic = Assert.Single(diagnostics);

        Assert.Equal("RESWP0010", diagnostic.Id);
    }

    [Fact]
    public void InvalidFormatString_IsReportedForTranslationsToo()
    {
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(("Greeting", "Hello {0}", "#Format[String name]"))),
            ("fr", ReswTestHelpers.CreateResw(("Greeting", "Bonjour {0", null))));

        var diagnostic = Assert.Single(diagnostics);

        Assert.Equal("RESWP0010", diagnostic.Id);
    }

    [Fact]
    public void InvalidFormatString_IsNotReportedForResourcesThatAreNeverFormatted()
    {
        // A value that is never passed to string.Format can contain any brace it wants.
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(("Shortcut", "Press { to open the menu", null))));

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void InvalidFormatString_IsNotReportedForValidFormatItems()
    {
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(("Donate", "Hey {1}, donate {2:C2} to {0}!", "#Format[\"WWF\", String username, Int amount]"))));

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void IgnoredResourcesAreNotAnalyzed()
    {
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(("Welcome", "Welcome!", "#ReswPlusIgnore"), ("welcome", "Welcome?", null))));

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task TheAnalyzerIsRegisteredAndReportsThroughTheCompiler()
    {
        // The rules are exercised directly by the other tests, this one covers the wiring: without the right
        // registration the analyzer would silently report nothing when the compiler runs it.
        var diagnostics = await ReswTestHelpers.RunAnalyzerAsync(
            ("en-US", ReswTestHelpers.CreateResw(("Greeting", "Hello {0}, you are {1}", "#Format[String name, Int age]"))),
            ("fr", ReswTestHelpers.CreateResw(("Greeting", "Bonjour {0}", null))));

        var diagnostic = Assert.Single(diagnostics);

        Assert.Equal("RESWP0006", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
    }

    [Fact]
    public void DiagnosticsPointAtTheLineOfTheOffendingResource()
    {
        var diagnostics = ReswTestHelpers.Analyze(
            "en-US",
            ("en-US", ReswTestHelpers.CreateResw(
                ("First", "First", null),
                ("Greeting", "Hello {0} {1}", "#Format[String name]"))));

        var lineSpan = Assert.Single(diagnostics).Location.GetLineSpan();
        var line = ReswTestHelpers.CreateResw(("First", "First", null), ("Greeting", "Hello {0} {1}", "#Format[String name]"))
            .Split('\n')[lineSpan.StartLinePosition.Line];

        Assert.Contains(@"name=""Greeting""", line);
        Assert.Equal("Greeting".Length, lineSpan.EndLinePosition.Character - lineSpan.StartLinePosition.Character);
    }
}
