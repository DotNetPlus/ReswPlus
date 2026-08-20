using System.Linq;
using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Covers the names a resource file can hold that C# cannot take verbatim.
/// </summary>
/// <remarks>
/// The names in a <c>.resw</c> file are written by whoever writes the strings, and nothing about the format
/// restricts them to what C# accepts as an identifier. Every case here used to make the generator emit source
/// that does not build, which breaks the build of the consumer with an error pointing at generated code rather
/// than at the resource that caused it.
/// </remarks>
public class GeneratedIdentifiers
{
    [Theory]
    [InlineData("class")]
    [InlineData("event")]
    [InlineData("return")]
    [InlineData("namespace")]
    [InlineData("string")]
    [InlineData("int")]
    [InlineData("static")]
    [InlineData("base")]
    [InlineData("this")]
    [InlineData("null")]
    public void AResourceNamedAfterAKeywordIsGeneratedAsAnEscapedIdentifier(string keyword)
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw((keyword, "A value", null)))]);

        var generated = run.Source("Resources.resw");

        Assert.Contains($"public static string @{keyword}", generated);

        // The resource is still looked up under the name it is declared with, and the markup extension still
        // resolves it through the name of its enumeration member, neither of which carries the escape.
        Assert.Contains($"GetString(\"{keyword}\")", generated);

        run.AssertCompiles();
    }

    [Fact]
    public void AFormatParameterNamedAfterAKeywordIsGeneratedAsAnEscapedIdentifier()
    {
        var run = ReswGeneratorHarness.Run(
        [
            ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(
                ("Greeting", "Hello {0}, from {1}", "#Format[String namespace, Int int]"))),
        ]);

        var generated = run.Source("Resources.resw");

        Assert.Contains("string @namespace", generated);
        Assert.Contains("int @int", generated);
        run.AssertCompiles();
    }

    [Theory]
    [InlineData("Resources")]
    [InlineData("GetString")]
    [InlineData("_resourceStringProvider")]
    [InlineData("_Undefined")]
    [InlineData("KeyEnum")]
    public void AResourceNamedAfterAGeneratedMemberIsSkippedAndReported(string reservedName)
    {
        var run = ReswGeneratorHarness.Run(
        [
            ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(
                (reservedName, "A value", null),
                ("Usable", "Another value", null))),
        ]);

        var generated = run.Source("Resources.resw");

        Assert.Contains("Usable", generated);

        // No member looks the resource up, which is what says it was not generated. Asserting on the name
        // alone would not do: the class legitimately declares a GetString of its own, and names the resource
        // file after the class.
        Assert.DoesNotContain($"GetString(\"{reservedName}\")", generated);
        run.AssertCompiles();

        var reported = Assert.Single(ReswTestHelpers.Analyze(null, ("en-US", ReswTestHelpers.CreateResw(
            (reservedName, "A value", null),
            ("Usable", "Another value", null)))));

        Assert.Equal("RESWP0012", reported.Id);
        Assert.Contains(reservedName, reported.GetMessage());
    }

    [Fact]
    public void APluralizedResourceNamedAfterAGeneratedMemberIsSkipped()
    {
        var run = ReswGeneratorHarness.Run(
        [
            ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(
                ("GetString_One", "{0} item", "#Format[Plural count]"),
                ("GetString_Other", "{0} items", "#Format[Plural count]"),
                ("Usable", "Another value", null))),
        ]);

        var generated = run.Source("Resources.resw");

        Assert.Contains("Usable", generated);

        // Neither the pluralized member nor the forms it is declined from may leak back in as plain members.
        Assert.DoesNotContain("GetString_One", generated);
        run.AssertCompiles();
    }

    [Fact]
    public void AResourceIsNotSkippedWhenItOnlyDiffersFromAGeneratedMemberByCase()
    {
        var run = ReswGeneratorHarness.Run(
            [ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("getString", "A value", null)))]);

        // C# member lookup is case sensitive, so this one sits beside the generated GetString without
        // conflicting with it, and taking it away would be a silent loss.
        Assert.Contains("string getString", run.Source("Resources.resw"));
        run.AssertCompiles();
    }

    /// <summary>
    /// The generator adds a parameter of its own to a varianted resource, and used to add it under a name the
    /// tag was free to have taken already.
    /// </summary>
    [Fact]
    public void AVariantedResourceWhoseTagAlreadyUsesTheNameOfTheAddedParameterCompiles()
    {
        var run = ReswGeneratorHarness.Run(
        [
            ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(
                ("Message_Variant1", "She wrote {0}", "#Format[String variantId]"),
                ("Message_Variant2", "He wrote {0}", "#Format[String variantId]"))),
        ]);

        run.AssertCompiles();
    }

    /// <summary>
    /// The same, for the quantity a pluralized resource is declined by.
    /// </summary>
    [Fact]
    public void APluralizedResourceWhoseTagAlreadyUsesTheNameOfTheAddedParameterCompiles()
    {
        var run = ReswGeneratorHarness.Run(
        [
            ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(
                ("Items_One", "{0} item", "#Format[String pluralizationReferenceNumber]"),
                ("Items_Other", "{0} items", "#Format[String pluralizationReferenceNumber]"))),
        ]);

        run.AssertCompiles();
    }

    [Fact]
    public void ATagThatDeclaresTheSameParameterTwiceCompilesAndIsReported()
    {
        var resw = ReswTestHelpers.CreateResw(
            ("Greeting", "Hello {0} and {1}", "#Format[String user, String user]"));

        var run = ReswGeneratorHarness.Run([ReswGeneratorHarness.File("en-US", resw)]);

        var generated = run.Source("Resources.resw");

        // The order of the arguments is what the value depends on, so the duplicate is renamed rather than
        // dropped.
        Assert.Contains("string user, string user2", generated);
        run.AssertCompiles();

        var reported = Assert.Single(ReswTestHelpers.Analyze(null, ("en-US", resw)));

        Assert.Equal("RESWP0013", reported.Id);
        Assert.Contains("user", reported.GetMessage());
    }

    [Fact]
    public void ATagThatDeclaresEachParameterOnceIsNotReported()
    {
        var resw = ReswTestHelpers.CreateResw(
            ("Greeting", "Hello {0} and {1}", "#Format[String user, String other]"),
            ("Items_One", "{0} item", "#Format[Plural count]"),
            ("Items_Other", "{0} items", "#Format[Plural count]"));

        Assert.Empty(ReswTestHelpers.Analyze(null, ("en-US", resw)));
    }

    /// <summary>
    /// A varianted resource whose tag takes the name the generator adds is not the author's mistake, and the
    /// generator resolves it on its own.
    /// </summary>
    [Fact]
    public void AParameterThatOnlyClashesWithTheAddedOneIsNotReported()
    {
        var resw = ReswTestHelpers.CreateResw(
            ("Message_Variant1", "She wrote {0}", "#Format[String variantId]"),
            ("Message_Variant2", "He wrote {0}", "#Format[String variantId]"));

        Assert.DoesNotContain("RESWP0013", ReswTestHelpers.Analyze(null, ("en-US", resw)).Select(d => d.Id));
    }

    [Fact]
    public void TheGeneratedTypesDoNotBindToTypesOfTheConsumer()
    {
        var run = ReswGeneratorHarness.Run(
        [
            ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(
                ("Plain", "A value", null),
                ("Message_Variant1", "She wrote", null),
                ("Message_Variant2", "He wrote", null))),
        ]);

        var generated = run.Source("Resources.resw");

        // The generated types are emitted inside the namespace of the consumer, where a type of the consumer
        // takes precedence over one a using directive brings in.
        Assert.Contains("global::Microsoft.UI.Xaml.Markup.MarkupExtension", generated);
        Assert.Contains("global::Microsoft.UI.Xaml.Data.IValueConverter", generated);
        Assert.Contains("global::_ReswPlus_AutoGenerated.ResourceStringProvider", generated);
        Assert.Contains("global::System.Convert.ToInt64", generated);

        Assert.DoesNotContain(": MarkupExtension", generated);
        Assert.DoesNotContain("public IValueConverter", generated);
    }

    [Fact]
    public void TheGeneratedCodeCompilesBesideConsumerTypesOfTheSameName()
    {
        var run = ReswGeneratorHarness.Run(
        [
            ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(
                ("Plain", "A value", null),
                ("Message_Variant1", "She wrote", null),
                ("Message_Variant2", "He wrote", null))),
        ]);

        // Types named after the ones the generated code binds to, in the very namespace it is emitted into.
        run.AssertCompilesWith("""
            namespace TestProject.Strings
            {
                public class MarkupExtension { }
                public interface IValueConverter { }
                public static class Convert { }
            }
            """);
    }

    /// <summary>
    /// A resource file the generator cannot read must not take the rest of the project down with it.
    /// </summary>
    [Fact]
    public void AMalformedResourceFileIsReportedAndTheOtherResourcesAreStillGenerated()
    {
        var run = ReswGeneratorHarness.Run(
        [
            ReswGeneratorHarness.File("en-US", "<root><data name=\"Broken\"", baseName: "Broken"),
            ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("Plain", "A value", null)), baseName: "Resources"),
        ]);

        var reported = Assert.Single(run.Diagnostics, diagnostic => diagnostic.Id == "RESWP0014");

        Assert.Contains("Broken.resw", reported.GetMessage());
        Assert.Contains(@"Broken.resw", reported.Location.GetLineSpan().Path);

        // The resource that is readable is still generated, and still compiles.
        Assert.Contains("Plain", run.Source("Resources.resw"));
        run.AssertCompiles();
    }
}
