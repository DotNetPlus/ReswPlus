using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using ReswPlus.SourceGenerator.ClassGenerators;
using ReswPlus.SourceGenerator.CodeGenerators;

namespace ReswPlus.SourceGenerator.Analysis;

/// <summary>
/// Implements the rules reported on the content of the <c>.resw</c> files of a project.
/// </summary>
/// <remarks>
/// Translation lag is informational by default, while problems that can break localized output are warnings.
/// Strict translation checks promote incomplete translations to warnings and output-breaking problems to errors.
/// <para>
/// The rules err on the side of not firing. A noisy analyzer gets disabled wholesale, taking the valuable rules
/// with it, so a rule that cannot decide stays silent.
/// </para>
/// </remarks>
internal static class ReswResourceRules
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
    /// <param name="generateResourceInterfaces">Whether injectable resource interfaces and providers are generated.</param>
    /// <param name="translationChecks">How diagnostics comparing translations with the default language are reported.</param>
    /// <param name="reportDiagnostic">The callback invoked for every problem found.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    public static void Analyze(
        IReadOnlyList<(string Path, SourceText Text)> reswFiles,
        string? defaultLanguage,
        bool generateResourceInterfaces,
        ReswTranslationChecks translationChecks,
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

            AnalyzeDocument(defaultModel, defaultModel, generateResourceInterfaces, translationChecks, reportDiagnostic);

            foreach (var path in group)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (path == defaultPath || ReswDocument.Parse(path, textsByPath[path], cancellationToken) is not { } document)
                {
                    continue;
                }

                AnalyzeDocument(
                    ReswResourceModel.Create(document),
                    defaultModel,
                    generateResourceInterfaces,
                    translationChecks,
                    reportDiagnostic);
            }
        }
    }

    private static void AnalyzeDocument(
        ReswResourceModel model,
        ReswResourceModel defaultModel,
        bool generateResourceInterfaces,
        ReswTranslationChecks translationChecks,
        Action<Diagnostic> reportDiagnostic)
    {
        ReportDuplicateMembers(model, translationChecks, reportDiagnostic);
        ReportReservedNames(model, generateResourceInterfaces, translationChecks, reportDiagnostic);
        ReportDuplicateFormatParameters(model, translationChecks, reportDiagnostic);

        var isDefaultLanguage = ReferenceEquals(model, defaultModel);

        if (!isDefaultLanguage && translationChecks != ReswTranslationChecks.Off)
        {
            ReportTranslationDifferences(model, defaultModel, translationChecks, reportDiagnostic);
        }

        if (isDefaultLanguage || translationChecks != ReswTranslationChecks.Off)
        {
            ReportMissingPluralForms(model, defaultModel, translationChecks, reportDiagnostic);
        }

        ReportFormattingProblems(model, defaultModel, translationChecks, reportDiagnostic);
    }

    /// <summary>
    /// RESWP0012: reports the resources the generated types already declare a member for.
    /// </summary>
    /// <remarks>
    /// The generator skips these resources rather than emitting a member that would not compile, which makes
    /// them silently absent from the generated class. This is what says so.
    /// </remarks>
    private static void ReportReservedNames(
        ReswResourceModel model,
        bool generateResourceInterfaces,
        ReswTranslationChecks translationChecks,
        Action<Diagnostic> reportDiagnostic)
    {
        var className = Path.GetFileNameWithoutExtension(model.Document.Path);

        foreach (var member in model.Members)
        {
            if (!GeneratedIdentifier.ConflictsWithGeneratedMember(member.Name, className, generateResourceInterfaces))
            {
                continue;
            }

            ReportDiagnostic(reportDiagnostic, translationChecks,
                Diagnostics.ReservedResourceName,
                member.Entries[0].Location,
                member.Entries[0].Key,
                Path.GetFileName(model.Document.Path));
        }
    }

    /// <summary>
    /// RESWP0013: reports the <c>#Format</c> tags that declare the same parameter name twice.
    /// </summary>
    /// <remarks>
    /// Only the names the tag itself declares are compared. The generator adds a parameter of its own to a
    /// pluralized or varianted resource, and renames it when the tag already uses its name, which is a
    /// conflict the author of the tag did not create and is not asked to resolve.
    /// </remarks>
    private static void ReportDuplicateFormatParameters(
        ReswResourceModel model,
        ReswTranslationChecks translationChecks,
        Action<Diagnostic> reportDiagnostic)
    {
        foreach (var member in model.Members)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var name in member.FormatParameterNames)
            {
                if (seen.Add(name))
                {
                    continue;
                }

                ReportDiagnostic(reportDiagnostic, translationChecks,
                    Diagnostics.DuplicateFormatParameter,
                    member.Entries[0].Location,
                    member.Name,
                    name);
            }
        }
    }

    /// <summary>
    /// RESWP0009: reports the resources that conflict with a resource declared earlier in the same file.
    /// </summary>
    /// <remarks>
    /// The comparison is case insensitive because resource lookup is: two resources whose names only differ by
    /// case resolve to the same string at runtime. A plain resource can also conflict with a pluralized or
    /// varianted one, in which case the generated members collide and the project no longer compiles.
    /// </remarks>
    private static void ReportDuplicateMembers(
        ReswResourceModel model,
        ReswTranslationChecks translationChecks,
        Action<Diagnostic> reportDiagnostic)
    {
        var membersByName = new Dictionary<string, ReswMember>(StringComparer.OrdinalIgnoreCase);

        foreach (var member in model.Members)
        {
            if (membersByName.TryGetValue(member.Name, out var existing))
            {
                ReportDiagnostic(reportDiagnostic, translationChecks,
                    Diagnostics.DuplicateResource,
                    member.Entries[0].Location,
                    member.Entries[0].Key,
                    existing.Entries[0].Key);
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
    private static void ReportMissingPluralForms(
        ReswResourceModel model,
        ReswResourceModel defaultModel,
        ReswTranslationChecks translationChecks,
        Action<Diagnostic> reportDiagnostic)
    {
        if (model.Document.Language is not { Length: > 0 } language ||
            PluralFormsRetriever.RetrievePluralFormForLanguage(language) is not { } pluralForm)
        {
            return;
        }

        foreach (var member in model.Members)
        {
            // A resource left behind in a translation after the default language dropped it generates nothing,
            // so its plural forms are never looked up and its missing ones don't matter.
            if (!member.IsPlural || !defaultModel.TryGetMember(member.Name, out var defaultMember) || !defaultMember.IsPlural)
            {
                continue;
            }

            // A pluralized resource that also has variants is declined once per variant, and every variant needs
            // the full set of plural forms of the language.
            foreach (var declension in GetDeclensions(member))
            {
                // GetPlural short circuits a zero quantity to the _None form when the resource declares one, so
                // for a language whose provider only returns Zero for a zero quantity the _Zero form is dead.
                var hasNoneForm = model.TryGetEntry($"{declension.Prefix}_None", out _);

                var missingCategories = pluralForm.Categories
                    .Where(category => !pluralForm.OptionalCategories.Contains(category))
                    .Where(category => !(category == PluralCategory.Zero && hasNoneForm && pluralForm.ZeroIsOnlyForZeroQuantity))
                    .Where(category => !model.TryGetEntry($"{declension.Prefix}_{category}", out _))
                    .ToArray();

                if (missingCategories.Length == 0)
                {
                    continue;
                }

                ReportDiagnostic(reportDiagnostic, translationChecks,
                    Diagnostics.MissingPluralForms,
                    declension.Location,
                    declension.Prefix,
                    string.Join(", ", missingCategories.Select(category => $"'_{category}'")),
                    language);
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
    private static void ReportFormattingProblems(
        ReswResourceModel model,
        ReswResourceModel defaultModel,
        ReswTranslationChecks translationChecks,
        Action<Diagnostic> reportDiagnostic)
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
                    ReportDiagnostic(
                        reportDiagnostic,
                        translationChecks,
                        Diagnostics.InvalidFormatString,
                        entry.Location,
                        entry.Key);

                    continue;
                }

                if (TryGetUndeclaredIndex(indexes, defaultMember.FormatParameterCount, out var undeclaredIndex))
                {
                    ReportDiagnostic(reportDiagnostic, translationChecks,
                        Diagnostics.UndeclaredFormatParameter,
                        entry.Location,
                        entry.Key,
                        undeclaredIndex,
                        defaultMember.FormatParameterCount);

                    continue;
                }

                // Comparing a translation against itself would always match, and a resource that only exists in
                // the default language has nothing to be compared with.
                if (translationChecks == ReswTranslationChecks.Off ||
                    isDefaultLanguage ||
                    !defaultModel.TryGetEntry(entry.Key, out var defaultEntry) ||
                    !CompositeFormatString.TryGetArgumentIndexes(defaultEntry.Value, out var defaultIndexes))
                {
                    continue;
                }

                // A translation is free to use more placeholders than the default language: string.Format ignores
                // the arguments a format string doesn't reference, and the indexes that have no matching argument
                // at all are already covered above. Only the placeholders a translation drops are a problem, since
                // they silently remove information from the localized string.
                var missingPlaceholders = defaultIndexes.Where(index => !indexes.Contains(index)).ToArray();

                if (missingPlaceholders.Length == 0)
                {
                    continue;
                }

                ReportDiagnostic(reportDiagnostic, translationChecks,
                    Diagnostics.PlaceholderMismatch,
                    entry.Location,
                    entry.Key,
                    DescribePlaceholders(missingPlaceholders));
            }
        }
    }

    private static void ReportTranslationDifferences(
        ReswResourceModel model,
        ReswResourceModel defaultModel,
        ReswTranslationChecks translationChecks,
        Action<Diagnostic> reportDiagnostic)
    {
        var language = model.Document.Language ?? Path.GetFileName(Path.GetDirectoryName(model.Document.Path));

        foreach (var defaultMember in defaultModel.Members)
        {
            if (!model.TryGetMember(defaultMember.Name, out var translatedMember))
            {
                ReportDiagnostic(
                    reportDiagnostic,
                    translationChecks,
                    Diagnostics.MissingTranslation,
                    defaultMember.Entries[0].Location,
                    defaultMember.Name,
                    language);

                continue;
            }

            if (defaultMember.IsPlural != translatedMember.IsPlural ||
                defaultMember.SupportsVariants != translatedMember.SupportsVariants)
            {
                ReportDiagnostic(
                    reportDiagnostic,
                    translationChecks,
                    Diagnostics.IncompatibleTranslationShape,
                    translatedMember.Entries[0].Location,
                    defaultMember.Name,
                    language,
                    "uses a different plain, plural, or variant structure than the default-language resource");

                continue;
            }

            var missingVariants = defaultMember.VariantIds
                .Where(id => !translatedMember.VariantIds.Contains(id, StringComparer.OrdinalIgnoreCase))
                .ToArray();

            if (missingVariants.Length > 0)
            {
                ReportDiagnostic(
                    reportDiagnostic,
                    translationChecks,
                    Diagnostics.IncompatibleTranslationShape,
                    translatedMember.Entries[0].Location,
                    defaultMember.Name,
                    language,
                    $"does not define the required variant(s) {DescribeValues(missingVariants)}");
            }

            var extraVariants = translatedMember.VariantIds
                .Where(id => !defaultMember.VariantIds.Contains(id, StringComparer.OrdinalIgnoreCase))
                .ToArray();

            if (extraVariants.Length > 0)
            {
                ReportDiagnostic(
                    reportDiagnostic,
                    translationChecks,
                    Diagnostics.ExtraTranslationVariants,
                    translatedMember.Entries[0].Location,
                    defaultMember.Name,
                    language,
                    DescribeValues(extraVariants));
            }
        }

        foreach (var translatedMember in model.Members)
        {
            if (!defaultModel.TryGetMember(translatedMember.Name, out _))
            {
                ReportDiagnostic(
                    reportDiagnostic,
                    translationChecks,
                    Diagnostics.TranslationWithoutDefault,
                    translatedMember.Entries[0].Location,
                    translatedMember.Name,
                    language);
            }
        }

        foreach (var entry in model.Members.SelectMany(member => member.Entries))
        {
            if (defaultModel.TryGetEntry(entry.Key, out var defaultEntry) &&
                string.Equals(entry.Value, defaultEntry.Value, StringComparison.Ordinal))
            {
                ReportDiagnostic(
                    reportDiagnostic,
                    translationChecks,
                    Diagnostics.UnchangedTranslation,
                    entry.Location,
                    entry.Key,
                    language);
            }
        }
    }

    private static string DescribeValues(IEnumerable<string> values)
    {
        return string.Join(", ", values.Select(value => $"'{value}'"));
    }

    private static void ReportDiagnostic(
        Action<Diagnostic> reportDiagnostic,
        ReswTranslationChecks translationChecks,
        DiagnosticDescriptor descriptor,
        Location location,
        params object[] messageArgs)
    {
        var severity = GetSeverity(descriptor, translationChecks);

        reportDiagnostic(Diagnostic.Create(
            descriptor,
            location,
            severity,
            additionalLocations: null,
            properties: null,
            messageArgs));
    }

    private static DiagnosticSeverity GetSeverity(
        DiagnosticDescriptor descriptor,
        ReswTranslationChecks translationChecks)
    {
        if (translationChecks != ReswTranslationChecks.Strict)
        {
            return descriptor.DefaultSeverity;
        }

        return descriptor.Id switch
        {
            "RESWP0016" or "RESWP0017" or "RESWP0020" => DiagnosticSeverity.Warning,
            "RESWP0018" => DiagnosticSeverity.Info,
            "RESWP0006" or "RESWP0007" or "RESWP0008" or "RESWP0009" or "RESWP0010" or
            "RESWP0012" or "RESWP0019" => DiagnosticSeverity.Error,
            _ => descriptor.DefaultSeverity,
        };
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

    /// <summary>
    /// Describes a set of placeholders the way they appear in the value of a resource.
    /// </summary>
    /// <param name="indexes">The argument indexes to describe, which must not be empty.</param>
    /// <returns>The description of the placeholders, to embed in a diagnostic message.</returns>
    private static string DescribePlaceholders(IReadOnlyList<int> indexes)
    {
        var placeholders = indexes.Select(index => $"{{{index.ToString(CultureInfo.InvariantCulture)}}}");

        return $"the placeholder{(indexes.Count == 1 ? "" : "s")} {string.Join(", ", placeholders)}";
    }
}
