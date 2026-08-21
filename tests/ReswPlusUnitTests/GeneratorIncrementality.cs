using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ReswPlus.SourceGenerator;
using ReswPlus.SourceGenerator.Pipeline;
using ReswPlus.SourceGenerator.ClassGenerators;
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
    /// Adding a resource file costs the work of that file, not the work of the project.
    /// </summary>
    /// <remarks>
    /// Adding, removing or renaming a resource changes how the project is laid out, which every file's
    /// generation reads to learn its own hint name. Reading it is cheap; parsing and formatting a resource file
    /// is not, and there is no reason for a file nobody touched to be parsed again because a different file
    /// appeared beside it.
    /// </remarks>
    [Fact]
    public void AddingAResourceLeavesTheGenerationOfTheOthersAlone()
    {
        var before = ThreeResources();
        var after = before.Append(
            ReswGeneratorHarness.File(English, ReswTestHelpers.CreateResw(("Added", "Fourth", null)), baseName: "Fourth"))
            .ToList();

        AssertOnlyRan(before, after, ran: 1);
    }

    [Fact]
    public void RemovingAResourceLeavesTheGenerationOfTheOthersAlone()
    {
        var before = ThreeResources();
        var after = before.Take(2).ToList();

        AssertOnlyRan(before, after, ran: 0);
    }

    [Fact]
    public void RenamingAResourceLeavesTheGenerationOfTheOthersAlone()
    {
        var before = ThreeResources();
        var after = before.Take(2).Append(
            ReswGeneratorHarness.File(English, ReswTestHelpers.CreateResw(("Question", "Third", null)), baseName: "Renamed"))
            .ToList();

        AssertOnlyRan(before, after, ran: 1);
    }

    [Fact]
    public void TranslatingAResourceIntoANewLanguageLeavesTheGenerationOfTheOthersAlone()
    {
        var before = ThreeResources();
        var after = before.Append(
            ReswGeneratorHarness.File("fr-FR", ReswTestHelpers.CreateResw(("Greeting", "Bonjour", null)), baseName: "First"))
            .ToList();

        // A translation is not generated from, so nothing should be generated at all for it -- and the files
        // that were already there should not be touched because a language appeared beside them.
        AssertOnlyRan(before, after, ran: 0);
    }

    /// <summary>
    /// Asserts how many resource files had their code generated again after a structural change to a project.
    /// </summary>
    /// <param name="before">The files of the project on the first run.</param>
    /// <param name="after">The files of the project on the second run.</param>
    /// <param name="ran">How many files are expected to be generated on the second run.</param>
    /// <remarks>
    /// <see cref="IncrementalStepRunReason.Cached"/> is the transform not running at all, while
    /// <see cref="IncrementalStepRunReason.Unchanged"/> is it running and happening to produce the same value,
    /// which costs exactly as much as producing a different one. Only the former is free.
    /// <para>
    /// <see cref="IncrementalStepRunReason.Removed"/> is neither: it is the record of an output that used to
    /// exist and no longer does, and nothing was computed for it.
    /// </para>
    /// </remarks>
    [Fact]
    public void AFileIsGeneratedAgainWhenOnlyTheProjectChanged()
    {
        // What is generated for a file depends on the project it belongs to -- its namespace, the kind of app
        // it is -- so two of these are the same only if the project is the same too. Drop the project from
        // that comparison and a file whose project was renamed keeps the namespace it had.
        var file = new InMemoryAdditionalText(@"C:\Project\Strings\en-US\Resources.resw", "<root />");

        var before = new ReswFileToGenerate(file, Project("TestProject"), "Resources.g.cs");
        var after = new ReswFileToGenerate(file, Project("RenamedProject"), "Resources.g.cs");

        Assert.NotEqual(before, after);
        Assert.Equal(before, new ReswFileToGenerate(file, Project("TestProject"), "Resources.g.cs"));
    }

    [Fact]
    public void AFileIsGeneratedAgainWhenOnlyItsNameChanged()
    {
        var file = new InMemoryAdditionalText(@"C:\Project\Strings\en-US\Resources.resw", "<root />");

        // Two resources of the same name in different folders are told apart by the name they are emitted
        // under, so that has to count as well.
        Assert.NotEqual(
            new ReswFileToGenerate(file, Project("TestProject"), "Resources.g.cs"),
            new ReswFileToGenerate(file, Project("TestProject"), "Resources.2.g.cs"));
    }

    [Fact]
    public void TwoOfTheSameFileHashAlike()
    {
        var file = new InMemoryAdditionalText(@"C:\Project\Strings\en-US\Resources.resw", "<root />");

        Assert.Equal(
            new ReswFileToGenerate(file, Project("TestProject"), "Resources.g.cs").GetHashCode(),
            new ReswFileToGenerate(file, Project("TestProject"), "Resources.g.cs").GetHashCode());
    }

    [Fact]
    public void AFileIsGeneratedAgainWhenItIsTheOneThatChanged()
    {
        // The compiler hands back the same object for a file it has not seen change, so identity is the
        // question being asked. Two files of the same content are still two files.
        var project = Project("TestProject");

        Assert.NotEqual(
            new ReswFileToGenerate(new InMemoryAdditionalText(@"C:\Project\Strings\en-US\Resources.resw", "<root />"), project, "Resources.g.cs"),
            new ReswFileToGenerate(new InMemoryAdditionalText(@"C:\Project\Strings\en-US\Resources.resw", "<root />"), project, "Resources.g.cs"));
    }

    private static ReswProject Project(string rootNamespace)
    {
        var options = new Dictionary<string, string>(AnalyzerConfigOptions.KeyComparer)
        {
            ["build_property.ProjectDir"] = ReswGeneratorHarness.ProjectDir,
            ["build_property.OutputType"] = "Library",
            ["build_property.RootNamespace"] = rootNamespace,
        };

        return ReswProject.Create(
            new CompilationInfo(true, AppType.WindowsAppSDK, "TestProject"),
            ReswBuildOptions.Read(new TestAnalyzerConfigOptionsProvider(options).GlobalOptions));
    }

    private static void AssertOnlyRan(IReadOnlyList<ReswFile> before, IReadOnlyList<ReswFile> after, int ran)
    {
        var second = ReswGeneratorHarness.Run(before).RunAgain(after);
        var reasons = second.Reasons(ReswPlus.SourceGenerator.Pipeline.TrackingNames.Generation);

        var executed = reasons.Count(reason =>
            reason is not (IncrementalStepRunReason.Cached or IncrementalStepRunReason.Removed));

        Assert.True(
            executed == ran,
            $"The code of {executed} resource files was generated again, where {ran} was expected. " +
            $"Reasons: {string.Join(", ", reasons)}.");
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
