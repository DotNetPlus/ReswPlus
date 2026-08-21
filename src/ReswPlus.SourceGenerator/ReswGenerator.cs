using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using ReswPlus.SourceGenerator.ClassGenerators;
using ReswPlus.SourceGenerator.Plurals;
using ReswPlus.SourceGenerator.CodeGenerators;
using ReswPlus.SourceGenerator.Models;
using ReswPlus.SourceGenerator.Pipeline;

#if DEBUG
using System.Diagnostics;
#endif

namespace ReswPlus.SourceGenerator;

public enum AppType
{
    Unknown,
    WindowsAppSDK,
    UWP,
}

/// <summary>
/// Generates the strongly typed classes of the <c>.resw</c> files of a project.
/// </summary>
/// <remarks>
/// The pipeline is split so that editing one resource file only costs the work of that file. Everything that
/// can be decided without reading a resource file -- the properties of the project, the kind of app, which
/// file of each resource carries the default language -- is decided in a stage of its own, and the reading,
/// parsing and generation of a resource file happens in a stage that runs once per file. The compiler keeps
/// the result of every stage between runs and only reruns the ones whose inputs changed, so how the stages are
/// split is what decides the cost of a keystroke in the IDE.
/// </remarks>
[Generator]
public partial class ReswSourceGenerator : IIncrementalGenerator
{
    /// <summary>
    /// The templates, decoded once.
    /// </summary>
    /// <remarks>
    /// The templates are immutable resources baked into this assembly, and the compiler keeps a generator alive
    /// for as long as the project is open, so reading and decoding them off a manifest stream on every run is
    /// pure repetition.
    /// </remarks>
    private static readonly ConcurrentDictionary<string, string> Templates = new(StringComparer.Ordinal);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
        if (!Debugger.IsAttached)
        {
            // Uncomment the following line to debug the source generator.
            // Debugger.Launch();
        }
#endif

        // The properties of the project are read into a value of their own: the options provider is a new
        // object on every run, so anything reading from it directly would be recomputed on every keystroke.
        var options = context.AnalyzerConfigOptionsProvider
            .Select(static (provider, _) => ReswBuildOptions.Read(provider.GlobalOptions))
            .WithTrackingName(TrackingNames.Options);

        // Only the parts of the compilation the generation actually depends on are captured, for the same
        // reason: a Compilation is a new object every time.
        var compilationInfo = context.CompilationProvider
            .Select(static (compilation, _) =>
                new CompilationInfo(compilation is CSharpCompilation, RetrieveAppType(compilation), compilation.AssemblyName))
            .WithTrackingName(TrackingNames.CompilationInfo);

        var project = compilationInfo
            .Combine(options)
            .Select(static (pair, _) => ReswProject.Create(pair.Left, pair.Right))
            .WithTrackingName(TrackingNames.Project);

        var resourceFiles = context.AdditionalTextsProvider
            .Where(static file => Path.GetExtension(file.Path).Equals(".resw", StringComparison.OrdinalIgnoreCase));

        // The layout is derived from the paths alone, never from the content of the files, so that editing a
        // string does not make the whole project regroup.
        var layout = resourceFiles
            .Select(static (file, _) => file.Path)
            .WithTrackingName(TrackingNames.Paths)
            .Collect()
            .Combine(project)
            .Select(static (pair, _) => ReswLayout.Create(pair.Left, pair.Right.DefaultLanguage, pair.Right.GetNamespace))
            .WithTrackingName(TrackingNames.Layout);

        // Which file of a resource the code is generated from, and what to call the file generated from it,
        // are read out of the layout here. That is cheap, so it can afford to run whenever the layout changes.
        var toGenerate = resourceFiles
            .Combine(project)
            .Combine(layout)
            .Select(static (input, _) => new ReswFileToGenerate(
                input.Left.Left,
                input.Left.Right,
                input.Right.GetHintName(input.Left.Left.Path)))
            .WithTrackingName(TrackingNames.FilesToGenerate);

        // One run per resource file, so that editing one of them leaves the others alone. This is the step that
        // parses and formats, and it is kept clear of the layout: a resource nobody touched compares equal to
        // what it was and is not generated again because a different resource appeared beside it.
        //
        // The tracking name sits on the step that does the work, not on the filtering after it: a name further
        // down reports the filtered result being reused even when the work above it ran again.
        var generated = toGenerate
            .Select(static (input, cancellationToken) => GenerateFile(input, cancellationToken))
            .WithTrackingName(TrackingNames.Generation)
            .Where(static file => file is not null)
            .Select(static (file, _) => file!);

        // Which support sources the project needs can only be decided once every file has been generated, but
        // it comes down to a handful of flags that almost never change while a project is edited.
        var support = generated
            .Collect()
            .Combine(project)
            .Combine(layout)
            .Select(static (input, _) => ReswSupport.Create(input.Left.Left, input.Left.Right, input.Right))
            .WithTrackingName(TrackingNames.Support);

        context.RegisterSourceOutput(project, static (spc, project) => ReportSetupProblems(spc, project));
        context.RegisterSourceOutput(generated, static (spc, file) => EmitGeneratedFile(spc, file));
        context.RegisterSourceOutput(support, static (spc, support) => EmitSupport(spc, support));
    }

    /// <summary>
    /// Reports what is wrong with the setup of the project, if anything is.
    /// </summary>
    private static void ReportSetupProblems(SourceProductionContext spc, ReswProject project)
    {
        foreach (var id in project.SetupProblems)
        {
            spc.ReportDiagnostic(Diagnostic.Create(Diagnostics.GetDescriptor(id), Location.None));
        }
    }

    /// <summary>
    /// Generates the code of one resource file.
    /// </summary>
    /// <param name="input">The resource file, and what generating it depends on.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// The generated code, or <see langword="null"/> when the file holds a translation rather than the
    /// language the code is generated from, or when the project is not one ReswPlus supports.
    /// </returns>
    private static ReswGeneratedFile? GenerateFile(ReswFileToGenerate input, CancellationToken cancellationToken)
    {
        var file = input.File;
        var project = input.Project;

        if (!project.IsSupported || input.HintName is not { } hintName)
        {
            return null;
        }

        var resourceFileInfo = new ResourceFileInfo(file.Path, new Project(project.AssemblyName, project.IsLibrary));
        var codeGenerator = ReswClassGenerator.CreateGenerator(resourceFileInfo, null);

        if (codeGenerator is null)
        {
            return null;
        }

        var content = file.GetText(cancellationToken)?.ToString() ?? "";

        cancellationToken.ThrowIfCancellationRequested();

        // Whatever a resource file holds, and however malformed it is, it must not take the generation of the
        // rest of the project down with it: an exception escaping here is reported by the compiler as a single
        // CS8785 that names neither the file nor the reason, after which no resource file of the project is
        // generated at all.
        try
        {
            var generated = codeGenerator.GenerateCode(
                baseFilename: Path.GetFileName(file.Path).Split('.')[0],
                content: content,
                defaultNamespace: project.GetNamespace(file.Path),
                isAdvanced: true,
                appType: project.AppType);

            if (generated?.Files.FirstOrDefault() is not { } generatedFile)
            {
                return null;
            }

            return ReswGeneratedFile.Generated(
                file.Path,
                hintName,
                generatedFile.Content,
                generated.ContainsMacro,
                generated.ContainsPlural);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return ReswGeneratedFile.Failed(file.Path, hintName, exception.Message);
        }
    }

    private static void EmitGeneratedFile(SourceProductionContext spc, ReswGeneratedFile file)
    {
        if (file.Content is null)
        {
            spc.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.ResourceFileNotProcessed,
                CreateFileLocation(file.SourcePath),
                Path.GetFileName(file.SourcePath),
                file.Error));

            return;
        }

        spc.AddSource(file.HintName, SourceText.From(file.Content, Encoding.UTF8));
    }

    /// <summary>
    /// Emits the sources shared by every resource file of the project.
    /// </summary>
    private static void EmitSupport(SourceProductionContext spc, ReswSupport support)
    {
        if (!support.IsSupported)
        {
            return;
        }

        var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // The support sources are shared by every resource file of the project, so they are emitted once.
        // Adding the same hint name twice throws, which used to make the generator produce nothing at all for
        // a project holding more than one .resw that uses macros or plurals.
        var emitted = new HashSet<string>(StringComparer.Ordinal);

        AddSourceFromResource(
            spc,
            emitted,
            support.AppType == AppType.WindowsAppSDK
                ? $"{assemblyName}.Templates.ResourceStringProviders.MicrosoftResourceStringProvider.txt"
                : $"{assemblyName}.Templates.ResourceStringProviders.WindowsResourceStringProvider.txt",
            "ResourceStringProvider");

        if (support.NeedsMacros)
        {
            AddSourceFromResource(spc, emitted, $"{assemblyName}.Templates.Macros.Macros.txt", "Macros");
        }

        if (!support.NeedsPlurals)
        {
            return;
        }

        AddSourceFromResource(spc, emitted, $"{assemblyName}.Templates.Plurals.IPluralProvider.txt", "IPluralProvider");
        AddSourceFromResource(spc, emitted, $"{assemblyName}.Templates.Plurals.PluralTypeEnum.txt", "PluralTypeEnum");
        AddSourceFromResource(spc, emitted, $"{assemblyName}.Templates.Utils.IntExt.txt", "IntExt");
        AddSourceFromResource(spc, emitted, $"{assemblyName}.Templates.Utils.DoubleExt.txt", "DoubleExt");

        AddLanguageSupport(spc, emitted, support, assemblyName!);
    }

    /// <summary>
    /// Determines the application type (WindowsAppSDK, UWP, or Unknown) by inspecting the compilation's external references.
    /// </summary>
    private static AppType RetrieveAppType(Compilation compilation)
    {
        return compilation.ExternalReferences.Any(r =>
            r.Display?.IndexOf("Microsoft.WindowsAppSdk", StringComparison.OrdinalIgnoreCase) >= 0)
            ? AppType.WindowsAppSDK
            : compilation.ExternalReferences.Any(r =>
            r.Display?.IndexOf("Windows.Foundation.UniversalApiContract", StringComparison.OrdinalIgnoreCase) >= 0)
            ? AppType.UWP
            : AppType.Unknown;
    }

    /// <summary>
    /// Adds the plural support of the languages the project holds.
    /// </summary>
    private static void AddLanguageSupport(SourceProductionContext spc, HashSet<string> emittedSources, ReswSupport support, string assemblyName)
    {
        try
        {
            AddPluralSupport(spc, emittedSources, support, assemblyName);
        }
        catch (Exception error)
        {
            // The rules are a table generated ahead of time, so nothing a project does can make emitting them
            // fail; a bug in ReswPlus can. Reporting it keeps the failure named and keeps it from taking every
            // other shared source down with it.
            spc.ReportDiagnostic(Diagnostic.Create(Diagnostics.PluralRulesNotRead, Location.None, error.Message));
        }
    }

    private static void AddPluralSupport(SourceProductionContext spc, HashSet<string> emittedSources, ReswSupport support, string assemblyName)
    {
        var languages = support.Languages.Select(language => language.Name).ToArray();
        var mappings = new List<(string LanguageTag, string ProviderId)>();

        // The single-form provider backs the fallback of the selector, which a language reaches by matching no
        // case at all, so it is emitted once up front and reused.
        AddProvider(spc, emittedSources, "Other", OtherProviderSource);

        foreach (var pluralFile in PluralFormsRetriever.RetrievePluralFormsForLanguages(languages))
        {
            AddProvider(spc, emittedSources, pluralFile.Id, pluralFile.Source);

            // Add each language handled by this provider.
            foreach (var lng in pluralFile.Languages)
            {
                mappings.Add((lng, pluralFile.Id));
            }
        }

        var pluralSelectorCode = PluralSelector.Build(mappings);

        // Report the languages that have no rules, so that falling back to a single plural form is a visible
        // choice rather than a silent one. Each one is reported against a resource file of that language,
        // rather than against nothing, so that it has a place to point at and can be configured per file.
        var filesByLanguage = support.Languages.ToDictionary(language => language.Name, language => language.Path, StringComparer.Ordinal);

        foreach (var language in PluralFormsRetriever.RetrieveLanguagesWithoutPluralForm(languages))
        {
            spc.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.UnknownPluralLanguage,
                CreateFileLocation(filesByLanguage[language]),
                language));
        }

        // Build and add the ResourceLoaderExtension with the plural selector injected.
        var hintName = $"ResourceLoaderExtension{GeneratedCode.FileExtension}";

        if (!emittedSources.Add(hintName))
        {
            return;
        }

        var resourceLoaderCode = ReadTemplate($"{assemblyName}.Templates.Plurals.ResourceLoaderExtension.txt")
            .Replace("{{PluralProviderSelector}}", pluralSelectorCode)
            .Replace("{{PluralLanguageResolver}}", PluralLanguageResolvers.GetResolver(support.UseApplicationLanguages, support.AppType));

        spc.AddSource(hintName, SourceText.From(GeneratedCode.AddFileHeader(resourceLoaderCode), Encoding.UTF8));
    }

    /// <summary>
    /// Creates a location pointing at the start of a file, so that a diagnostic about it has somewhere to go.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <returns>A location at the start of the file.</returns>
    private static Location CreateFileLocation(string path)
    {
        var start = new LinePosition(0, 0);

        return Location.Create(path, new TextSpan(0, 0), new LinePositionSpan(start, start));
    }

    /// <summary>
    /// The source of the provider a language with no rules of its own falls back to.
    /// </summary>
    private const string OtherProviderSource = """
        namespace _ReswPlus_AutoGenerated.Plurals
        {
            /// <summary>The rules of a language that declines the same whatever the quantity.</summary>
            internal sealed class OtherProvider : IPluralProvider
            {
                public PluralTypeEnum ComputePlural(double n)
                {
                    return PluralTypeEnum.OTHER;
                }
            }
        }
        """;

    /// <summary>
    /// Adds the source of a plural provider, unless it was already added.
    /// </summary>
    /// <param name="spc">The context to add the source to.</param>
    /// <param name="emittedSources">The hint names already emitted for the project.</param>
    /// <param name="providerId">The identifier of the provider, without the <c>Provider</c> suffix.</param>
    /// <param name="source">The source of the provider, as the importer wrote it.</param>
    private static void AddProvider(SourceProductionContext spc, HashSet<string> emittedSources, string providerId, string source)
    {
        var hintName = $"{providerId}Provider{GeneratedCode.FileExtension}";

        if (!emittedSources.Add(hintName))
        {
            return;
        }

        spc.AddSource(hintName, SourceText.From(GeneratedCode.AddFileHeader(source), Encoding.UTF8));
    }

    /// <summary>
    /// Reads a template and adds its content as a source file, unless it was already added.
    /// </summary>
    /// <param name="spc">The context to add the source to.</param>
    /// <param name="emittedSources">The hint names already emitted for the project.</param>
    /// <param name="resourcePath">The path of the embedded resource holding the template.</param>
    /// <param name="typeName">The name of the type declared by the template, used as the hint name.</param>
    private static void AddSourceFromResource(SourceProductionContext spc, HashSet<string> emittedSources, string resourcePath, string typeName)
    {
        var hintName = $"{typeName}{GeneratedCode.FileExtension}";

        if (!emittedSources.Add(hintName))
        {
            return;
        }

        var template = ReadTemplate(resourcePath);

        if (template.Length == 0)
        {
            return;
        }

        spc.AddSource(hintName, SourceText.From(GeneratedCode.AddFileHeader(template), Encoding.UTF8));
    }

    /// <summary>
    /// Reads a template out of the embedded resources of this assembly.
    /// </summary>
    /// <param name="resourcePath">The path of the embedded resource holding the template.</param>
    /// <returns>The content of the template, or an empty string when there is no such resource.</returns>
    private static string ReadTemplate(string resourcePath)
    {
        return Templates.GetOrAdd(resourcePath, static path =>
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);

            if (stream is null)
            {
                return "";
            }

            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        });
    }
}
