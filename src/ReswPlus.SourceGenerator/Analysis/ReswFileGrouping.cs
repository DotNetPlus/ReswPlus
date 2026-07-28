using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReswPlus.SourceGenerator.Analysis;

/// <summary>
/// Groups the <c>.resw</c> files of a project into the sets of files that translate one another.
/// </summary>
internal static class ReswFileGrouping
{
    /// <summary>
    /// Groups the given <c>.resw</c> files by the resource file they are a translation of.
    /// </summary>
    /// <param name="reswFiles">The paths of the <c>.resw</c> files of the project.</param>
    /// <returns>The groups of files, keyed by the language independent path of the resource.</returns>
    /// <remarks>
    /// Translations of the same resource live in sibling language folders, so dropping the language folder from
    /// the path of a file yields a key shared by all of its translations.
    /// </remarks>
    public static IEnumerable<IGrouping<string, string>> GroupByResource(IEnumerable<string> reswFiles)
    {
        return reswFiles.GroupBy(static path => Path.Combine(
            Path.GetDirectoryName(Path.GetDirectoryName(path)) ?? string.Empty,
            Path.GetFileName(path)));
    }

    /// <summary>
    /// Retrieve the default resource file from the given list that matches one of the preferred languages.
    /// </summary>
    /// <param name="reswFiles">The paths of the files of one group.</param>
    /// <param name="defaultLanguage">The default language of the project, if it declares one.</param>
    /// <returns>The path of the file holding the resources of the default language.</returns>
    public static string? RetrieveDefaultResourceFile(IEnumerable<string> reswFiles, string? defaultLanguage)
    {
        // Build a list of candidate languages.
        var candidateLanguages = new List<string>();
        if (defaultLanguage is { Length: > 0 })
        {
            candidateLanguages.Add(defaultLanguage);
        }

        // Ensure "en-us" and "en" are included if not already the default.
        if (!"en-us".Equals(defaultLanguage, StringComparison.OrdinalIgnoreCase))
        {
            candidateLanguages.Add("en-us");
        }

        if (!"en".Equals(defaultLanguage, StringComparison.OrdinalIgnoreCase))
        {
            candidateLanguages.Add("en");
        }

        // Iterate candidates and files to find a match.
        foreach (var language in candidateLanguages)
        {
            foreach (var reswFile in reswFiles)
            {
                // Get the immediate parent folder name (e.g. "en-us").
                var parentFolderName = Path.GetFileName(Path.GetDirectoryName(reswFile));
                if (parentFolderName.Equals(language, StringComparison.OrdinalIgnoreCase))
                {
                    return reswFile;
                }
            }
        }

        // Fallback to the first available resource file.
        return reswFiles.FirstOrDefault();
    }
}
