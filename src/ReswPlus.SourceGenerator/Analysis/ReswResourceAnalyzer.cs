using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using ReswPlus.SourceGenerator.ClassGenerators;

namespace ReswPlus.SourceGenerator.Analysis;

/// <summary>
/// Reports the problems of the <c>.resw</c> files of a project that would otherwise only surface at runtime, in
/// a language the team may not read.
/// </summary>
/// <remarks>
/// Every rule is reported as a warning rather than an error: raising the severity would break the build of every
/// project that already has an inconsistency the moment it updates the package. Projects that want a rule to be
/// fatal can escalate it through <c>.editorconfig</c>.
/// <para>
/// The rules err on the side of not firing. A noisy analyzer gets disabled wholesale, taking the valuable rules
/// with it, so a rule that cannot decide stays silent.
/// </para>
/// </remarks>
internal static class ReswResourceAnalyzer
{
    /// <summary>
    /// The suffixes that identify a plural form of a resource, including the ReswPlus specific empty state.
    /// </summary>
    private static readonly string[] PluralSuffixes = ["Zero", "One", "Two", "Few", "Many", "Other", "None"];

    /// <summary>
    /// Analyzes the <c>.resw</c> files of a project.
    /// </summary>
    /// <param name="reswFiles">The <c>.resw</c> files of the project, with their content.</param>
    /// <param name="defaultLanguage">The default language of the project, if it declares one.</param>
    /// <param name="reportDiagnostic">The callback invoked for every problem found.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    public static void Analyze(
        IReadOnlyList<(string Path, SourceText Text)> reswFiles,
        string? defaultLanguage,
        Action<Diagnostic> reportDiagnostic,
        CancellationToken cancellationToken)
    {
        var textsByPath = new Dictionary<string, SourceText>();

        foreach (var (path, text) in reswFiles)
        {
            textsByPath[path] = text;
        }

        foreach (var group in ReswFileGrouping.GroupByResource(textsByPath.Keys))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var defaultPath = ReswFileGrouping.RetrieveDefaultResourceFile(group, defaultLanguage);

            if (defaultPath is null || ReswDocument.Parse(defaultPath, textsByPath[defaultPath], cancellationToken) is not { } defaultDocument)
            {
                continue;
            }

            var defaultModel = ReswResourceModel.Create(defaultDocument);

            AnalyzeDocument(defaultModel, defaultModel, reportDiagnostic);

            foreach (var path in group)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (path == defaultPath || ReswDocument.Parse(path, textsByPath[path], cancellationToken) is not { } document)
                {
                    continue;
                }

                AnalyzeDocument(ReswResourceModel.Create(document), defaultModel, reportDiagnostic);
            }
        }
    }

    private static void AnalyzeDocument(ReswResourceModel model, ReswResourceModel defaultModel, Action<Diagnostic> reportDiagnostic)
    {
        ReportDuplicateMembers(model, reportDiagnostic);
        ReportMissingPluralForms(model, reportDiagnostic);
        ReportFormattingProblems(model, defaultModel, reportDiagnostic);
    }

    /// <summary>
    /// RESWP0009: reports the resources that are generated as a member another resource already generates.
    /// </summary>
    /// <remarks>
    /// The comparison is case insensitive because resource lookup is: two resources whose names only differ by
    /// case resolve to the same string at runtime, even though the members generated for them do not collide.
    /// </remarks>
    private static void ReportDuplicateMembers(ReswResourceModel model, Action<Diagnostic> reportDiagnostic)
    {
        var membersByName = new Dictionary<string, ReswMember>(StringComparer.OrdinalIgnoreCase);

        foreach (var member in model.Members)
        {
            if (membersByName.TryGetValue(member.Name, out var existing))
            {
                reportDiagnostic(Diagnostic.Create(
                    Diagnostics.DuplicateResource,
                    member.Entries[0].Location,
                    member.Entries[0].Key,
                    member.Name,
                    existing.Entries[0].Key));
            }
            else
            {
                membersByName.Add(member.Name, member);
            }
        }
    }

    /// <summary>
    /// RESWP0008: reports the pluralized resources that don't define every plural form their language requires.
    /// </summary>
    /// <remarks>
    /// A missing form is not a build failure: the lookup simply returns an empty string at runtime, which makes
    /// this the kind of problem that ships unnoticed.
    /// </remarks>
    private static void ReportMissingPluralForms(ReswResourceModel model, Action<Diagnostic> reportDiagnostic)
    {
        if (model.Document.Language is not { Length: > 0 } language ||
            PluralFormsRetriever.RetrievePluralCategoriesForLanguage(language) is not { } requiredCategories)
        {
            return;
        }

        foreach (var member in model.Members)
        {
            if (!member.IsPlural)
            {
                continue;
            }

            // A pluralized resource that also has variants is declined once per variant, and every variant needs
            // the full set of plural forms of the language.
            foreach (var declension in GetDeclensions(member))
            {
                var missingCategories = requiredCategories
                    .Where(category => !model.TryGetEntry($"{declension.Prefix}_{category}", out _))
                    .ToArray();

                if (missingCategories.Length == 0)
                {
                    continue;
                }

                reportDiagnostic(Diagnostic.Create(
                    Diagnostics.MissingPluralForms,
                    declension.Location,
                    declension.Prefix,
                    string.Join(", ", missingCategories.Select(category => $"'_{category}'")),
                    language));
            }
        }
    }

    /// <summary>
    /// RESWP0006, RESWP0007 and RESWP0010: reports the values that are not usable as the composite format string
    /// the generated code passes them to.
    /// </summary>
    /// <param name="model">The file being analyzed.</param>
    /// <param name="defaultModel">
    /// The file of the default language, which is the only one carrying the <c>#Format</c> tags and therefore the
    /// only one that determines whether, and with how many arguments, a resource is formatted.
    /// </param>
    /// <param name="reportDiagnostic">The callback invoked for every problem found.</param>
    private static void ReportFormattingProblems(ReswResourceModel model, ReswResourceModel defaultModel, Action<Diagnostic> reportDiagnostic)
    {
        var isDefaultLanguage = ReferenceEquals(model, defaultModel);

        foreach (var member in model.Members)
        {
            // Values of resources that are never formatted are returned verbatim, so braces in them are literal.
            if (!defaultModel.TryGetMember(member.Name, out var defaultMember) || !defaultMember.IsFormatted)
            {
                continue;
            }

            foreach (var entry in member.Entries)
            {
                if (!CompositeFormatString.TryGetArgumentIndexes(entry.Value, out var indexes))
                {
                    reportDiagnostic(Diagnostic.Create(Diagnostics.InvalidFormatString, entry.Location, entry.Key));

                    continue;
                }

                if (TryGetUndeclaredIndex(indexes, defaultMember.FormatParameterCount, out var undeclaredIndex))
                {
                    reportDiagnostic(Diagnostic.Create(
                        Diagnostics.UndeclaredFormatParameter,
                        entry.Location,
                        entry.Key,
                        undeclaredIndex,
                        defaultMember.FormatParameterCount));

                    continue;
                }

                // Comparing a translation against itself would always match, and a resource that only exists in
                // the default language has nothing to be compared with.
                if (isDefaultLanguage ||
                    !defaultModel.TryGetEntry(entry.Key, out var defaultEntry) ||
                    !CompositeFormatString.TryGetArgumentIndexes(defaultEntry.Value, out var defaultIndexes) ||
                    indexes.SetEquals(defaultIndexes))
                {
                    continue;
                }

                reportDiagnostic(Diagnostic.Create(
                    Diagnostics.PlaceholderMismatch,
                    entry.Location,
                    entry.Key,
                    DescribePlaceholders(indexes),
                    DescribePlaceholders(defaultIndexes)));
            }
        }
    }

    /// <summary>
    /// Looks for a placeholder that has no matching argument in the generated call to <c>string.Format</c>.
    /// </summary>
    /// <param name="indexes">The argument indexes referenced by the value, in ascending order.</param>
    /// <param name="parameterCount">The number of arguments the generated code passes.</param>
    /// <param name="undeclaredIndex">The first index that has no matching argument.</param>
    /// <returns>Whether the value would throw a <see cref="FormatException"/> at runtime.</returns>
    private static bool TryGetUndeclaredIndex(IEnumerable<int> indexes, int parameterCount, out int undeclaredIndex)
    {
        foreach (var index in indexes)
        {
            if (index >= parameterCount)
            {
                undeclaredIndex = index;

                return true;
            }
        }

        undeclaredIndex = 0;

        return false;
    }

    /// <summary>
    /// Returns the resource name prefixes a pluralized resource is declined from, one per variant.
    /// </summary>
    private static IEnumerable<(string Prefix, Location Location)> GetDeclensions(ReswMember member)
    {
        var locationsByPrefix = new Dictionary<string, Location>(StringComparer.Ordinal);

        foreach (var entry in member.Entries)
        {
            var separator = entry.Key.LastIndexOf('_');

            if (separator <= 0 || !PluralSuffixes.Contains(entry.Key.Substring(separator + 1)))
            {
                continue;
            }

            var prefix = entry.Key.Substring(0, separator);

            if (!locationsByPrefix.ContainsKey(prefix))
            {
                locationsByPrefix.Add(prefix, entry.Location);
            }
        }

        return locationsByPrefix.Select(pair => (pair.Key, pair.Value));
    }

    private static string DescribePlaceholders(IEnumerable<int> indexes)
    {
        var placeholders = indexes.Select(index => $"{{{index.ToString(CultureInfo.InvariantCulture)}}}").ToArray();

        return placeholders.Length == 0 ? "no placeholder" : string.Join(", ", placeholders);
    }
}
