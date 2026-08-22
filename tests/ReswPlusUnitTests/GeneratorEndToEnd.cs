using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ReswPlus.SourceGenerator.ClassGenerators;
using ReswPlus.SourceGenerator.Pipeline;
using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Runs the generator the way the compiler runs it, and compiles what comes out.
/// </summary>
/// <remarks>
/// These tests are the only ones that cover <see cref="ReswPlus.SourceGenerator.ReswSourceGenerator"/> itself:
/// the MSBuild properties it reads, the kind of app it infers from the references of the compilation, the
/// namespace it derives from the folder of a resource, the language it picks to generate from, and the shared
/// support sources it has to emit exactly once per project. They also assert that the emitted code builds, at
/// the language version a UWP project uses by default, which is the guarantee a source generator lives or dies
/// by.
/// </remarks>
public class GeneratorEndToEnd
{
    /// <summary>
    /// A resource file exercising every kind of member the generator emits.
    /// </summary>
    private static string EveryFeature => ReswTestHelpers.CreateResw(
        ("Plain", "A plain string", null),
        ("Formatted", "Hello {0}, you are {1}", "#Format[String name, Int age]"),
        ("Named", "Hello {0}", "#Format[String userName]"),
        ("Literal", "{0} - {1}", "#Format[String name, \"a literal\"]"),
        ("WithMacro", "Today is {0}", "#Format[DATE]"),
        ("WithReference", "{0} and {1}", "#Format[String name, Plain]"),
        ("Items_One", "{0} item", "#Format[Plural itemCount]"),
        ("Items_Other", "{0} items", "#Format[Plural itemCount]"),
        ("Items_None", "no item", null),
        ("Message_Variant1", "She wrote", "#Format[Variant]"),
        ("Message_Variant2", "He wrote", "#Format[Variant]"));

    [Theory]
    [InlineData(ReswPlus.SourceGenerator.AppType.WindowsAppSDK)]
    [InlineData(ReswPlus.SourceGenerator.AppType.UWP)]
    public void TheGeneratedCodeCompiles(ReswPlus.SourceGenerator.AppType appType)
    {
        var run = ReswGeneratorHarness.Run([ReswGeneratorHarness.File("en-US", EveryFeature)], appType);

        Assert.Empty(run.Diagnostics.Where(diagnostic => diagnostic.Severity >= DiagnosticSeverity.Warning));
        run.AssertCompiles();
    }

    [Fact]
    public void ResourceInterfacesAreNotGeneratedByDefault()
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("Plain", "A plain string", null)))]);

        Assert.DoesNotContain("interface IResources", run.Source("Resources.resw"));
        Assert.Contains("public static class Resources", run.Source("Resources.resw"));
    }

    [Fact]
    public void ResourceInterfaceOptionIsReadFromTheBuild()
    {
        var options = new System.Collections.Generic.Dictionary<string, string>(AnalyzerConfigOptions.KeyComparer)
        {
            ["build_property.ReswPlusGenerateResourceInterfaces"] = "true",
        };

        var buildOptions = ReswBuildOptions.Read(new TestAnalyzerConfigOptionsProvider(options).GlobalOptions);

        Assert.True(buildOptions.GenerateResourceInterfaces);
    }

    [Fact]
    public void ResourceInterfaceCanBeGeneratedDirectly()
    {
        var generated = ReswTestHelpers.GenerateCode(
            ReswTestHelpers.CreateResw(("Plain", "A plain string", null)),
            generateResourceInterface: true);

        Assert.Contains("public interface IResources", generated);
    }

    [Theory]
    [InlineData(ReswPlus.SourceGenerator.AppType.WindowsAppSDK)]
    [InlineData(ReswPlus.SourceGenerator.AppType.UWP)]
    public void ResourceInterfacesCanBeInjectedWithoutChangingTheStaticApi(ReswPlus.SourceGenerator.AppType appType)
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", EveryFeature)],
            appType,
            generateResourceInterfaces: true);

        var generated = run.Source("Resources.resw");

        Assert.Contains("public interface IResources", generated);
        Assert.Contains("GeneratedCodeAttribute(\"ReswPlus\"", generated);
        Assert.Contains("public sealed class Resources : IResources", generated);
        Assert.Contains("string IResources.Plain => Resources.Plain;", generated);
        Assert.Contains("string IResources.Formatted(string name, int age) => Resources.Formatted(name, age);", generated);

        run.AssertCompilesWith(
            """
            namespace TestProject.Consumer
            {
                public sealed class ViewModel
                {
                    private readonly global::TestProject.Strings.IResources _resources;

                    public ViewModel(global::TestProject.Strings.IResources resources)
                    {
                        _resources = resources;
                    }

                    public string Greeting => _resources.Formatted("Ada", 37);
                }

                public static class Composition
                {
                    public static ViewModel Create() =>
                        new ViewModel(new global::TestProject.Strings.Resources());
                }
            }
            """);
    }

    [Fact]
    public void AResourceCannotCollideWithItsGeneratedInterface()
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("IResources", "A collision", null)))],
            generateResourceInterfaces: true);

        Assert.DoesNotContain("string IResources {", run.Source("Resources.resw"));
        run.AssertCompiles();
    }

    [Fact]
    public void TheGeneratedCodeCompilesWhenThePluralLanguageComesFromTheApplicationLanguages()
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", EveryFeature)],
            useApplicationLanguages: true);

        run.AssertCompiles();
    }

    [Theory]
    [InlineData(ReswPlus.SourceGenerator.AppType.WindowsAppSDK, false)]
    [InlineData(ReswPlus.SourceGenerator.AppType.WindowsAppSDK, true)]
    [InlineData(ReswPlus.SourceGenerator.AppType.UWP, false)]
    [InlineData(ReswPlus.SourceGenerator.AppType.UWP, true)]
    public void TheGeneratedCodeCompilesForEveryCombinationOfAppTypeAndLanguageSource(
        ReswPlus.SourceGenerator.AppType appType,
        bool useApplicationLanguages)
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", EveryFeature)],
            appType,
            useApplicationLanguages: useApplicationLanguages);

        run.AssertCompiles();
    }

    [Fact]
    public void AWindowsAppSdkProjectBindsToTheWindowsAppSdkFramework()
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("Plain", "A plain string", null)))],
            ReswPlus.SourceGenerator.AppType.WindowsAppSDK);

        Assert.Contains("using Microsoft.UI.Xaml.Markup;", run.Source("Resources.resw"));
        Assert.Contains("Microsoft.Windows.ApplicationModel.Resources", run.Source("ResourceStringProvider"));
    }

    [Fact]
    public void AUwpProjectBindsToTheUwpFramework()
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("Plain", "A plain string", null)))],
            ReswPlus.SourceGenerator.AppType.UWP);

        Assert.Contains("using Windows.UI.Xaml.Markup;", run.Source("Resources.resw"));
        Assert.Contains("GetForViewIndependentUse", run.Source("ResourceStringProvider"));
    }

    [Fact]
    public void AProjectThatIsNeitherUwpNorWindowsAppSdkIsReported()
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("Plain", "A plain string", null)))],
            ReswPlus.SourceGenerator.AppType.Unknown);

        Assert.Contains("RESWP0005", run.DiagnosticIds);
        Assert.Empty(run.Sources);
    }

    [Fact]
    public void AProjectWithoutARootNamespaceIsReported()
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("Plain", "A plain string", null)))],
            rootNamespace: null);

        Assert.Contains("RESWP0002", run.DiagnosticIds);
    }

    [Fact]
    public void AProjectWithoutARootPathIsReported()
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("Plain", "A plain string", null)))],
            projectDir: null);

        Assert.Contains("RESWP0003", run.DiagnosticIds);
    }

    [Fact]
    public void TheRootPathFallsBackToTheProjectFile()
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("Plain", "A plain string", null)))],
            projectDir: null,
            msBuildProjectFullPath: $@"{ReswGeneratorHarness.ProjectDir}TestProject.csproj");

        Assert.DoesNotContain("RESWP0003", run.DiagnosticIds);
        Assert.Contains("namespace TestProject.Strings", run.Source("Resources.resw"));
    }

    [Fact]
    public void AProjectThatDeclaresNeitherAnOutputTypeNorProjectTypeGuidsIsReported()
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("Plain", "A plain string", null)))],
            outputType: null);

        Assert.Contains("RESWP0004", run.DiagnosticIds);
    }

    [Fact]
    public void ALegacyProjectIsRecognizedAsALibraryThroughItsProjectTypeGuids()
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("Plain", "A plain string", null)))],
            outputType: null,
            projectTypeGuids: "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}",
            assemblyName: "MyLibrary");

        Assert.DoesNotContain("RESWP0004", run.DiagnosticIds);

        // A library addresses its resources through the name of the assembly holding them.
        Assert.Contains("\"MyLibrary/Resources\"", run.Source("Resources.resw"));
    }

    [Theory]
    [InlineData("Library", "\"MyLibrary/Resources\"")]
    [InlineData("WinExe", "\"Resources\"")]
    [InlineData("Exe", "\"Resources\"")]
    public void TheResourceMapOfALibraryIsQualifiedWithItsAssemblyName(string outputType, string expectedResourceMap)
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("Plain", "A plain string", null)))],
            outputType: outputType,
            assemblyName: "MyLibrary");

        Assert.Contains(expectedResourceMap, run.Source("Resources.resw"));
        run.AssertCompiles();
    }

    [Fact]
    public void TheNamespaceOfTheGeneratedClassFollowsTheFolderOfTheResource()
    {
        var run = ReswGeneratorHarness.Run(
        [
            ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("Plain", "A plain string", null)), folder: @"Assets\Text"),
        ]);

        Assert.Contains("namespace TestProject.Assets.Text", run.Source("Resources.resw"));
        run.AssertCompiles();
    }

    [Fact]
    public void TheDefaultLanguageOfTheProjectPicksTheResourceTheCodeIsGeneratedFrom()
    {
        var english = ReswTestHelpers.CreateResw(("OnlyInEnglish", "English", null));
        var french = ReswTestHelpers.CreateResw(("OnlyInFrench", "Français", null));

        var run = ReswGeneratorHarness.Run(
        [
            ReswGeneratorHarness.File("en-US", english),
            ReswGeneratorHarness.File("fr-FR", french),
        ],
        defaultLanguage: "fr-FR");

        var generated = run.Source("Resources.resw");

        Assert.Contains("OnlyInFrench", generated);
        Assert.DoesNotContain("OnlyInEnglish", generated);
        run.AssertCompiles();
    }

    [Fact]
    public void OnlyOneClassIsGeneratedForAResourceTranslatedInSeveralLanguages()
    {
        var run = ReswGeneratorHarness.Run(
        [
            ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("Plain", "A plain string", null))),
            ReswGeneratorHarness.File("fr-FR", ReswTestHelpers.CreateResw(("Plain", "Une chaîne", null))),
            ReswGeneratorHarness.File("de-DE", ReswTestHelpers.CreateResw(("Plain", "Eine Zeichenfolge", null))),
        ]);

        Assert.Single(run.Sources.Keys, name => name.Contains("Resources.resw"));
        run.AssertCompiles();
    }

    [Fact]
    public void AProjectHoldingSeveralResourcesGeneratesAClassForEachOfThem()
    {
        var run = ReswGeneratorHarness.Run(
        [
            ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("FromFirst", "First", null)), baseName: "Resources"),
            ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("FromSecond", "Second", null)), baseName: "Errors"),
        ]);

        Assert.Contains("FromFirst", run.Source("Resources.resw"));
        Assert.Contains("FromSecond", run.Source("Errors.resw"));
        run.AssertCompiles();
    }

    [Fact]
    public void TwoResourcesOfTheSameNameInDifferentFoldersDoNotCollide()
    {
        var run = ReswGeneratorHarness.Run(
        [
            ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("FromAssets", "Assets", null)), folder: "Assets"),
            ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("FromStrings", "Strings", null)), folder: "Strings"),
        ]);

        Assert.Equal(2, run.Sources.Keys.Count(name => name.Contains("Resources.resw")));
        Assert.Contains("namespace TestProject.Assets", run.Source("TestProject.Assets.en-US.Resources.resw"));
        Assert.Contains("namespace TestProject.Strings", run.Source("TestProject.Strings.en-US.Resources.resw"));
        run.AssertCompiles();
    }

    /// <summary>
    /// Covers the regression the generator guards against with its set of emitted hint names: the support
    /// sources are shared by every resource of a project, and emitting one of them twice used to make the
    /// generator produce nothing at all.
    /// </summary>
    [Fact]
    public void TheSharedSupportSourcesAreEmittedOnceForAProjectHoldingSeveralResources()
    {
        var first = ReswTestHelpers.CreateResw(
            ("WithMacro", "Today is {0}", "#Format[DATE]"),
            ("Items_One", "{0} item", "#Format[Plural itemCount]"),
            ("Items_Other", "{0} items", "#Format[Plural itemCount]"));

        var second = ReswTestHelpers.CreateResw(
            ("AlsoWithMacro", "The time is {0}", "#Format[TIME]"),
            ("Failures_One", "{0} error", "#Format[Plural errorCount]"),
            ("Failures_Other", "{0} errors", "#Format[Plural errorCount]"));

        var run = ReswGeneratorHarness.Run(
        [
            ReswGeneratorHarness.File("en-US", first, baseName: "Resources"),
            ReswGeneratorHarness.File("en-US", second, baseName: "Errors"),
        ]);

        Assert.Single(run.Sources.Keys, name => name.StartsWith("Macros", System.StringComparison.Ordinal));
        Assert.Single(run.Sources.Keys, name => name.StartsWith("ResourceLoaderExtension", System.StringComparison.Ordinal));
        Assert.Single(run.Sources.Keys, name => name.StartsWith("IPluralProvider", System.StringComparison.Ordinal));
        run.AssertCompiles();
    }

    [Fact]
    public void ThePluralSupportIsOnlyEmittedForAResourceThatUsesIt()
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("Plain", "A plain string", null)))]);

        Assert.DoesNotContain(run.Sources.Keys, name => name.StartsWith("ResourceLoaderExtension", System.StringComparison.Ordinal));
        Assert.DoesNotContain(run.Sources.Keys, name => name.StartsWith("Macros", System.StringComparison.Ordinal));
    }

    [Fact]
    public void ThePluralProviderOfEveryLanguageOfTheProjectIsEmitted()
    {
        var plural = ReswTestHelpers.CreateResw(
            ("Items_One", "{0} item", "#Format[Plural itemCount]"),
            ("Items_Other", "{0} items", "#Format[Plural itemCount]"));

        var run = ReswGeneratorHarness.Run(
        [
            ReswGeneratorHarness.File("en-US", plural),
            ReswGeneratorHarness.File("ru-RU", plural),
            ReswGeneratorHarness.File("cy-GB", plural),
        ]);

        var selector = run.Source("ResourceLoaderExtension");

        Assert.Contains("case \"en\":", selector);
        Assert.Contains("case \"ru\":", selector);
        Assert.Contains("case \"cy\":", selector);
        Assert.Contains(run.Sources.Keys, name => name.StartsWith(
            $"{PluralFormsRetriever.RetrievePluralFormForLanguage("ru")!.Id}Provider", System.StringComparison.Ordinal));
        Assert.Contains(run.Sources.Keys, name => name.StartsWith(
            $"{PluralFormsRetriever.RetrievePluralFormForLanguage("cy")!.Id}Provider", System.StringComparison.Ordinal));
        run.AssertCompiles();
    }

    [Fact]
    public void ALanguageWithoutPluralRulesIsReportedAgainstOneOfItsFiles()
    {
        var plural = ReswTestHelpers.CreateResw(
            ("Items_One", "{0} item", "#Format[Plural itemCount]"),
            ("Items_Other", "{0} items", "#Format[Plural itemCount]"));

        var run = ReswGeneratorHarness.Run(
        [
            ReswGeneratorHarness.File("en-US", plural),
            ReswGeneratorHarness.File("zz-ZZ", plural),
        ]);

        var reported = Assert.Single(run.Diagnostics, diagnostic => diagnostic.Id == "RESWP0011");

        Assert.Contains("zz", reported.GetMessage());
        Assert.Contains(@"zz-ZZ\Resources.resw", reported.Location.GetLineSpan().Path);
        run.AssertCompiles();
    }

    [Fact]
    public void AProjectWithoutAnyResourceGeneratesNothingAndReportsNothing()
    {
        var run = ReswGeneratorHarness.Run([]);

        Assert.Empty(run.Diagnostics.Where(diagnostic => diagnostic.Severity >= DiagnosticSeverity.Warning));
        Assert.DoesNotContain(run.Sources.Keys, name => name.Contains(".resw"));
    }

    [Fact]
    public void TheFilesTheGeneratorDoesNotOwnAreIgnored()
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("Plain", "A plain string", null)))],
            additionalFiles:
            [
                new ReswFile($@"{ReswGeneratorHarness.ProjectDir}readme.txt", "not a resource"),
                new ReswFile($@"{ReswGeneratorHarness.ProjectDir}Strings\en-US\Other.json", "{}"),
            ]);

        Assert.Single(run.Sources.Keys, name => name.Contains(".resw"));
        run.AssertCompiles();
    }

    [Fact]
    public void TheExtensionOfAResourceIsMatchedWhateverItsCase()
    {
        var run = ReswGeneratorHarness.Run(
            [new ReswFile($@"{ReswGeneratorHarness.ProjectDir}Strings\en-US\Resources.RESW", ReswTestHelpers.CreateResw(("Plain", "A plain string", null)))]);

        Assert.Contains(run.Sources.Keys, name => name.Contains("Resources.RESW"));
    }

    [Fact]
    public void RunningTheGeneratorTwiceOverTheSameProjectProducesTheSameSources()
    {
        var files = new[] { ReswGeneratorHarness.File("en-US", EveryFeature) };

        var first = ReswGeneratorHarness.Run(files);
        var second = ReswGeneratorHarness.Run(files);

        Assert.Equal(
            first.Sources.OrderBy(source => source.Key, System.StringComparer.Ordinal),
            second.Sources.OrderBy(source => source.Key, System.StringComparer.Ordinal));
    }

    [Fact]
    public void EditingAResourceIsReflectedInTheGeneratedSources()
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("Before", "Before", null)))]);

        Assert.Contains("Before", run.Source("Resources.resw"));

        var edited = run.RunAgain(
            [ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("After", "After", null)))]);

        Assert.Contains("After", edited.Source("Resources.resw"));
        Assert.DoesNotContain("Before", edited.Source("Resources.resw"));
        edited.AssertCompiles();
    }
}
