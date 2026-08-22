using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace ReswPlus.SourceGenerator.Analysis;

/// <summary>
/// Reports the problems of the <c>.resw</c> files of a project that would otherwise only surface at runtime, in
/// a language the team may not read.
/// </summary>
/// <remarks>
/// This is deliberately an analyzer and not part of the source generator. A generator runs on every keystroke in
/// the IDE, and inspecting every <c>.resw</c> file of every language of a project is far too expensive to do
/// there. Analyzers are scheduled independently of the generator pipeline, run out of process, and can be turned
/// off per rule by the consumer, so the cost is both smaller and opt out.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReswResourceAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        Diagnostics.PlaceholderMismatch,
        Diagnostics.UndeclaredFormatParameter,
        Diagnostics.MissingPluralForms,
        Diagnostics.DuplicateResource,
        Diagnostics.InvalidFormatString,
        Diagnostics.ReservedResourceName,
        Diagnostics.DuplicateFormatParameter,
        Diagnostics.MissingTranslation,
        Diagnostics.TranslationWithoutDefault,
        Diagnostics.UnchangedTranslation,
        Diagnostics.IncompatibleTranslationShape,
        Diagnostics.ExtraTranslationVariants
    ];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        // The rules compare the resources of a language against the resources of the default language, so they
        // need the whole set of files at once and are registered as a single action per compilation.
        context.RegisterCompilationAction(AnalyzeCompilation);
    }

    private static void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        var reswFiles = new List<(string Path, SourceText Text)>();

        foreach (var additionalFile in context.Options.AdditionalFiles)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (!Path.GetExtension(additionalFile.Path).Equals(".resw", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (additionalFile.GetText(context.CancellationToken) is { } text)
            {
                reswFiles.Add((additionalFile.Path, text));
            }
        }

        if (reswFiles.Count == 0)
        {
            return;
        }

        var globalOptions = context.Options.AnalyzerConfigOptionsProvider.GlobalOptions;
        var defaultLanguage = globalOptions.TryGetValue("build_property.DefaultLanguage", out var value)
            ? value
            : null;
        var generateResourceInterfaces =
            !globalOptions.TryGetValue("build_property.ReswPlusGenerateResourceInterfaces", out var interfaceOption)
            || !bool.TryParse(interfaceOption, out var parsedInterfaces)
            || parsedInterfaces;
        var translationChecks = ReswTranslationChecksParser.Parse(
            globalOptions.TryGetValue("build_property.ReswPlusTranslationChecks", out var checks)
                ? checks
                : null);

        ReswResourceRules.Analyze(
            reswFiles,
            defaultLanguage,
            generateResourceInterfaces,
            translationChecks,
            context.ReportDiagnostic,
            context.CancellationToken);
    }
}
