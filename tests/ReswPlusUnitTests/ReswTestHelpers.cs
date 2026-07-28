using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using ReswPlus.SourceGenerator;
using ReswPlus.SourceGenerator.Analysis;
using ReswPlus.SourceGenerator.ClassGenerators;
using ReswPlus.SourceGenerator.CodeGenerators;
using ReswPlus.SourceGenerator.Models;
using Xunit;

namespace ReswPlusUnitTests;

/// <summary>
/// Helpers to build <c>.resw</c> documents and run the ReswPlus generators over them from tests.
/// </summary>
internal static class ReswTestHelpers
{
    /// <summary>
    /// Builds the content of a <c>.resw</c> file from the given entries.
    /// </summary>
    /// <param name="entries">The entries to add, as key/value/comment triples. A <see langword="null"/> comment is omitted.</param>
    /// <returns>The content of the resulting <c>.resw</c> file.</returns>
    public static string CreateResw(params (string Key, string Value, string? Comment)[] entries)
    {
        var elements = entries.Select(entry =>
            $"""
               <data name="{entry.Key}" xml:space="preserve">
                 <value>{Escape(entry.Value)}</value>{(entry.Comment is null ? "" : $"\r\n    <comment>{Escape(entry.Comment)}</comment>")}
               </data>
             """);

        return $"""
            <?xml version="1.0" encoding="utf-8"?>
            <root>
              <resheader name="resmimetype">
                <value>text/microsoft-resx</value>
              </resheader>
            {string.Join("\r\n", elements)}
            </root>
            """;
    }

    /// <summary>
    /// Generates the C# code for a <c>.resw</c> file.
    /// </summary>
    /// <param name="reswContent">The content of the <c>.resw</c> file.</param>
    /// <param name="appType">The type of the consuming application.</param>
    /// <returns>The generated C# code.</returns>
    public static string GenerateCode(string reswContent, AppType appType = AppType.WindowsAppSDK)
    {
        return GenerateFile(reswContent, appType).Content;
    }

    /// <summary>
    /// Generates the file for a <c>.resw</c> file.
    /// </summary>
    /// <param name="reswContent">The content of the <c>.resw</c> file.</param>
    /// <param name="appType">The type of the consuming application.</param>
    /// <returns>The generated file.</returns>
    public static GeneratedFile GenerateFile(string reswContent, AppType appType = AppType.WindowsAppSDK)
    {
        var resourceFileInfo = new ResourceFileInfo(@"C:\Project\Strings\en-US\Resources.resw", new Project("TestProject", isLibrary: false));
        var generator = ReswClassGenerator.CreateGenerator(resourceFileInfo, logger: null);

        Assert.NotNull(generator);

        var result = generator!.GenerateCode(
            baseFilename: "Resources",
            content: reswContent,
            defaultNamespace: "TestProject.Strings",
            isAdvanced: true,
            appType: appType);

        Assert.NotNull(result);

        return result!.Files.Single();
    }

    private static string Escape(string text)
    {
        return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
    }

    /// <summary>
    /// Runs the resource analysis over a set of in-memory <c>.resw</c> files.
    /// </summary>
    /// <param name="defaultLanguage">The default language declared by the project.</param>
    /// <param name="files">The files of the project, as language folder name and content pairs.</param>
    /// <returns>The diagnostics reported for those files.</returns>
    public static IReadOnlyList<Diagnostic> Analyze(string? defaultLanguage, params (string Language, string Content)[] files)
    {
        var documents = files
            .Select(file => ($@"C:\Project\Strings\{file.Language}\Resources.resw", SourceText.From(file.Content)))
            .ToArray();

        var diagnostics = new List<Diagnostic>();

        ReswResourceAnalyzer.Analyze(documents, defaultLanguage, diagnostics.Add, CancellationToken.None);

        return diagnostics;
    }

    /// <summary>
    /// Returns the identifiers of the given diagnostics, in the order they were reported.
    /// </summary>
    /// <param name="diagnostics">The diagnostics to describe.</param>
    /// <returns>The identifiers of the diagnostics.</returns>
    public static string[] GetIds(this IEnumerable<Diagnostic> diagnostics)
    {
        return diagnostics.Select(diagnostic => diagnostic.Id).ToArray();
    }
}
