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
        // Keyed without regard to case, because Windows paths are matched that way: 'Strings\en-US\R.resw' and
        // 'strings\en-us\R.resw' name one file, and grouping them apart would generate the same class twice.
        return reswFiles.GroupBy(
            static path => Path.Combine(
                Path.GetDirectoryName(Path.GetDirectoryName(path)) ?? string.Empty,
                Path.GetFileName(path)),
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Retrieve the default resource file from the given list that matches one of the preferred languages.
    /// </summary>
    /// <param name="reswFiles">The paths of the files of one group.</param>
    /// <param name="defaultLanguage">The default language of the project, if it declares one.</param>
    /// <returns>The path of the file holding the resources of the default language.</returns>
    /// <remarks>
    /// The whole tag is preferred, then the language on its own, so a project whose default is <c>fr</c> and
    /// whose only folder is <c>fr-FR</c> reads its resources from there rather than from whichever file the
    /// file system happened to list first.
    /// </remarks>
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

        var files = reswFiles.ToList();

        // Each candidate is tried whole and then by its language alone, before the next candidate: a project
        // that declares 'fr' should read 'fr-FR' rather than fall through to English.
        foreach (var language in candidateLanguages)
        {
            if (Match(files, language, whole: true) is { } exact)
            {
                return exact;
            }

            if (Match(files, language, whole: false) is { } byLanguage)
            {
                return byLanguage;
            }
        }

        // Fall back to a file of the group, chosen the same way every time: which resources the generated class
        // is built from should not depend on the order the file system listed them in.
        return files.OrderBy(static path => path, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
    }

    /// <summary>
    /// Finds the file of a language among a group.
    /// </summary>
    /// <param name="reswFiles">The paths of the files of one group.</param>
    /// <param name="language">The language to look for.</param>
    /// <param name="whole">Whether the whole tag has to match, rather than the language of it.</param>
    /// <returns>The path of the file, or <see langword="null"/> when the group holds none.</returns>
    private static string? Match(IEnumerable<string> reswFiles, string language, bool whole)
    {
        var wanted = whole ? language : PrimarySubtag(language);

        foreach (var reswFile in reswFiles)
        {
            // The immediate parent folder names the language, and is missing for a file that has no folder.
            var parentFolderName = Path.GetFileName(Path.GetDirectoryName(reswFile));

            if (string.IsNullOrEmpty(parentFolderName))
            {
                continue;
            }

            var candidate = whole ? parentFolderName : PrimarySubtag(parentFolderName);

            if (candidate.Equals(wanted, StringComparison.OrdinalIgnoreCase))
            {
                return reswFile;
            }
        }

        return null;
    }

    private static string PrimarySubtag(string languageTag)
    {
        var separator = languageTag.IndexOfAny(['-', '_']);

        return separator < 0 ? languageTag : languageTag.Substring(0, separator);
    }
}
