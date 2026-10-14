using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using ReswPlus.SourceGenerator;
using ReswPlus.SourceGenerator.Pipeline;
using Xunit;

namespace ReswPlusUnitTests;

public class AppTypeDetection
{
    private static ImmutableArray<MetadataReference> FrameworkReferences => PlatformStubs.ReferencesFor(AppType.Unknown);

    private static string SampleResources => ReswTestHelpers.CreateResw(
        ("Welcome", "Welcome!", null),
        ("Greeting", "Hello {0}", "#Format[String name]"),
        ("FileCount_None", "No files", null),
        ("FileCount_One", "{0} file", "#Format[Plural Int count]"),
        ("FileCount_Other", "{0} files", "#Format[Plural Int count]"));

    [Theory]
    [InlineData("interface", false, false)]
    [InlineData("interface", true, false)]
    [InlineData("class", false, false)]
    [InlineData("class", true, false)]
    [InlineData("interface", false, true)]
    [InlineData("interface", true, true)]
    [InlineData("class", false, true)]
    [InlineData("class", true, true)]
    public void UnoWinUIUsesWinUIHelpersAndWindowsResources(string typeKind, bool isLibrary, bool useApplicationLanguages)
    {
        var reference = CreatePlatformReference(AppType.UnoWinUI, typeKind);
        Assert.DoesNotContain("Uno", reference.Display!);

        var run = Run([reference], isLibrary, useApplicationLanguages);
        Assert.Empty(run.Diagnostics);

        var resources = run.Source("Resources.resw");
        Assert.Contains("using Microsoft.UI.Xaml.Markup;", resources);
        Assert.Contains("using Microsoft.UI.Xaml.Data;", resources);
        Assert.Contains(isLibrary ? "\"TestProject/Resources\"" : "\"Resources\"", resources);
        Assert.Contains("public static string Welcome", resources);
        Assert.Contains("public static string Greeting(string name)", resources);
        Assert.Contains("public static string FileCount(int count)", resources);
        Assert.Contains("[global::Microsoft.UI.Xaml.Markup.MarkupExtensionReturnType(ReturnType = typeof(string))]", resources);
        Assert.Contains("public global::Microsoft.UI.Xaml.Data.IValueConverter Converter", resources);
        Assert.Contains("public interface IResources", resources);
        Assert.Contains("public sealed class ResourcesProvider : IResources", resources);

        var provider = run.Source("ResourceStringProvider");
        Assert.Contains("using Windows.ApplicationModel.Resources;", provider);
        Assert.Contains("ResourceLoader.GetForViewIndependentUse(resourceMapId)", provider);
        var plurals = run.Source("ResourceLoaderExtension");
        Assert.Contains("GetPlural", plurals);
        if (useApplicationLanguages)
        {
            Assert.Contains("global::Windows.Globalization.ApplicationLanguages.Languages", plurals);
        }
        else
        {
            Assert.DoesNotContain("global::Windows.Globalization.ApplicationLanguages", plurals);
        }

        foreach (var source in run.Sources.Values)
        {
            Assert.DoesNotContain("Windows.UI.Xaml", source);
            Assert.DoesNotContain("Microsoft.Windows.ApplicationModel.Resources", source);
            Assert.DoesNotContain("Microsoft.Windows.Globalization", source);
        }

        run.AssertCompiles();
        AssertHelperTypes(run.OutputCompilation, "Microsoft.UI.Xaml", "Uno.UI");
    }

    [Theory]
    [InlineData(AppType.WindowsAppSDK, "Microsoft.UI.Xaml", "Microsoft.Windows.ApplicationModel.Resources")]
    [InlineData(AppType.UWP, "Windows.UI.Xaml", "Windows.ApplicationModel.Resources")]
    public void ExistingPlatformsKeepTheirHelpersAndResourceLoaders(AppType appType, string xamlNamespace, string resourceNamespace)
    {
        var run = Run([CreatePlatformReference(appType)]);

        Assert.Empty(run.Diagnostics);
        Assert.Equal(ReswTestHelpers.GenerateCode(SampleResources, appType), run.Source("Resources.resw"));
        var provider = run.Source("ResourceStringProvider");
        Assert.Contains($"using {resourceNamespace};", provider);
        Assert.Contains(
            appType == AppType.UWP
                ? "ResourceLoader.GetForViewIndependentUse(resourceMapId)"
                : "new ResourceLoader(ResourceLoader.GetDefaultResourceFilePath(), resourceMapId)",
            provider);
        run.AssertCompiles();
        AssertHelperTypes(run.OutputCompilation, xamlNamespace, appType == AppType.UWP ? "Windows.Foundation.UniversalApiContract" : "Microsoft.WinUI");
    }

    [Theory]
    [InlineData("Unrelated.UI", "Microsoft.UI.Xaml")]
    [InlineData("Uno.WinUI", "Microsoft.UI.Xaml")]
    [InlineData("Uno.UI", "Windows.UI.Xaml")]
    public void MatchingNamespaceOrPackageNameAloneDoesNotSelectUno(string assemblyName, string xamlNamespace)
    {
        var reference = CreateReference(assemblyName, $"namespace {xamlNamespace} {{ public interface DependencyObject {{ }} }}");

        AssertUnrecognized(Run([reference]));
    }

    [Fact]
    public void MissingDependencyObjectDoesNotSelectUno()
    {
        AssertUnrecognized(Run([CreateReference("Uno.UI", "public class UnrelatedType { }")]));
    }

    [Fact]
    public void AmbiguousDependencyObjectDoesNotSelectUno()
    {
        const string source = "namespace Microsoft.UI.Xaml { public interface DependencyObject { } }";
        var run = Run([CreateReference("Uno.UI", source), CreateReference("Unrelated.UI", source)]);

        Assert.Null(run.OutputCompilation.GetTypeByMetadataName("Microsoft.UI.Xaml.DependencyObject"));
        AssertUnrecognized(run);
    }

    [Theory]
    [InlineData(AppType.WindowsAppSDK)]
    [InlineData(AppType.UWP)]
    public void UnoWinUIDetectionTakesPrecedenceOverReferencePathHints(AppType hintedAppType)
    {
        var hintedReference = CreateReference("PathHint", "public class UnrelatedType { }", GetReferencePath(hintedAppType));
        var run = Run([CreatePlatformReference(AppType.UnoWinUI), hintedReference]);

        Assert.Empty(run.Diagnostics);
        Assert.Contains("using Microsoft.UI.Xaml.Markup;", run.Source("Resources.resw"));
        Assert.Contains("using Windows.ApplicationModel.Resources;", run.Source("ResourceStringProvider"));
    }

    [Theory]
    [InlineData(AppType.WindowsAppSDK, false)]
    [InlineData(AppType.WindowsAppSDK, true)]
    [InlineData(AppType.UWP, false)]
    [InlineData(AppType.UWP, true)]
    public void RejectedUnoTypesPreserveExistingFallbacks(AppType appType, bool ambiguous)
    {
        var references = ImmutableArray.Create<MetadataReference>(CreatePlatformReference(appType));
        if (ambiguous)
        {
            const string source = "namespace Microsoft.UI.Xaml { public interface DependencyObject { } }";
            references = references.Add(CreateReference("Uno.UI", source)).Add(CreateReference("Unrelated.UI", source));
        }
        else
        {
            references = references.Add(CreateReference("Uno.UI", "public class UnrelatedType { }"));
        }

        var run = Run(references);

        Assert.Empty(run.Diagnostics);
        Assert.Equal(ReswTestHelpers.GenerateCode(SampleResources, appType), run.Source("Resources.resw"));
        Assert.Contains(
            appType == AppType.UWP ? "using Windows.ApplicationModel.Resources;" : "using Microsoft.Windows.ApplicationModel.Resources;",
            run.Source("ResourceStringProvider"));
    }

    [Fact]
    public void WinAppSDKStillTakesPrecedenceOverUWPReferencePath()
    {
        var run = Run([
            CreatePlatformReference(AppType.WindowsAppSDK),
            CreateReference("UwpPathHint", "public class UnrelatedType { }", GetReferencePath(AppType.UWP))]);

        Assert.Empty(run.Diagnostics);
        Assert.Equal(ReswTestHelpers.GenerateCode(SampleResources), run.Source("Resources.resw"));
        Assert.Contains("using Microsoft.Windows.ApplicationModel.Resources;", run.Source("ResourceStringProvider"));
    }

    [Fact]
    public void ChangingReferencesInvalidatesTheIncrementalAppType()
    {
        var before = Run([CreatePlatformReference(AppType.WindowsAppSDK)]);
        Assert.Contains("using Microsoft.Windows.ApplicationModel.Resources;", before.Source("ResourceStringProvider"));

        var after = before.RunAgainWithReferences(FrameworkReferences.Add(CreatePlatformReference(AppType.UnoWinUI)));

        Assert.Empty(after.Diagnostics);
        Assert.Contains("using Windows.ApplicationModel.Resources;", after.Source("ResourceStringProvider"));
        Assert.Equal(before.Source("Resources.resw"), after.Source("Resources.resw"));
        Assert.Equal(1, after.RecomputedCount(TrackingNames.CompilationInfo));
        Assert.Equal(1, after.RecomputedCount(TrackingNames.Project));
        Assert.Equal(1, after.RecomputedCount(TrackingNames.Support));
        after.AssertCompiles();

        Assert.NotEqual(
            new CompilationInfo(true, AppType.WindowsAppSDK, "TestProject"),
            new CompilationInfo(true, AppType.UnoWinUI, "TestProject"));
        Assert.Equal(
            new CompilationInfo(true, AppType.UnoWinUI, "TestProject"),
            new CompilationInfo(true, AppType.UnoWinUI, "TestProject"));
    }

    private static void AssertHelperTypes(Compilation compilation, string xamlNamespace, string assemblyName)
    {
        var extension = compilation.GetTypeByMetadataName("TestProject.Strings.ResourcesExtension");
        Assert.NotNull(extension);
        Assert.Equal($"{xamlNamespace}.Markup.MarkupExtension", extension.BaseType!.ToDisplayString());
        Assert.Equal(assemblyName, extension.BaseType.ContainingAssembly.Identity.Name);
        var converter = Assert.IsAssignableFrom<IPropertySymbol>(Assert.Single(extension.GetMembers("Converter")));
        Assert.Equal($"{xamlNamespace}.Data.IValueConverter", converter.Type.ToDisplayString());
        Assert.Equal(assemblyName, converter.Type.ContainingAssembly.Identity.Name);
        Assert.Contains(extension.GetAttributes(), attribute =>
            attribute.AttributeClass?.ToDisplayString() == $"{xamlNamespace}.Markup.MarkupExtensionReturnTypeAttribute" &&
            attribute.AttributeClass.ContainingAssembly.Identity.Name == assemblyName);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UseUwpDoesNotOverrideUnoWinUIReferences(bool useUwp)
    {
        var run = Run([CreatePlatformReference(AppType.UnoWinUI)], useUwp: useUwp);

        Assert.Empty(run.Diagnostics);
        AssertHelperTypes(run.OutputCompilation, "Microsoft.UI.Xaml", "Uno.UI");
        run.AssertCompiles();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UnoWinUIHonorsResourceInterfaceGeneration(bool generateResourceInterfaces)
    {
        var run = Run([CreatePlatformReference(AppType.UnoWinUI)], generateResourceInterfaces: generateResourceInterfaces);
        var resources = run.Source("Resources.resw");

        Assert.Empty(run.Diagnostics);
        Assert.Equal(generateResourceInterfaces, resources.Contains("public interface IResources"));
        Assert.Equal(generateResourceInterfaces, resources.Contains("public sealed class ResourcesProvider : IResources"));
        run.AssertCompiles();
    }

    private static void AssertUnrecognized(ReswGeneratorRun run)
    {
        Assert.Equal("RESWP0005", Assert.Single(run.Diagnostics).Id);
        Assert.Empty(run.Sources);
    }

    private static ReswGeneratorRun Run(
        ImmutableArray<MetadataReference> references,
        bool isLibrary = false,
        bool useApplicationLanguages = false,
        bool? useUwp = null,
        bool? generateResourceInterfaces = null)
    {
        return ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", SampleResources)],
            outputType: isLibrary ? "Library" : "WinExe",
            defaultLanguage: "en-US",
            useApplicationLanguages: useApplicationLanguages,
            useUwp: useUwp,
            generateResourceInterfaces: generateResourceInterfaces,
            references: FrameworkReferences.AddRange(references),
            projectDir: null,
            msBuildProjectFullPath: @"C:\Project\TestProject.csproj");
    }

    private static MetadataReference CreateReference(string assemblyName, string source, string? filePath = null)
    {
        return PlatformStubs.Compile(assemblyName, source, filePath ?? @"C:\References\Platform.dll");
    }

    private static string GetReferencePath(AppType appType)
    {
        return appType switch
        {
            AppType.WindowsAppSDK => @"C:\Packages\Microsoft.WindowsAppSdk\Platform.dll",
            AppType.UWP => @"C:\References\Windows.Foundation.UniversalApiContract.winmd",
            _ => @"C:\References\Platform.dll"
        };
    }

    private static MetadataReference CreatePlatformReference(AppType appType, string typeKind = "class")
    {
        var xamlNamespace = appType == AppType.UWP ? "Windows.UI.Xaml" : "Microsoft.UI.Xaml";
        var resourceNamespace = appType == AppType.WindowsAppSDK
            ? "Microsoft.Windows.ApplicationModel.Resources"
            : "Windows.ApplicationModel.Resources";
        var assemblyName = appType switch
        {
            AppType.WindowsAppSDK => "Microsoft.WinUI",
            AppType.UWP => "Windows.Foundation.UniversalApiContract",
            _ => "Uno.UI"
        };
        var source = $$"""
            namespace {{xamlNamespace}}
            {
                public {{typeKind}} DependencyObject { }
            }
            namespace {{xamlNamespace}}.Markup
            {
                public abstract class MarkupExtension
                {
                    protected abstract object ProvideValue();
                }
                public sealed class MarkupExtensionReturnTypeAttribute : System.Attribute
                {
                    public System.Type ReturnType { get; set; }
                }
            }
            namespace {{xamlNamespace}}.Data
            {
                public interface IValueConverter
                {
                    object Convert(object value, System.Type targetType, object parameter, string language);
                    object ConvertBack(object value, System.Type targetType, object parameter, string language);
                }
            }
            namespace {{resourceNamespace}}
            {
                public sealed class ResourceLoader
                {
                    {{(appType == AppType.WindowsAppSDK ? """
                    public ResourceLoader(string filePath, string resourceMapId) { }
                    public static string GetDefaultResourceFilePath() => "";
                    """ : """
                    public static ResourceLoader GetForViewIndependentUse(string resourceMapId) => new ResourceLoader();
                    """)}}
                    public string GetString(string resourceId) => resourceId;
                }
            }
            namespace Windows.Globalization
            {
                public static class ApplicationLanguages
                {
                    public static System.Collections.Generic.IReadOnlyList<string> Languages { get; set; }
                }
            }
            """;

        return CreateReference(assemblyName, source, GetReferencePath(appType));
    }
}
