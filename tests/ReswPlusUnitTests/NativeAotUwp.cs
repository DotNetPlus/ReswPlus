using System.Linq;
using ReswPlus.SourceGenerator;
using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Covers the UWP projects that are built for Native AOT.
/// </summary>
/// <remarks>
/// A UWP project is otherwise recognized by the <c>Windows.Foundation.UniversalApiContract</c> reference it
/// carries. A UWP project built for Native AOT has the UWP types available but no reference of that name, so it
/// looked to the generator like no supported app type at all: it got RESWP0005 and no code, with nothing
/// pointing at what to do about it. Such a project says what it is with the <c>UseUwp</c> property instead.
/// </remarks>
public class NativeAotUwp
{
    private static string Sample => ReswTestHelpers.CreateResw(
        ("Plain", "A plain string", null),
        ("Formatted", "Hello {0}", "#Format[String name]"),
        ("Items_One", "{0} item", "#Format[Plural itemCount]"),
        ("Items_Other", "{0} items", "#Format[Plural itemCount]"));

    /// <summary>
    /// The state of things before <c>UseUwp</c> was read: this is what a Native AOT UWP project used to get.
    /// </summary>
    [Fact]
    public void AProjectWithoutTheApiContractReferenceIsNotRecognizedOnItsOwn()
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", Sample)],
            nativeAotUwp: true);

        Assert.Contains("RESWP0005", run.DiagnosticIds);
        Assert.DoesNotContain(run.Sources.Keys, name => name.Contains(".resw"));
    }

    [Fact]
    public void AProjectThatDeclaresUseUwpIsRecognizedWithoutTheApiContractReference()
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", Sample)],
            nativeAotUwp: true,
            useUwp: true);

        Assert.DoesNotContain("RESWP0005", run.DiagnosticIds);
        Assert.Contains("Plain", run.Source("Resources.resw"));
        run.AssertCompiles();
    }

    [Fact]
    public void AProjectThatDeclaresUseUwpBindsToTheUwpFramework()
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", Sample)],
            nativeAotUwp: true,
            useUwp: true);

        // The markup extension has to derive from the UWP one, and the resources have to be read through the
        // UWP resource loader, not the Windows App SDK one.
        Assert.Contains("using Windows.UI.Xaml.Markup;", run.Source("Resources.resw"));
        Assert.Contains("global::Windows.UI.Xaml.Markup.MarkupExtension", run.Source("Resources.resw"));
        Assert.Contains("GetForViewIndependentUse", run.Source("ResourceStringProvider"));
        run.AssertCompiles();
    }

    /// <summary>
    /// The property says what the references don't; it doesn't contradict them.
    /// </summary>
    /// <remarks>
    /// A project whose references positively identify it as a Windows App SDK app needs the Windows App SDK
    /// types, so letting a stray <c>UseUwp</c> take it for a UWP app would generate code against types it
    /// doesn't have.
    /// </remarks>
    [Fact]
    public void UseUwpDoesNotOverrideReferencesThatIdentifyTheProject()
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", Sample)],
            AppType.WindowsAppSDK,
            useUwp: true);

        Assert.Contains("using Microsoft.UI.Xaml.Markup;", run.Source("Resources.resw"));
        Assert.DoesNotContain("using Windows.UI.Xaml.Markup;", run.Source("Resources.resw"));
        run.AssertCompiles();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    public void AProjectThatDoesNotDeclareUseUwpStillFollowsItsReferences(bool? useUwp)
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", Sample)],
            AppType.WindowsAppSDK,
            useUwp: useUwp);

        Assert.Contains("using Microsoft.UI.Xaml.Markup;", run.Source("Resources.resw"));
        run.AssertCompiles();
    }

    [Fact]
    public void ThePluralSupportOfANativeAotUwpProjectReadsTheApplicationLanguages()
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", Sample)],
            nativeAotUwp: true,
            useUwp: true,
            useApplicationLanguages: true);

        // The Windows App SDK resolver reads an override this project has no API for, so opting in has to give
        // a Native AOT UWP project the UWP resolver.
        Assert.Contains("global::Windows.Globalization.ApplicationLanguages.Languages", run.Source("ResourceLoaderExtension"));
        Assert.DoesNotContain("Microsoft.Windows.Globalization", run.Source("ResourceLoaderExtension"));
        run.AssertCompiles();
    }
}
