using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using ReswPlus.SourceGenerator.ClassGenerators;
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
    // Diagnostic descriptors (could be moved to a central location if reused)
    private static readonly DiagnosticDescriptor UnsupportedLanguageDiagnostic =
        new(
            "RESWP0001",
            "Language not supported",
            "ReswPlus source generator only supports C#",
            "Compatibility",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor UnknownNamespaceDiagnostic =
        new(
            "RESWP0002",
            "Unknown namespace",
            "ReswPlus cannot determine the namespace",
            "Compatibility",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor MissingRootPathDiagnostic =
        new(
            "RESWP0003",
            "Root path missing",
            "Can't retrieve the root path of the project",
            "Compatibility",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor UnknownProjectTypeDiagnostic =
        new(
            "RESWP0004",
            "Unknown Project Type",
            "ReswPlus cannot determine the project type, defaulting to application",
            "Compatibility",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor UnrecognizedAppTypeDiagnostic =
        new(
            "RESWP0005",
            "Project type not recognized",
            "ReswPlus only supports UWP and WinAppSDK applications/libraries",
            "Compatibility",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

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
            RootNamespace = GetOption(options.GlobalOptions, "build_property.RootNamespace")
        });

        // Provider for additional files with .resw extension.
        var reswFilesProvider = context.AdditionalTextsProvider
            .Where(file => Path.GetExtension(file.Path).Equals(".resw", StringComparison.OrdinalIgnoreCase))
            .Collect();

        // Combine the Compilation, the global options, and the additional files.
        var combinedProvider = context.CompilationProvider
            .Combine(globalOptionsProvider)
            .Combine(reswFilesProvider);

        context.RegisterSourceOutput(combinedProvider, (spc, source) =>
        {
            // Unpack the combined tuple.
            var ((compilation, options), additionalFiles) = source;

            if (compilation is null || options is null)
            {
                return;
            }

            // Only support C#
            if (compilation is not CSharpCompilation)
            {
                spc.ReportDiagnostic(Diagnostic.Create(UnsupportedLanguageDiagnostic, Location.None));
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
                spc.ReportDiagnostic(Diagnostic.Create(MissingRootPathDiagnostic, Location.None));
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
                spc.ReportDiagnostic(Diagnostic.Create(UnknownProjectTypeDiagnostic, Location.None));
            }

            // Determine AppType based on referenced assemblies.
            var appType = RetrieveAppType(compilation);
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

            switch (appType)
            {
                case AppType.WindowsAppSDK:
                    AddSourceFromResource(spc, $"{assemblyName}.Templates.ResourceStringProviders.MicrosoftResourceStringProvider.txt", "ResourceStringProvider.cs");
                    break;
                case AppType.UWP:
                    AddSourceFromResource(spc, $"{assemblyName}.Templates.ResourceStringProviders.WindowsResourceStringProvider.txt", "ResourceStringProvider.cs");
                    break;
                default:
                    spc.ReportDiagnostic(Diagnostic.Create(UnrecognizedAppTypeDiagnostic, Location.None));
                    return;
            }

            // Retrieve the default language (optional)
            var projectDefaultLanguage = options.DefaultLanguage;

            // Retrieve the project's root namespace.
            if (string.IsNullOrEmpty(options.RootNamespace))
            {
                spc.ReportDiagnostic(Diagnostic.Create(UnknownNamespaceDiagnostic, Location.None));
                return;
            }
            var projectRootNamespace = options.RootNamespace!;

            // Process all .resw additional files.
            var allResourceFiles = additionalFiles.Distinct().ToArray();

            // Group files and retrieve the default resource file per group.
            var defaultLanguageResourceFiles = (from file in allResourceFiles
                                                group file by
                                                  Path.Combine(
                                                      Path.GetDirectoryName(Path.GetDirectoryName(file.Path)),
                                                      Path.GetFileName(file.Path))
                                                into fileGroup
                                                let defaultFile = RetrieveDefaultResourceFile(
                                                    fileGroup.Select(f => f.Path),
                                                    projectDefaultLanguage)
                                                where defaultFile != null
                                                select defaultFile).ToArray();

            // Gather all distinct languages.
            var allLanguages = allResourceFiles
                .Select(f => Path.GetFileName(Path.GetDirectoryName(f.Path)).Split('-')[0].ToLower())
                .Distinct()
                .ToArray();

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
                var resourceFileInfo = new ResourceFileInfo(filePath, new Project(compilation.AssemblyName!, isLibrary));
                var codeGenerator = ReswClassGenerator.CreateGenerator(resourceFileInfo, null);
                if (codeGenerator is null)
                {
                    continue;
                }

                var baseFilename = Path.GetFileName(filePath).Split('.')[0];
                var text = additionalText.GetText(spc.CancellationToken)?.ToString() ?? "";
                var generatedData = codeGenerator.GenerateCode(
                    baseFilename: baseFilename,
                    content: text,
                    defaultNamespace: namespaceForReswFile,
                    isAdvanced: true,
                    appType: appType);

                if (generatedData is null)
                {
                    continue;
                }

                // Add each generated file as a new source.
                foreach (var generatedFile in generatedData.Files)
                {
                    spc.AddSource($"{Path.GetFileName(filePath)}.cs", SourceText.From(generatedFile.Content, Encoding.UTF8));
                }

                // If macros were used, include the Macros source file.
                if (generatedData.ContainsMacro)
                {
                    AddSourceFromResource(spc, "ReswPlus.SourceGenerator.Templates.Macros.Macros.txt", "Macros.cs");
                }

                // If plural forms are detected, add plural-related support sources.
                if (generatedData.ContainsPlural)
                {
                    AddSourceFromResource(spc, $"{assemblyName}.Templates.Plurals.IPluralProvider.txt", "IPluralProvider.cs");
                    AddSourceFromResource(spc, $"{assemblyName}.Templates.Plurals.PluralTypeEnum.txt", "PluralTypeEnum.cs");
                    AddSourceFromResource(spc, $"{assemblyName}.Templates.Utils.IntExt.txt", "IntExt.cs");
                    AddSourceFromResource(spc, $"{assemblyName}.Templates.Utils.DoubleExt.txt", "DoubleExt.cs");
                    AddLanguageSupport(spc, allLanguages);
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
    /// Retrieve the default resource file from the given list that matches one of the preferred languages.
    /// </summary>
    private static string? RetrieveDefaultResourceFile(IEnumerable<string> reswFiles, string? defaultLanguage)
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
    private static void AddLanguageSupport(SourceProductionContext spc, string[] languagesSupported)
    {
        var pluralSelectorCode = "default:\n  return new _ReswPlus_AutoGenerated.Plurals.OtherProvider();\n";
        var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        foreach (var pluralFile in PluralFormsRetriever.RetrievePluralFormsForLanguages(languagesSupported))
        {
            var resourceName = $"{assemblyName}.Templates.Plurals.{pluralFile.Id}Provider.txt";
            AddSourceFromResource(spc, resourceName, $"{pluralFile.Id}Provider.cs");

            // Add each language handled by this provider.
            foreach (var lng in pluralFile.Languages)
            {
                pluralSelectorCode += $"case \"{lng}\":\n";
            }
            pluralSelectorCode += $"  return new _ReswPlus_AutoGenerated.Plurals.{pluralFile.Id}Provider();\n";
        }

        // Add the fallback provider.
        AddSourceFromResource(spc, $"{assemblyName}.Templates.Plurals.OtherProvider.txt", "OtherProvider.cs");

        // Build and add the ResourceLoaderExtension with the plural selector injected.
        var resourceLoaderResourceName = $"{assemblyName}.Templates.Plurals.ResourceLoaderExtension.txt";
        var resourceLoaderTemplate = ReadAllText(Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceLoaderResourceName));
        var resourceLoaderCode = resourceLoaderTemplate.Replace("{{PluralProviderSelector}}", pluralSelectorCode);
        spc.AddSource("ResourceLoaderExtension.cs", SourceText.From(resourceLoaderCode, Encoding.UTF8));
    }

    /// <summary>
    /// Reads a resource stream and adds its content as a source file.
    /// </summary>
    private static void AddSourceFromResource(SourceProductionContext spc, string resourcePath, string itemName)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
        if (stream is null)
        {
            // Optionally, report a diagnostic or throw if the resource is missing.
            return;
        }
        var sourceText = ReadAllText(stream);
        spc.AddSource(itemName, SourceText.From(sourceText, Encoding.UTF8));
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
