using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ReswPlus.SourceGenerator;
using ReswPlus.SourceGenerator.Pipeline;
using Xunit;

namespace ReswPlusUnitTests;

public class AppTypeOverride
{
    private static string SampleResources => ReswTestHelpers.CreateResw(
        ("Welcome", "Welcome!", null),
        ("Greeting", "Hello {0}", "#Format[String name]"),
        ("Items_One", "{0} item", "#Format[Plural Int count]"),
        ("Items_Other", "{0} items", "#Format[Plural Int count]"));

    [Theory]
    [InlineData(AppType.WindowsAppSDK, null)]
    [InlineData(AppType.WindowsAppSDK, "")]
    [InlineData(AppType.WindowsAppSDK, " \t ")]
    [InlineData(AppType.WindowsAppSDK, "Auto")]
    [InlineData(AppType.WindowsAppSDK, " auto ")]
    [InlineData(AppType.UWP, null)]
    [InlineData(AppType.UWP, "")]
    [InlineData(AppType.UWP, " \t ")]
    [InlineData(AppType.UWP, "Auto")]
    [InlineData(AppType.UWP, " auto ")]
    [InlineData(AppType.UnoWinUI, null)]
    [InlineData(AppType.UnoWinUI, "")]
    [InlineData(AppType.UnoWinUI, " \t ")]
    [InlineData(AppType.UnoWinUI, "Auto")]
    [InlineData(AppType.UnoWinUI, " auto ")]
    public void AutomaticSelectionPreservesDetection(AppType appType, string? selection)
    {
        var run = Run(appType, selection);

        Assert.Empty(run.Diagnostics);
        AssertAppModel(run, appType);
        run.AssertCompiles();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t ")]
    [InlineData("Auto")]
    [InlineData(" auto ")]
    public void AutomaticSelectionPreservesTheLegacyUseUwpFallback(string? selection)
    {
        var run = Run(
            AppType.Unknown,
            selection,
            useUwp: true,
            references: PlatformStubs.NativeAotUwpReferences());

        Assert.Empty(run.Diagnostics);
        AssertAppModel(run, AppType.UWP);
        run.AssertCompiles();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Auto")]
    public void AutomaticSelectionDoesNotInventAnAppModel(string? selection)
    {
        var run = Run(AppType.Unknown, selection);
        var diagnostic = Assert.Single(run.Diagnostics);

        Assert.Equal("RESWP0005", diagnostic.Id);
        Assert.Contains("ReswPlusAppType", diagnostic.GetMessage());
        Assert.Empty(run.Sources);
    }

    [Theory]
    [InlineData(AppType.WindowsAppSDK, false)]
    [InlineData(AppType.WindowsAppSDK, true)]
    [InlineData(AppType.UWP, false)]
    [InlineData(AppType.UWP, true)]
    [InlineData(AppType.UnoWinUI, false)]
    [InlineData(AppType.UnoWinUI, true)]
    public void AnExplicitSelectionRecognizesAnOtherwiseUnknownProjection(AppType appType, bool useApplicationLanguages)
    {
        var references = PlatformStubs.ReferencesWithDetectionHint(appType);
        Assert.Equal("RESWP0005", Assert.Single(Run(AppType.Unknown, references: references).Diagnostics).Id);

        var run = Run(
            AppType.Unknown,
            appType.ToString(),
            useApplicationLanguages: useApplicationLanguages,
            references: references);

        Assert.Empty(run.Diagnostics);
        AssertAppModel(run, appType);
        Assert.Contains("public interface IResources", run.Source("Resources.resw"));
        Assert.Contains("public sealed class ResourcesProvider : IResources", run.Source("Resources.resw"));
        if (useApplicationLanguages)
        {
            var pluralSupport = run.Source("ResourceLoaderExtension");
            Assert.Contains("global::Windows.Globalization.ApplicationLanguages.Languages", pluralSupport);
            Assert.Equal(
                appType == AppType.WindowsAppSDK,
                pluralSupport.Contains("global::Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride"));
        }
        run.AssertCompiles();
    }

    [Theory]
    [InlineData(AppType.WindowsAppSDK, AppType.UWP)]
    [InlineData(AppType.UWP, AppType.WindowsAppSDK)]
    [InlineData(AppType.UnoWinUI, AppType.WindowsAppSDK)]
    [InlineData(AppType.UnoWinUI, AppType.UWP)]
    public void AnExplicitSelectionOverridesMisleadingReferences(AppType actualAppType, AppType hintedAppType)
    {
        var references = PlatformStubs.ReferencesWithDetectionHint(actualAppType, hintedAppType);
        var automatic = Run(AppType.Unknown, references: references);
        Assert.Empty(automatic.Diagnostics);
        AssertAppModel(automatic, hintedAppType);

        var explicitSelection = Run(
            AppType.Unknown,
            actualAppType.ToString(),
            useApplicationLanguages: true,
            references: references);

        Assert.Empty(explicitSelection.Diagnostics);
        AssertAppModel(explicitSelection, actualAppType);
        explicitSelection.AssertCompiles();
    }

    [Theory]
    [InlineData(AppType.WindowsAppSDK)]
    [InlineData(AppType.UWP)]
    [InlineData(AppType.UnoWinUI)]
    public void AnExplicitSelectionTakesPrecedenceOverUseUwp(AppType appType)
    {
        var run = Run(
            AppType.Unknown,
            appType.ToString(),
            useUwp: true,
            references: PlatformStubs.ReferencesWithDetectionHint(appType));

        Assert.Empty(run.Diagnostics);
        AssertAppModel(run, appType);
        run.AssertCompiles();
    }

    [Theory]
    [InlineData(AppType.WindowsAppSDK, AppType.UWP)]
    [InlineData(AppType.WindowsAppSDK, AppType.UnoWinUI)]
    [InlineData(AppType.UWP, AppType.WindowsAppSDK)]
    [InlineData(AppType.UWP, AppType.UnoWinUI)]
    [InlineData(AppType.UnoWinUI, AppType.WindowsAppSDK)]
    [InlineData(AppType.UnoWinUI, AppType.UWP)]
    public void AnExplicitSelectionTakesPrecedenceOverEveryDetectedModel(AppType detectedAppType, AppType selectedAppType)
    {
        var run = Run(detectedAppType, selectedAppType.ToString(), useUwp: true);

        Assert.Empty(run.Diagnostics);
        AssertAppModel(run, selectedAppType);
    }

    [Theory]
    [InlineData(AppType.WindowsAppSDK)]
    [InlineData(AppType.UWP)]
    [InlineData(AppType.UnoWinUI)]
    public void OverridesPreserveTheResourceInterfaceGenerationSetting(AppType appType)
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", SampleResources)],
            references: PlatformStubs.ReferencesWithDetectionHint(appType),
            appTypeOverride: appType.ToString(),
            generateResourceInterfaces: false);

        Assert.Empty(run.Diagnostics);
        Assert.DoesNotContain("public interface IResources", run.Source("Resources.resw"));
        Assert.DoesNotContain("public sealed class ResourcesProvider", run.Source("Resources.resw"));
        run.AssertCompiles();
    }

    [Theory]
    [InlineData(" windowsappsdk ", AppType.WindowsAppSDK)]
    [InlineData(" uwp ", AppType.UWP)]
    [InlineData(" unowinui ", AppType.UnoWinUI)]
    public void NamedSelectionsIgnoreCaseAndSurroundingWhitespace(string selection, AppType appType)
    {
        var run = Run(AppType.Unknown, selection, references: PlatformStubs.ReferencesWithDetectionHint(appType));

        Assert.Empty(run.Diagnostics);
        AssertAppModel(run, appType);
        run.AssertCompiles();
    }

    [Theory]
    [InlineData("Unknown")]
    [InlineData("Uno.WinUI")]
    [InlineData("WinAppSDK")]
    [InlineData("3")]
    [InlineData(" 1 ")]
    [InlineData("-1")]
    [InlineData("WindowsAppSDK, UWP")]
    [InlineData("Auto;UnoWinUI")]
    [InlineData("UnowinU\u0131")]
    public void InvalidSelectionsReportAnErrorInsteadOfFallingBack(string selection)
    {
        foreach (var detectedAppType in new[] { AppType.Unknown, AppType.WindowsAppSDK, AppType.UWP, AppType.UnoWinUI })
        {
            var run = Run(detectedAppType, selection, useUwp: true);
            var diagnostic = Assert.Single(run.Diagnostics);

            Assert.Equal("RESWP0021", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
            Assert.Contains("ReswPlusAppType", diagnostic.GetMessage());
            Assert.Contains("Auto, WindowsAppSDK, UWP, or UnoWinUI", diagnostic.GetMessage());
            Assert.Empty(run.Sources);
        }
    }

    [Fact]
    public void AnInvalidSelectionPreservesOtherSetupProblemsWithoutReportingUnrecognizedAppType()
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", SampleResources)],
            outputType: null,
            appTypeOverride: "Unknown");

        Assert.Equal(new[] { "RESWP0004", "RESWP0021" }, run.DiagnosticIds);
        Assert.Empty(run.Sources);
    }

    [Fact]
    public void AnOverrideDoesNotSupplyMissingPlatformReferences()
    {
        var run = Run(AppType.Unknown, "UnoWinUI");

        Assert.Empty(run.Diagnostics);
        AssertAppModel(run, AppType.UnoWinUI);
        Assert.Contains(run.OutputCompilation.GetDiagnostics(), diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Theory]
    [InlineData(null, "Auto")]
    [InlineData("", " auto ")]
    [InlineData(" \t ", "Auto")]
    [InlineData("WindowsAppSDK", " windowsappsdk ")]
    [InlineData("UWP", " uwp ")]
    [InlineData("UnoWinUI", " unowinui ")]
    [InlineData("Unknown", "AnotherInvalidValue")]
    public void EquivalentSelectionsCompareAndHashAlike(string? first, string? second)
    {
        var firstOptions = ReadOptions(first);
        var secondOptions = ReadOptions(second);

        Assert.Equal(firstOptions, secondOptions);
        Assert.Equal(firstOptions.GetHashCode(), secondOptions.GetHashCode());
    }

    [Theory]
    [InlineData("auto", "Auto")]
    [InlineData("windowsappsdk", "WindowsAppSDK")]
    [InlineData("uwp", "UWP")]
    [InlineData("unowinui", "UnoWinUI")]
    [InlineData("Unknown", "Invalid")]
    public void OptionsCaptureTheParsedSelection(string selection, string expected)
    {
        Assert.Equal(expected, ReadOptions(selection).AppTypeOverride.ToString());
    }

    [Fact]
    public void AChangedSelectionInvalidatesTheOptionsModel()
    {
        Assert.NotEqual(ReadOptions(null), ReadOptions("UnoWinUI"));
        Assert.NotEqual(ReadOptions("UnoWinUI"), ReadOptions("Unknown"));
    }

    [Fact]
    public void SelectionParsingIsIndependentOfTheCurrentCulture()
    {
        var previousCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
            Assert.Equal(AppTypeSelection.UnoWinUI, ReadOptions("unowinui").AppTypeOverride);
            Assert.Equal(AppTypeSelection.WindowsAppSDK, ReadOptions("windowsappsdk").AppTypeOverride);
            Assert.Equal(AppTypeSelection.Auto, ReadOptions("auto").AppTypeOverride);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Fact]
    public void SelectingTheDetectedModelDoesNotRegenerateSources()
    {
        var automatic = Run(AppType.WindowsAppSDK);
        var explicitSelection = automatic.RunAgainWithOptions(Options("WindowsAppSDK"));

        Assert.Empty(explicitSelection.Diagnostics);
        Assert.Equal(1, explicitSelection.RecomputedCount(TrackingNames.Options));
        explicitSelection.AssertReused(TrackingNames.CompilationInfo);
        explicitSelection.AssertReused(TrackingNames.Project);
        explicitSelection.AssertReused(TrackingNames.Generation);
        explicitSelection.AssertReused(TrackingNames.Support);
        AssertSameSources(automatic, explicitSelection);
    }

    [Fact]
    public void EquivalentSelectionSpellingsDoNotInvalidateThePipeline()
    {
        var before = Run(AppType.UnoWinUI, "UnoWinUI");
        var after = before.RunAgainWithOptions(Options(" unowinui "));

        after.AssertReused(TrackingNames.Options);
        after.AssertReused(TrackingNames.Project);
        after.AssertReused(TrackingNames.Generation);
        after.AssertReused(TrackingNames.Support);
        AssertSameSources(before, after);
    }

    [Fact]
    public void ChangingOnlyTheSelectionUpdatesEveryAppModelDependentOutput()
    {
        var before = Run(AppType.WindowsAppSDK);
        var after = before.RunAgainWithOptions(Options("UWP"));

        Assert.Empty(after.Diagnostics);
        Assert.Equal(1, after.RecomputedCount(TrackingNames.Options));
        after.AssertReused(TrackingNames.CompilationInfo);
        Assert.Equal(1, after.RecomputedCount(TrackingNames.Project));
        Assert.Equal(1, after.RecomputedCount(TrackingNames.Generation));
        Assert.Equal(1, after.RecomputedCount(TrackingNames.Support));
        AssertAppModel(after, AppType.UWP);
        Assert.NotEqual(before.Source("Resources.resw"), after.Source("Resources.resw"));
        Assert.NotEqual(before.Source("ResourceStringProvider"), after.Source("ResourceStringProvider"));
    }

    [Fact]
    public void InvalidatingAndRestoringTheSelectionRemovesAndRestoresAllSources()
    {
        var before = Run(AppType.UnoWinUI, "UnoWinUI");
        var invalid = before.RunAgainWithOptions(Options("Unknown"));
        Assert.Equal("RESWP0021", Assert.Single(invalid.Diagnostics).Id);
        Assert.Empty(invalid.Sources);

        var restored = invalid.RunAgainWithOptions(Options("UnoWinUI"));
        Assert.Empty(restored.Diagnostics);
        AssertSameSources(before, restored);
        restored.AssertCompiles();

        var automatic = restored.RunAgainWithOptions(Options(null));
        Assert.Empty(automatic.Diagnostics);
        AssertSameSources(before, automatic);
        automatic.AssertReused(TrackingNames.Generation);
    }

    private static ReswGeneratorRun Run(
        AppType appType,
        string? selection = null,
        bool useUwp = false,
        bool useApplicationLanguages = false,
        IEnumerable<MetadataReference>? references = null)
    {
        return ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", SampleResources)],
            appType,
            defaultLanguage: "en-US",
            useUwp: useUwp,
            useApplicationLanguages: useApplicationLanguages,
            references: references,
            appTypeOverride: selection);
    }

    private static Dictionary<string, string> Options(string? selection)
    {
        var options = new Dictionary<string, string>(AnalyzerConfigOptions.KeyComparer)
        {
            ["build_property.ProjectDir"] = ReswGeneratorHarness.ProjectDir,
            ["build_property.OutputType"] = "Library",
            ["build_property.RootNamespace"] = "TestProject",
            ["build_property.DefaultLanguage"] = "en-US",
        };
        if (selection is not null)
        {
            options["build_property.ReswPlusAppType"] = selection;
        }
        return options;
    }

    private static ReswBuildOptions ReadOptions(string? selection)
    {
        return ReswBuildOptions.Read(new TestAnalyzerConfigOptionsProvider(Options(selection)).GlobalOptions);
    }

    private static void AssertSameSources(ReswGeneratorRun first, ReswGeneratorRun second)
    {
        Assert.Equal(
            first.Sources.OrderBy(source => source.Key, StringComparer.Ordinal),
            second.Sources.OrderBy(source => source.Key, StringComparer.Ordinal));
    }

    private static void AssertAppModel(ReswGeneratorRun run, AppType appType)
    {
        var xamlNamespace = appType == AppType.UWP ? "Windows.UI.Xaml" : "Microsoft.UI.Xaml";
        var generated = run.Source("Resources.resw");
        Assert.Contains($"using {xamlNamespace}.Markup;", generated);
        Assert.Contains($"global::{xamlNamespace}.Markup.MarkupExtensionReturnType", generated);
        Assert.Contains($"global::{xamlNamespace}.Markup.MarkupExtension", generated);
        Assert.Contains($"global::{xamlNamespace}.Data.IValueConverter", generated);

        var provider = run.Source("ResourceStringProvider");
        Assert.Contains(
            appType == AppType.WindowsAppSDK
                ? "using Microsoft.Windows.ApplicationModel.Resources;"
                : "using Windows.ApplicationModel.Resources;",
            provider);
        Assert.Contains(
            appType == AppType.WindowsAppSDK
                ? "new ResourceLoader(ResourceLoader.GetDefaultResourceFilePath(), resourceMapId)"
                : "ResourceLoader.GetForViewIndependentUse(resourceMapId)",
            provider);
    }
}
