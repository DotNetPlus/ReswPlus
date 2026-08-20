using System.Linq;
using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Covers the resource files a project shares with another one.
/// </summary>
/// <remarks>
/// Two apps built from the same sources -- the same app for two toolchains, for instance -- keep their strings
/// in one place and link them into both projects. The compiler hands such a file over the way it is written in
/// the project, as a path reaching out of the project directory, which used to end up in the namespace of the
/// generated class: a resource shared as <c>..\Shared\Strings\en-US\Resources.resw</c> was generated into the
/// namespace <c>RootNamespace....Shared.Strings</c>, which is not a namespace at all and does not compile.
/// </remarks>
public class SharedResources
{
    private static string Sample => ReswTestHelpers.CreateResw(
        ("Plain", "A plain string", null),
        ("Formatted", "Hello {0}", "#Format[String name]"));

    /// <summary>
    /// A resource file reached through a relative path that climbs out of the project directory.
    /// </summary>
    private static ReswFile SharedFile(string language = "en-US") =>
        new($@"{ReswGeneratorHarness.ProjectDir}..\Shared\Strings\{language}\Resources.resw", Sample);

    [Fact]
    public void AResourceSharedFromOutsideTheProjectTakesTheRootNamespaceOfTheProject()
    {
        var run = ReswGeneratorHarness.Run([SharedFile()]);

        Assert.Contains("namespace TestProject\r\n", run.Source("Resources.resw").Replace("\n", "\r\n").Replace("\r\r", "\r"));
        Assert.DoesNotContain("..", run.Source("Resources.resw"));
    }

    [Fact]
    public void TheCodeGeneratedForASharedResourceCompiles()
    {
        var run = ReswGeneratorHarness.Run([SharedFile()]);

        Assert.Empty(run.Diagnostics);
        run.AssertCompiles();
    }

    [Fact]
    public void TheHintNameOfASharedResourceHoldsNoPathSegment()
    {
        var run = ReswGeneratorHarness.Run([SharedFile()]);

        // A hint name is a file name, so the segments a relative path climbs through have no business in it.
        var hintName = Assert.Single(run.Sources.Keys.Where(name => name.Contains(".resw")));

        Assert.DoesNotContain("..", hintName);
        Assert.StartsWith("TestProject.Resources.resw", hintName);
    }

    [Fact]
    public void ASharedResourceIsTranslatedTheSameWayAsAResourceOfTheProject()
    {
        var run = ReswGeneratorHarness.Run(
            [SharedFile("en-US"), SharedFile("fr")],
            defaultLanguage: "en-US");

        // The two files are one resource in two languages, so one class is generated, not two.
        Assert.Single(run.Sources.Keys, name => name.Contains(".resw"));
        run.AssertCompiles();
    }

    [Fact]
    public void AProjectCanHoldBothItsOwnResourcesAndSharedOnes()
    {
        var run = ReswGeneratorHarness.Run(
        [
            SharedFile(),
            ReswGeneratorHarness.File("en-US", ReswTestHelpers.CreateResw(("Own", "Its own string", null)), baseName: "Own"),
        ]);

        Assert.Contains("namespace TestProject", run.Source("Resources.resw"));
        Assert.Contains("namespace TestProject.Strings", run.Source("Own.resw"));
        run.AssertCompiles();
    }
}
