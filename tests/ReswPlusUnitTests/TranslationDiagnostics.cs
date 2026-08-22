using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;

namespace ReswPlusUnitTests;

public class TranslationDiagnostics
{
    private static string DefaultResources => ReswTestHelpers.CreateResw(
        ("Welcome", "Welcome", null),
        ("OnlyDefault", "Only in the default language", null),
        ("Same", "ReswPlus", null),
        ("Message", "Hello {0}", "#Format[String name]"));

    private static string FrenchResources => ReswTestHelpers.CreateResw(
        ("Welcome", "Bienvenue", null),
        ("OnlyFrench", "Seulement en français", null),
        ("Same", "ReswPlus", null),
        ("Message", "Bonjour", null));

    [Fact]
    public async Task DefaultModeReportsHarmlessDriftAsInformationAndCriticalDriftAsWarning()
    {
        var diagnostics = await Analyze("Default");

        AssertDiagnostic(diagnostics, "RESWP0016", DiagnosticSeverity.Info);
        AssertDiagnostic(diagnostics, "RESWP0017", DiagnosticSeverity.Info);
        AssertDiagnostic(diagnostics, "RESWP0018", DiagnosticSeverity.Info);
        AssertDiagnostic(diagnostics, "RESWP0006", DiagnosticSeverity.Warning);
    }

    [Fact]
    public async Task StrictModeEscalatesHarmlessDriftToWarningsAndCriticalDriftToErrors()
    {
        var diagnostics = await Analyze("Strict");

        AssertDiagnostic(diagnostics, "RESWP0016", DiagnosticSeverity.Warning);
        AssertDiagnostic(diagnostics, "RESWP0017", DiagnosticSeverity.Warning);
        AssertDiagnostic(diagnostics, "RESWP0018", DiagnosticSeverity.Info);
        AssertDiagnostic(diagnostics, "RESWP0006", DiagnosticSeverity.Error);
    }

    [Fact]
    public async Task OffDisablesCrossLanguageChecksButKeepsPerFileValidation()
    {
        var malformedFrench = ReswTestHelpers.CreateResw(
            ("Welcome", "Bienvenue", null),
            ("Message", "Bonjour {1}", null));

        var diagnostics = await ReswTestHelpers.RunAnalyzerAsyncWithOptions(
            "en-US",
            "Off",
            ("en-US", DefaultResources),
            ("fr", malformedFrench));

        Assert.DoesNotContain(diagnostics, diagnostic =>
            diagnostic.Id is "RESWP0006" or "RESWP0008" or "RESWP0016" or "RESWP0017" or
                "RESWP0018" or "RESWP0019" or "RESWP0020");
        AssertDiagnostic(diagnostics, "RESWP0007", DiagnosticSeverity.Warning);
    }

    [Fact]
    public async Task AnUnknownModeUsesDefaultBehavior()
    {
        var diagnostics = await Analyze("unexpected");

        AssertDiagnostic(diagnostics, "RESWP0016", DiagnosticSeverity.Info);
        AssertDiagnostic(diagnostics, "RESWP0006", DiagnosticSeverity.Warning);
    }

    [Fact]
    public async Task AnOmittedModeUsesDefaultBehavior()
    {
        var diagnostics = await ReswTestHelpers.RunAnalyzerAsyncWithOptions(
            "en-US",
            translationChecks: null,
            ("en-US", DefaultResources),
            ("fr", FrenchResources));

        AssertDiagnostic(diagnostics, "RESWP0016", DiagnosticSeverity.Info);
        AssertDiagnostic(diagnostics, "RESWP0006", DiagnosticSeverity.Warning);
    }

    [Fact]
    public async Task StrictModeEscalatesCriticalPerFileValidation()
    {
        var malformed = ReswTestHelpers.CreateResw(
            ("Message", "Hello {1}", "#Format[String name]"));

        var diagnostics = await ReswTestHelpers.RunAnalyzerAsyncWithOptions(
            "en-US",
            "Strict",
            ("en-US", malformed));

        AssertDiagnostic(diagnostics, "RESWP0007", DiagnosticSeverity.Error);
    }

    [Fact]
    public async Task MissingVariantsAreCriticalAndExtraVariantsAreAdvisory()
    {
        var defaults = ReswTestHelpers.CreateResw(
            ("Greeting_Variant1", "Hello", "#Format[Variant kind]"),
            ("Greeting_Variant2", "Hi", null));
        var translation = ReswTestHelpers.CreateResw(
            ("Greeting_Variant1", "Bonjour", null),
            ("Greeting_Variant3", "Salut", null));

        var normal = await ReswTestHelpers.RunAnalyzerAsyncWithOptions(
            "en-US",
            "Default",
            ("en-US", defaults),
            ("fr", translation));
        var strict = await ReswTestHelpers.RunAnalyzerAsyncWithOptions(
            "en-US",
            "Strict",
            ("en-US", defaults),
            ("fr", translation));

        AssertDiagnostic(normal, "RESWP0019", DiagnosticSeverity.Warning);
        AssertDiagnostic(normal, "RESWP0020", DiagnosticSeverity.Info);
        AssertDiagnostic(strict, "RESWP0019", DiagnosticSeverity.Error);
        AssertDiagnostic(strict, "RESWP0020", DiagnosticSeverity.Warning);
    }

    [Fact]
    public async Task DifferentLanguageSpecificPluralFormsAreNotShapeDrift()
    {
        var defaults = ReswTestHelpers.CreateResw(
            ("Items_One", "{0} item", "#Format[Plural Int count]"),
            ("Items_Other", "{0} items", null));
        var polish = ReswTestHelpers.CreateResw(
            ("Items_One", "{0} element", null),
            ("Items_Few", "{0} elementy", null),
            ("Items_Many", "{0} elementów", null),
            ("Items_Other", "{0} elementu", null));

        var diagnostics = await ReswTestHelpers.RunAnalyzerAsyncWithOptions(
            "en-US",
            "Default",
            ("en-US", defaults),
            ("pl", polish));

        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Id is "RESWP0019" or "RESWP0020");
    }

    private static Task<System.Collections.Immutable.ImmutableArray<Diagnostic>> Analyze(string mode)
    {
        return ReswTestHelpers.RunAnalyzerAsyncWithOptions(
            "en-US",
            mode,
            ("en-US", DefaultResources),
            ("fr", FrenchResources));
    }

    private static void AssertDiagnostic(
        System.Collections.Generic.IEnumerable<Diagnostic> diagnostics,
        string id,
        DiagnosticSeverity severity)
    {
        var diagnostic = Assert.Single(diagnostics, candidate => candidate.Id == id);

        Assert.Equal(severity, diagnostic.Severity);
    }
}
