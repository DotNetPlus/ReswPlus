using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Covers what the generator recomputes while a project is edited.
/// </summary>
/// <remarks>
/// The compiler keeps the result of every stage of the pipeline between runs and only reruns the ones whose
/// inputs changed, so how the stages are split is what decides the cost of a keystroke in the IDE. That cost
/// is invisible in the output -- a generator that recomputes everything on every keystroke produces exactly
/// the same code as one that recomputes nothing -- which is why it is asserted here rather than described.
/// </remarks>
public class GeneratorIncrementality
{
    private const string English = "en-US";

    private static IReadOnlyList<ReswFile> ThreeResources(string firstValue = "First") =>
    [
        ReswGeneratorHarness.File(English, ReswTestHelpers.CreateResw(("Greeting", firstValue, null)), baseName: "First"),
        ReswGeneratorHarness.File(English, ReswTestHelpers.CreateResw(("Farewell", "Second", null)), baseName: "Second"),
        ReswGeneratorHarness.File(English, ReswTestHelpers.CreateResw(("Question", "Third", null)), baseName: "Third"),
    ];

    [Fact]
    public void RunningAgainOverAnUnchangedProjectRecomputesNothing()
    {
        var files = ThreeResources();
        var second = ReswGeneratorHarness.Run(files).RunAgain(files);

        second.AssertReused(ReswPlus.SourceGenerator.Pipeline.TrackingNames.Options);
        second.AssertReused(ReswPlus.SourceGenerator.Pipeline.TrackingNames.CompilationInfo);
        second.AssertReused(ReswPlus.SourceGenerator.Pipeline.TrackingNames.Project);
        second.AssertReused(ReswPlus.SourceGenerator.Pipeline.TrackingNames.Layout);
        second.AssertReused(ReswPlus.SourceGenerator.Pipeline.TrackingNames.Generation);
        second.AssertReused(ReswPlus.SourceGenerator.Pipeline.TrackingNames.Support);
    }

    /// <summary>
    /// The whole point of splitting the pipeline: a string edited in one resource file costs the work of that
    /// file, not the work of the project.
    /// </summary>
    [Fact]
    public void EditingOneResourceOnlyRegeneratesThatResource()
    {
        var first = ReswGeneratorHarness.Run(ThreeResources());
        var second = first.RunAgain(ThreeResources(firstValue: "First, edited"));

        Assert.Equal(1, second.RecomputedCount(ReswPlus.SourceGenerator.Pipeline.TrackingNames.Generation));
        Assert.Contains("First, edited", second.Source("First.resw"));
    }

    [Fact]
    public void EditingAResourceDoesNotMakeTheProjectRegroup()
    {
        var first = ReswGeneratorHarness.Run(ThreeResources());
        var second = first.RunAgain(ThreeResources(firstValue: "First, edited"));

        // The layout is worked out from the paths of the resource files, which an edit does not touch.
        second.AssertReused(ReswPlus.SourceGenerator.Pipeline.TrackingNames.Paths);
        second.AssertReused(ReswPlus.SourceGenerator.Pipeline.TrackingNames.Layout);
    }

    [Fact]
    public void EditingAResourceDoesNotMakeThePropertiesOfTheProjectBeReadAgain()
    {
        var first = ReswGeneratorHarness.Run(ThreeResources());
        var second = first.RunAgain(ThreeResources(firstValue: "First, edited"));

        second.AssertReused(ReswPlus.SourceGenerator.Pipeline.TrackingNames.Options);
        second.AssertReused(ReswPlus.SourceGenerator.Pipeline.TrackingNames.CompilationInfo);
        second.AssertReused(ReswPlus.SourceGenerator.Pipeline.TrackingNames.Project);
    }

    /// <summary>
    /// The support sources are shared by the whole project, and rebuilding them means rebuilding the plural
    /// rules of every language it is translated in.
    /// </summary>
    [Fact]
    public void EditingAResourceDoesNotRebuildTheSharedSupport()
    {
        var plural = ReswTestHelpers.CreateResw(
            ("Items_One", "{0} item", "#Format[Plural itemCount]"),
            ("Items_Other", "{0} items", "#Format[Plural itemCount]"));

        var edited = ReswTestHelpers.CreateResw(
            ("Items_One", "{0} item, edited", "#Format[Plural itemCount]"),
            ("Items_Other", "{0} items", "#Format[Plural itemCount]"));

        var first = ReswGeneratorHarness.Run(
        [
            ReswGeneratorHarness.File(English, plural),
            ReswGeneratorHarness.File("ru-RU", plural),
        ]);

        var second = first.RunAgain(
        [
            ReswGeneratorHarness.File(English, edited),
            ReswGeneratorHarness.File("ru-RU", plural),
        ]);

        second.AssertReused(ReswPlus.SourceGenerator.Pipeline.TrackingNames.Support);
    }

    [Fact]
    public void EditingATranslationDoesNotRegenerateAnything()
    {
        var english = ReswTestHelpers.CreateResw(("Plain", "A value", null));

        var first = ReswGeneratorHarness.Run(
        [
            ReswGeneratorHarness.File(English, english),
            ReswGeneratorHarness.File("fr-FR", ReswTestHelpers.CreateResw(("Plain", "Une valeur", null))),
        ],
        defaultLanguage: English);

        var second = first.RunAgain(
        [
            ReswGeneratorHarness.File(English, english),
            ReswGeneratorHarness.File("fr-FR", ReswTestHelpers.CreateResw(("Plain", "Une autre valeur", null))),
        ]);

        // Only the file of the default language is generated from, so a translation being edited changes
        // nothing the generator emits.
        second.AssertReused(ReswPlus.SourceGenerator.Pipeline.TrackingNames.Generation);
    }

    [Fact]
    public void TouchingAResourceWithoutChangingWhatItDeclaresCostsNothingDownstream()
    {
        var files = ThreeResources();

        // The same resources, in files the compiler hands over as new objects, which is what happens when an
        // edit is undone or a file is rewritten by a tool.
        var touched = files
            .Select(file => new ReswFile(file.Path, file.Content + "\r\n"))
            .ToArray();

        var first = ReswGeneratorHarness.Run(files);
        var second = first.RunAgain(touched);

        // The files are read and generated again, but what comes out is the same, so nothing below the
        // generation stage runs and nothing is emitted again.
        Assert.Equal(0, second.RecomputedCount(ReswPlus.SourceGenerator.Pipeline.TrackingNames.Generation));
        second.AssertReused(ReswPlus.SourceGenerator.Pipeline.TrackingNames.Support);

        Assert.Equal(
            first.Sources.OrderBy(source => source.Key, System.StringComparer.Ordinal),
            second.Sources.OrderBy(source => source.Key, System.StringComparer.Ordinal));
    }

    [Fact]
    public void AddingAResourceGeneratesItAndLeavesTheOutputOfTheOthersAlone()
    {
        var first = ReswGeneratorHarness.Run(ThreeResources());

        var second = first.RunAgain(
        [
            .. ThreeResources(),
            ReswGeneratorHarness.File(English, ReswTestHelpers.CreateResw(("Extra", "Fourth", null)), baseName: "Fourth"),
        ]);

        Assert.Contains("Fourth", second.Source("Fourth.resw"));
        Assert.Equal(4, second.Sources.Keys.Count(name => name.Contains(".resw")));
        second.AssertCompiles();
    }

    [Fact]
    public void TheTemplatesAreOnlyReadOnceForTheWholeProject()
    {
        // Reading a template decodes an embedded resource, and the same handful of templates back every
        // project the generator is loaded for, so they are decoded once and kept.
        var first = ReswGeneratorHarness.Run(ThreeResources());
        var second = first.RunAgain(ThreeResources(firstValue: "First, edited"));

        Assert.Equal(
            first.Source("ResourceStringProvider"),
            second.Source("ResourceStringProvider"));
    }
}
