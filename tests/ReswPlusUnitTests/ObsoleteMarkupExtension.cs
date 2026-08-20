using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReswPlus.SourceGenerator;
using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Covers the markup extension being deprecated rather than removed.
/// </summary>
/// <remarks>
/// A markup extension is created by the XAML parser while it reads a page, which a UWP app compiled with
/// Native AOT cannot do: the page fails to load with "Markup extension could not provide value", and no
/// trimming directive keeps it working. It is still generated, so an app that does not use Native AOT keeps
/// building, but it is marked so that the build says what to move to.
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
        Assert.True(IsObsolete(Generated, "ResourcesExtension"));
    }

    /// <summary>
    /// Only the markup extension is deprecated.
    /// </summary>
    /// <remarks>
    /// The members of the strongly typed class are what replaces it, so deprecating them with it would leave a
    /// consumer nothing to move to. The class is read out of the syntax tree rather than searched for in the
    /// text, because the name of the extension starts with the name of the class, and because an attribute on
    /// a member of the class sits at a position that reads the same as one on the extension.
    /// </remarks>
    [Fact]
    public void TheStronglyTypedClassIsNotDeprecatedWithIt()
    {
        var generated = Generated;

        Assert.False(IsObsolete(generated, "Resources"), "The strongly typed class must not be deprecated.");
        Assert.Empty(ObsoleteAttributesOfMembersOf(generated, "Resources"));
    }

    [Fact]
    public void TheReasonNamesNativeAotAndTheReplacement()
    {
        // Read off the attribute itself: the documentation comment of the class says something similar, so
        // searching the whole file would pass on that alone.
        var reason = ObsoleteReasonOf(Generated, "ResourcesExtension");

        Assert.Contains("Native AOT", reason);
        Assert.Contains("{strings:Resources Key=Foo}", reason);
        Assert.Contains("{x:Bind strings:Resources.Foo}", reason);
    }

    [Theory]
    [InlineData(AppType.WindowsAppSDK)]
    [InlineData(AppType.UWP)]
    public void TheGeneratedCodeStillCompilesWithoutWarnings(AppType appType)
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("Title", "A title", null)))],
            appType);

        // Deprecating a type the generated file also uses would warn inside the generated file itself, which
        // lands in the consumer's build.
        run.AssertCompiles();
    }

    /// <summary>
    /// What a consumer who keeps using it actually sees.
    /// </summary>
    /// <remarks>
    /// This is the point of deprecating it rather than removing it, and none of the assertions above reach it:
    /// nothing in the generated file names the markup extension, so that file compiling cleanly says nothing
    /// about what happens when something else names it.
    /// </remarks>
    [Fact]
    public void AConsumerThatStillUsesItIsWarned()
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("Title", "A title", null)))]);

        var consumer = CSharpSyntaxTree.ParseText("""
            namespace Consumer
            {
                public static class UsesTheExtension
                {
                    public static object Provide()
                    {
                        return new global::TestProject.Strings.ResourcesExtension();
                    }
                }
            }
            """,
            // The generated sources are parsed at the language version a UWP project uses by default, and a
            // compilation cannot mix versions.
            new CSharpParseOptions(LanguageVersion.CSharp7_3, DocumentationMode.None));

        using var peStream = new MemoryStream();
        var diagnostics = run.OutputCompilation.AddSyntaxTrees(consumer).Emit(peStream).Diagnostics;

        var obsolete = Assert.Single(diagnostics, diagnostic => diagnostic.Id == "CS0618");

        Assert.Contains("Native AOT", obsolete.GetMessage());
        Assert.Contains("x:Bind", obsolete.GetMessage());
    }

    private static ClassDeclarationSyntax ClassNamed(string generated, string name)
    {
        var classes = CSharpSyntaxTree.ParseText(generated)
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>();

        return Assert.Single(classes, declaration => declaration.Identifier.Text == name);
    }

    private static AttributeSyntax[] ObsoleteAttributesOf(SyntaxNode node) =>
        [.. node.ChildNodes()
            .OfType<AttributeListSyntax>()
            .SelectMany(list => list.Attributes)
            .Where(attribute => attribute.Name.ToString().EndsWith("Obsolete"))];

    private static bool IsObsolete(string generated, string className) =>
        ObsoleteAttributesOf(ClassNamed(generated, className)).Length != 0;

    private static AttributeSyntax[] ObsoleteAttributesOfMembersOf(string generated, string className) =>
        [.. ClassNamed(generated, className).Members.SelectMany(ObsoleteAttributesOf)];

    private static string ObsoleteReasonOf(string generated, string className)
    {
        var attribute = Assert.Single(ObsoleteAttributesOf(ClassNamed(generated, className)));
        var argument = Assert.Single(attribute.ArgumentList!.Arguments);

        return Assert.IsType<LiteralExpressionSyntax>(argument.Expression).Token.ValueText;
    }
}
