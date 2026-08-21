using System.Linq;
using ReswPlus.SourceGenerator.Analysis;
using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Tests how the <c>.resw</c> files of a project are grouped, and which of a group the code is generated from.
/// </summary>
public class ResourceFileGrouping
{
    [Fact]
    public void TranslationsOfOneResourceAreGroupedTogetherWhateverTheirCase()
    {
        // A path names the same file whichever case it is written in, so grouping these apart would generate
        // the same class twice and each would fight the other for its file name.
        var groups = ReswFileGrouping.GroupByResource(
        [
            @"C:\p\Strings\en-US\Resources.resw",
            @"C:\p\strings\fr-FR\Resources.resw",
            @"C:\p\STRINGS\de-DE\resources.resw"
        ]);

        Assert.Single(groups);
    }

    [Fact]
    public void ResourcesOfDifferentNamesAreNotGroupedTogether()
    {
        var groups = ReswFileGrouping.GroupByResource(
        [
            @"C:\p\Strings\en-US\Resources.resw",
            @"C:\p\Strings\en-US\Errors.resw"
        ]);

        Assert.Equal(2, groups.Count());
    }

    [Theory]
    // The declared default is preferred, whole.
    [InlineData("fr-FR", @"C:\p\Strings\fr-FR\R.resw")]
    // A default naming only the language reads the resources of a region of it rather than of another language.
    [InlineData("fr", @"C:\p\Strings\fr-FR\R.resw")]
    // Nothing declared falls back to English, which is preferred whole before by language.
    [InlineData(null, @"C:\p\Strings\en-US\R.resw")]
    public void TheDefaultResourceFileIsTheOneOfTheDefaultLanguage(string? defaultLanguage, string expected)
    {
        string[] files =
        [
            @"C:\p\Strings\de-DE\R.resw",
            @"C:\p\Strings\en-US\R.resw",
            @"C:\p\Strings\fr-FR\R.resw"
        ];

        Assert.Equal(expected, ReswFileGrouping.RetrieveDefaultResourceFile(files, defaultLanguage));
    }

    [Fact]
    public void TheDefaultResourceFileIsTheSameOneWhateverOrderTheFilesArriveIn()
    {
        // Which resources the generated class is built from should not depend on the order the file system
        // listed them in: the same project would otherwise generate different code on different machines.
        string[] files = [@"C:\p\Strings\pt-BR\R.resw", @"C:\p\Strings\de-DE\R.resw", @"C:\p\Strings\it-IT\R.resw"];

        var chosen = ReswFileGrouping.RetrieveDefaultResourceFile(files, defaultLanguage: null);
        var reversed = ReswFileGrouping.RetrieveDefaultResourceFile(files.Reverse().ToArray(), defaultLanguage: null);

        Assert.Equal(chosen, reversed);
    }

    [Fact]
    public void AResourceWithNoLanguageFolderDoesNotStopTheOthersBeingRead()
    {
        // A file sitting at the root of the drive has no folder to read a language from.
        string[] files = [@"C:\R.resw", @"C:\p\Strings\en-US\R.resw"];

        Assert.Equal(@"C:\p\Strings\en-US\R.resw", ReswFileGrouping.RetrieveDefaultResourceFile(files, "en-US"));
    }
}
