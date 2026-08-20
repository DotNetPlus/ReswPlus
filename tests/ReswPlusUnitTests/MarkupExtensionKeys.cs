using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Covers the markup extension surviving trimming and Native AOT.
/// </summary>
/// <remarks>
/// The markup extension looks a resource up by the name of the key it is given. Reading that name off the
/// generated enumeration with <c>ToString</c> only works while the names of the enumeration are still there,
/// and they are metadata that trimming and Native AOT are free to drop: the call then returns the numeric
/// value of the member, every lookup misses, and XAML fails with "Markup extension could not provide value"
/// at the point the page is created. Emitting the names as literals keeps them out of the trimmer's reach.
/// </remarks>
public class MarkupExtensionKeys
{
    private static string Generated => ReswTestHelpers.GenerateCode(
        ReswTestHelpers.CreateResw(
            ("Title", "A title", null),
            ("Greeting", "Hello {0}", "#Format[String name]")));

    [Fact]
    public void TheResourceNameIsNotReadOffTheEnumerationAtRuntime()
    {
        Assert.DoesNotContain("Key.ToString()", Generated);
    }

    [Theory]
    [InlineData("Title")]
    [InlineData("Greeting")]
    public void EveryKeyIsMappedToItsResourceNameAsALiteral(string key)
    {
        var generated = Generated;

        Assert.Contains($"case KeyEnum.{key}:", generated);
        Assert.Contains($"return \"{key}\";", generated);
    }

    [Fact]
    public void AKeyThatIdentifiesNoResourceReadsAsAnEmptyName()
    {
        // '_Undefined' is the member the enumeration starts with, and it names no resource.
        Assert.Contains("default:", Generated);
        Assert.Contains("return \"\";", Generated);
    }

    [Fact]
    public void TheMarkupExtensionLooksResourcesUpThroughTheGeneratedNames()
    {
        Assert.Contains("GetString(GetKeyName(Key))", Generated.Replace(" ", ""));
    }

    [Fact]
    public void TheGeneratedCodeStillCompiles()
    {
        var run = ReswGeneratorHarness.Run(
        [
            ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(
                ("Title", "A title", null),
                ("class", "A keyword named resource", null))),
        ]);

        run.AssertCompiles();
    }
}
