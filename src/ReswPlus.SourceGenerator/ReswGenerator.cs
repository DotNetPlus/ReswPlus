using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using ReswPlus.SourceGenerator.Analysis;
using ReswPlus.SourceGenerator.ClassGenerators;
using ReswPlus.SourceGenerator.CodeGenerators;
using ReswPlus.SourceGenerator.Models;
using Microsoft.CodeAnalysis.Diagnostics;

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

[Generator]
public partial class ReswSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
        if (!Debugger.IsAttached)
        {
            // Uncomment the following line to debug the source generator.
            // Debugger.Launch();
        }
#endif

        // Create a provider for global analyzer config options.
        var globalOptionsProvider = context.AnalyzerConfigOptionsProvider.Select((options, cancellationToken) => new
        {
            ProjectDir = GetOption(options.GlobalOptions, "build_property.projectdir"),
            MSBuildProjectFullPath = GetOption(options.GlobalOptions, "build_property.MSBuildProjectFullPath"),
            OutputType = GetOption(options.GlobalOptions, "build_property.OutputType"),
            ProjectTypeGuids = GetOption(options.GlobalOptions, "build_property.projecttypeguids"),
            DefaultLanguage = GetOption(options.GlobalOptions, "build_property.DefaultLanguage"),
            RootNamespace = GetOption(options.GlobalOptions, "build_property.RootNamespace"),
            UseApplicationLanguages = GetOption(options.GlobalOptions, "build_property.ReswPlusUseApplicationLanguages")
        });

        // Provider for additional files with .resw extension.
        var reswFilesProvider = context.AdditionalTextsProvider
            .Where(file => Path.GetExtension(file.Path).Equals(".resw", StringComparison.OrdinalIgnoreCase))
            .Collect();

        // Only the parts of the compilation the generation actually depends on are captured, so that an unrelated
        // edit in the project doesn't invalidate the whole pipeline: a Compilation is a new object every time.
        var compilationInfoProvider = context.CompilationProvider.Select(static (compilation, cancellationToken) =>
            new CompilationInfo(compilation is CSharpCompilation, RetrieveAppType(compilation), compilation.AssemblyName));

        // Combine the compilation information, the global options, and the additional files.
        var combinedProvider = compilationInfoProvider
            .Combine(globalOptionsProvider)
            .Combine(reswFilesProvider);

        context.RegisterSourceOutput(combinedProvider, static (spc, source) =>
        {
            // Unpack the combined tuple.
            var ((compilationInfo, options), additionalFiles) = source;

            if (options is null)
            {
                return;
            }

            // Only support C#
            if (!compilationInfo.IsCSharp)
            {
                spc.ReportDiagnostic(Diagnostic.Create(Diagnostics.UnsupportedLanguage, Location.None));
                return;
            }

            // Retrieve project root path.
            var projectRootPath = options.ProjectDir;
            if (projectRootPath is not { Length: > 0 } && options.MSBuildProjectFullPath is { Length: > 0 })
            {
                projectRootPath = Path.GetDirectoryName(options.MSBuildProjectFullPath);
            }

            if (string.IsNullOrEmpty(projectRootPath))
            {
                spc.ReportDiagnostic(Diagnostic.Create(Diagnostics.MissingRootPath, Location.None));
                return;
            }

            // Determine if the project is a library.
            bool isLibrary = false;
            if (options.OutputType is { Length: > 0 })
            {
                isLibrary = options.OutputType.Equals("library", StringComparison.OrdinalIgnoreCase)
                         || options.OutputType.Equals("module", StringComparison.OrdinalIgnoreCase);
            }
            else if (options.ProjectTypeGuids is { Length: > 0 })
            {
                isLibrary = options.ProjectTypeGuids.Equals("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}", StringComparison.OrdinalIgnoreCase)
                         || options.ProjectTypeGuids.Equals("{BC8A1FFA-BEE3-4634-8014-F334798102B3}", StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                spc.ReportDiagnostic(Diagnostic.Create(Diagnostics.UnknownProjectType, Location.None));
            }

            // Determine AppType based on referenced assemblies.
            var appType = compilationInfo.AppType;
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

            // The support sources are shared by every resource file of the project, so they are emitted once.
            // Adding the same hint name twice throws, which used to make the generator produce nothing at all
            // for a project holding more than one .resw that uses macros or plurals.
            var emittedSources = new HashSet<string>(StringComparer.Ordinal);

            // Opt in to reading the plural language from the app runtime language list, the same list the
            // resources themselves are resolved against.
            var useApplicationLanguages =
                bool.TryParse(options.UseApplicationLanguages, out var parsedUseApplicationLanguages) && parsedUseApplicationLanguages;

            switch (appType)
            {
                case AppType.WindowsAppSDK:
                    AddSourceFromResource(spc, emittedSources, $"{assemblyName}.Templates.ResourceStringProviders.MicrosoftResourceStringProvider.txt", "ResourceStringProvider");
                    break;
                case AppType.UWP:
                    AddSourceFromResource(spc, emittedSources, $"{assemblyName}.Templates.ResourceStringProviders.WindowsResourceStringProvider.txt", "ResourceStringProvider");
                    break;
                default:
                    spc.ReportDiagnostic(Diagnostic.Create(Diagnostics.UnrecognizedAppType, Location.None));
                    return;
            }

            // Retrieve the default language (optional)
            var projectDefaultLanguage = options.DefaultLanguage;

            // Retrieve the project's root namespace.
            if (string.IsNullOrEmpty(options.RootNamespace))
            {
                spc.ReportDiagnostic(Diagnostic.Create(Diagnostics.UnknownNamespace, Location.None));
                return;
            }
            var projectRootNamespace = options.RootNamespace!;

            // Process all .resw additional files.
            var allResourceFiles = additionalFiles.Distinct().ToArray();

            // Group files and retrieve the default resource file per group.
            var defaultLanguageResourceFiles = (from fileGroup in ReswFileGrouping.GroupByResource(allResourceFiles.Select(file => file.Path))
                                                let defaultFile = ReswFileGrouping.RetrieveDefaultResourceFile(
                                                    fileGroup,
                                                    projectDefaultLanguage)
                                                where defaultFile != null
                                                select defaultFile).ToArray();

            // Gather all distinct languages, keeping a resource file for each so that a diagnostic about a
            // language has somewhere to point. The folder of a resource is reduced to its language exactly the
            // way the generated code reduces the language of the app, so that the two always agree: a
            // culture-sensitive ToLower would turn 'IS-IS' into 'ıs' under Turkish and never match again.
            var resourceFilePerLanguage = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var resourceFile in allResourceFiles)
            {
                var language = Path.GetFileName(Path.GetDirectoryName(resourceFile.Path)).Split('-', '_')[0].ToLowerInvariant();
                if (!resourceFilePerLanguage.ContainsKey(language))
                {
                    resourceFilePerLanguage.Add(language, resourceFile.Path);
                }
            }

            // Process each default resource file.
            foreach (var filePath in defaultLanguageResourceFiles)
            {
                // Determine namespace for the generated class.
                var namespaceForReswFile = projectRootNamespace;
                var reswParentDirectory = Path.GetDirectoryName(filePath);
                if (reswParentDirectory != null && reswParentDirectory.StartsWith(projectRootPath, StringComparison.OrdinalIgnoreCase))
                {
                    var additionalNamespace = reswParentDirectory.Substring(projectRootPath!.Length)
                        .Trim(Path.DirectorySeparatorChar)
                        .Replace(Path.DirectorySeparatorChar, '.');
                    if (!string.IsNullOrEmpty(additionalNamespace))
                    {
                        namespaceForReswFile += "." + additionalNamespace;
                    }
                }

                // Get the additional file matching this path.
                var additionalText = allResourceFiles.FirstOrDefault(f => f.Path == filePath);
                if (additionalText is null)
                {
                    continue;
                }

                // Generate code for the resource file.
                var resourceFileInfo = new ResourceFileInfo(filePath, new Project(compilationInfo.AssemblyName!, isLibrary));
                var codeGenerator = ReswClassGenerator.CreateGenerator(resourceFileInfo, null);
                if (codeGenerator is null)
                {
                    continue;
                }

                var baseFilename = Path.GetFileName(filePath).Split('.')[0];
                var text = additionalText.GetText(spc.CancellationToken)?.ToString() ?? "";

                GenerationResult? generatedData;

                // Whatever a resource file holds, and however malformed it is, it must not take the generation
                // of the rest of the project down with it: an exception escaping here is reported by the
                // compiler as a single CS8785 that names neither the file nor the reason, and no resource file
                // of the project is generated at all.
                try
                {
                    generatedData = codeGenerator.GenerateCode(
                        baseFilename: baseFilename,
                        content: text,
                        defaultNamespace: namespaceForReswFile,
                        isAdvanced: true,
                        appType: appType);
                }
                catch (Exception exception)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.ResourceFileNotProcessed,
                        CreateFileLocation(filePath),
                        Path.GetFileName(filePath),
                        exception.Message));

                    continue;
                }

                if (generatedData is null)
                {
                    continue;
                }

                // Add each generated file as a new source. The hint name is qualified with the namespace of the
                // resource file, because two .resw files with the same name can live in different folders.
                foreach (var generatedFile in generatedData.Files)
                {
                    var hintName = $"{namespaceForReswFile}.{Path.GetFileName(filePath)}{GeneratedCode.FileExtension}";
                    if (emittedSources.Add(hintName))
                    {
                        spc.AddSource(hintName, SourceText.From(generatedFile.Content, Encoding.UTF8));
                    }
                }

                // If macros were used, include the Macros source file.
                if (generatedData.ContainsMacro)
                {
                    AddSourceFromResource(spc, emittedSources, "ReswPlus.SourceGenerator.Templates.Macros.Macros.txt", "Macros");
                }

                // If plural forms are detected, add plural-related support sources.
                if (generatedData.ContainsPlural)
                {
                    AddSourceFromResource(spc, emittedSources, $"{assemblyName}.Templates.Plurals.IPluralProvider.txt", "IPluralProvider");
                    AddSourceFromResource(spc, emittedSources, $"{assemblyName}.Templates.Plurals.PluralTypeEnum.txt", "PluralTypeEnum");
                    AddSourceFromResource(spc, emittedSources, $"{assemblyName}.Templates.Utils.IntExt.txt", "IntExt");
                    AddSourceFromResource(spc, emittedSources, $"{assemblyName}.Templates.Utils.DoubleExt.txt", "DoubleExt");
                    AddLanguageSupport(spc, emittedSources, resourceFilePerLanguage, useApplicationLanguages, appType);
                }
            }
        });
    }

    /// <summary>
    /// Helper method to retrieve an option value.
    /// </summary>
    private static string? GetOption(AnalyzerConfigOptions globalOptions, string key)
    {
        return globalOptions.TryGetValue(key, out var value) ? value : null;
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
    /// Adds language support sources for pluralization based on the provided languages.
    /// </summary>
    /// <param name="resourceFilePerLanguage">A resource file of each language the project holds.</param>
    private static void AddLanguageSupport(SourceProductionContext spc, HashSet<string> emittedSources, Dictionary<string, string> resourceFilePerLanguage, bool useApplicationLanguages, AppType appType)
    {
        // The whole plural support is shared by every resource file of the project, so it is built once.
        if (!emittedSources.Add($"ResourceLoaderExtension{GeneratedCode.FileExtension}"))
        {
            return;
        }

        var pluralSelectorCode = "default:\n  return new _ReswPlus_AutoGenerated.Plurals.OtherProvider();\n";
        var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // The single-form provider backs the default branch of the selector, and the languages that only have
        // that form are mapped to it explicitly, so it is emitted once up front and reused.
        AddSourceFromResource(spc, emittedSources, $"{assemblyName}.Templates.Plurals.OtherProvider.txt", "OtherProvider");

        foreach (var pluralFile in PluralFormsRetriever.RetrievePluralFormsForLanguages(resourceFilePerLanguage.Keys))
        {
            var resourceName = $"{assemblyName}.Templates.Plurals.{pluralFile.Id}Provider.txt";
            AddSourceFromResource(spc, emittedSources, resourceName, $"{pluralFile.Id}Provider");

            // Add each language handled by this provider.
            foreach (var lng in pluralFile.Languages)
            {
                pluralSelectorCode += $"case \"{lng}\":\n";
            }
            pluralSelectorCode += $"  return new _ReswPlus_AutoGenerated.Plurals.{pluralFile.Id}Provider();\n";
        }

        // Report the languages that have no rules, so that falling back to a single plural form is a visible
        // choice rather than a silent one. Each one is reported against a resource file of that language,
        // rather than against nothing, so that it has a place to point at and can be configured per file.
        foreach (var language in PluralFormsRetriever.RetrieveLanguagesWithoutPluralForm(resourceFilePerLanguage.Keys))
        {
            spc.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.UnknownPluralLanguage,
                CreateFileLocation(resourceFilePerLanguage[language]),
                language));
        }

        // Build and add the ResourceLoaderExtension with the plural selector injected.
        var resourceLoaderResourceName = $"{assemblyName}.Templates.Plurals.ResourceLoaderExtension.txt";
        var resourceLoaderTemplate = ReadAllText(Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceLoaderResourceName));
        var resourceLoaderCode = resourceLoaderTemplate
            .Replace("{{PluralProviderSelector}}", pluralSelectorCode)
            .Replace("{{PluralLanguageResolver}}", PluralLanguageResolvers.GetResolver(useApplicationLanguages, appType));
        spc.AddSource($"ResourceLoaderExtension{GeneratedCode.FileExtension}", SourceText.From(GeneratedCode.AddFileHeader(resourceLoaderCode), Encoding.UTF8));
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
    /// Reads a resource stream and adds its content as a source file, unless it was already added.
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

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
        if (stream is null)
        {
            // Optionally, report a diagnostic or throw if the resource is missing.
            return;
        }
        var sourceText = ReadAllText(stream);
        spc.AddSource(hintName, SourceText.From(GeneratedCode.AddFileHeader(sourceText), Encoding.UTF8));
    }

    /// <summary>
    /// Reads all text from the provided stream.
    /// </summary>
    private static string ReadAllText(Stream stream)
    {
        _ = stream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
