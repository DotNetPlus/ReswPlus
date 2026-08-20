using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Covers the markup extension being deprecated rather than removed.
/// </summary>
/// <remarks>
/// A markup extension is created by the XAML parser while it reads a page, which a UWP app compiled with
/// Native AOT cannot do: the page fails to load with "Markup extension could not provide value", and no
/// trimming directive keeps it working. It is still generated, so that an app that does not use Native AOT
/// keeps building, but it is marked so that the build says what to move to.
/// </remarks>
public class ObsoleteMarkupExtension
{
    private static string Generated => ReswTestHelpers.GenerateCode(
        ReswTestHelpers.CreateResw(
            ("Title", "A title", null),
            ("Greeting", "Hello {0}", "#Format[String name]")));

    [Fact]
    public void TheMarkupExtensionIsStillGenerated()
    {
        var generated = Generated;

        Assert.Contains("class ResourcesExtension", generated);
        Assert.Contains("ProvideValue", generated);
    }

    [Fact]
    public void TheMarkupExtensionIsMarkedObsolete()
    {
        Assert.Contains("[global::System.Obsolete(", Generated);
    }

    [Fact]
    public void TheReasonNamesNativeAotAndTheReplacement()
    {
        var generated = Generated;

        // The message is what a consumer reads in the build output, so it has to say why it is going and what
        // to write instead, not merely that it is deprecated.
        Assert.Contains("Native AOT", generated);
        Assert.Contains("x:Bind", generated);
        Assert.Contains("{x:Bind strings:Resources.Foo}", generated);
    }

    [Fact]
    public void TheStronglyTypedClassIsNotDeprecatedWithIt()
    {
        var generated = Generated;

        // Only the markup extension is going. The members it looked resources up through are what replaces it,
        // so deprecating them too would leave a consumer with nothing to move to.
        var classDeclaration = generated.IndexOf("class Resources", System.StringComparison.Ordinal);
        var extensionDeclaration = generated.IndexOf("class ResourcesExtension", System.StringComparison.Ordinal);
        var obsolete = generated.IndexOf("[global::System.Obsolete(", System.StringComparison.Ordinal);

        Assert.True(classDeclaration >= 0 && extensionDeclaration > classDeclaration);
        Assert.True(obsolete > classDeclaration, "The strongly typed class must not carry the obsolete attribute.");
    }

    [Fact]
    public void TheGeneratedCodeStillCompilesWithoutWarnings()
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("Title", "A title", null)))]);

        // Deprecating a type the generated file also uses would warn inside the generated file itself, which
        // lands in the consumer's build.
        run.AssertCompiles();
    }
}
