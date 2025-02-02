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

[Generator(LanguageNames.CSharp)]
public partial class ReswSourceGenerator : ISourceGenerator
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

    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization needed for now.
    }

    public void Execute(GeneratorExecutionContext context)
    {
#if DEBUG
        if (!Debugger.IsAttached)
        {
            // Uncomment the following line to debug the source generator.
            // Debugger.Launch();
        }
#endif

        // Only support C#
        if (context.Compilation is not CSharpCompilation)
        {
            context.ReportDiagnostic(Diagnostic.Create(UnsupportedLanguageDiagnostic, null));
            return;
        }

        // Retrieve project root path.
        if (!TryGetGlobalOption(context, "build_property.projectdir", out var projectRootPath)
            && TryGetGlobalOption(context, "build_property.MSBuildProjectFullPath", out var projectFileFullPath))
        {
            projectRootPath = Path.GetDirectoryName(projectFileFullPath);
        }

        if (string.IsNullOrEmpty(projectRootPath))
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingRootPathDiagnostic, null));
            return;
        }

        // Determine if the project is a library.
        bool isLibrary = false;
        if (TryGetGlobalOption(context, "build_property.OutputType", out var outputType))
        {
            isLibrary = outputType.Equals("library", StringComparison.OrdinalIgnoreCase)
                     || outputType.Equals("module", StringComparison.OrdinalIgnoreCase);
        }
        else if (TryGetGlobalOption(context, "build_property.projecttypeguids", out var projectTypeGuidsValue))
        {
            isLibrary = projectTypeGuidsValue.Equals("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}", StringComparison.OrdinalIgnoreCase)
                     || projectTypeGuidsValue.Equals("{BC8A1FFA-BEE3-4634-8014-F334798102B3}", StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            // If unable to determine, assume it is an application.
            context.ReportDiagnostic(Diagnostic.Create(UnknownProjectTypeDiagnostic, Location.None));
        }

        // Determine AppType based on referenced assemblies.
        var appType = RetrieveAppType(context);
        var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        switch (appType)
        {
            case AppType.WindowsAppSDK:
                AddSourceFromResource(context, $"{assemblyName}.Templates.ResourceStringProviders.MicrosoftResourceStringProvider.txt", "ResourceStringProvider.cs");
                break;
            case AppType.UWP:
                AddSourceFromResource(context, $"{assemblyName}.Templates.ResourceStringProviders.WindowsResourceStringProvider.txt", "ResourceStringProvider.cs");
                break;
            default:
                context.ReportDiagnostic(Diagnostic.Create(UnrecognizedAppTypeDiagnostic, null));
                return;
        }

        // Retrieve the default language (optional)
        TryGetGlobalOption(context, "build_property.DefaultLanguage", out var projectDefaultLanguage);

        // Retrieve the project's root namespace.
        if (!TryGetGlobalOption(context, "build_property.RootNamespace", out var projectRootNamespace))
        {
            context.ReportDiagnostic(Diagnostic.Create(UnknownNamespaceDiagnostic, null));
            return;
        }

        // Retrieve all .resw files from AdditionalFiles.
        var allResourceFiles = context.AdditionalFiles
            .Where(f => Path.GetExtension(f.Path).Equals(".resw", StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .ToArray();

        // Group files and retrieve the default resource file per group.
        var defaultLanguageResourceFiles = (from file in allResourceFiles
                                            group file by
                                              Path.Combine(
                                                  Path.GetDirectoryName(Path.GetDirectoryName(file.Path)),
                                                  Path.GetFileName(file.Path))
                                            into fileGroup
                                            select RetrieveDefaultResourceFile(
                                                fileGroup.Select(f => f.Path),
                                                projectDefaultLanguage))
                                           .ToArray();

        // Gather all distinct languages.
        var allLanguages = allResourceFiles
            .Select(f => Path.GetFileName(Path.GetDirectoryName(f.Path)).Split('-')[0].ToLower())
            .Distinct()
            .ToArray();

        // Process each default resource file.
        foreach (var file in defaultLanguageResourceFiles)
        {
            // Determine namespace for the generated class.
            var namespaceForReswFile = projectRootNamespace;
            var reswParentDirectory = Path.GetDirectoryName(file);
            if (reswParentDirectory != null && reswParentDirectory.StartsWith(projectRootPath, StringComparison.OrdinalIgnoreCase))
            {
                var additionalNamespace = reswParentDirectory.Substring(projectRootPath.Length)
                    .Trim(Path.DirectorySeparatorChar)
                    .Replace(Path.DirectorySeparatorChar, '.');
                if (!string.IsNullOrEmpty(additionalNamespace))
                {
                    namespaceForReswFile += "." + additionalNamespace;
                }
            }

            // Get the additional file text.
            var additionalText = context.AdditionalFiles.FirstOrDefault(f => f.Path == file);
            if (additionalText is null)
            {
                continue;
            }

            // Generate code for the resource file.
            var resourceFileInfo = new ResourceFileInfo(file, new Project(context.Compilation.AssemblyName, isLibrary));
            var codeGenerator = ReswClassGenerator.CreateGenerator(resourceFileInfo, null);
            var baseFilename = Path.GetFileName(file).Split('.')[0];
            var generatedData = codeGenerator.GenerateCode(
                baseFilename: baseFilename,
                content: additionalText.GetText(context.CancellationToken).ToString(),
                defaultNamespace: namespaceForReswFile,
                isAdvanced: true,
                appType: appType);

            // Add each generated file as a new source.
            foreach (var generatedFile in generatedData.Files)
            {
                context.AddSource($"{Path.GetFileName(file)}.cs", SourceText.From(generatedFile.Content, Encoding.UTF8));
            }

            // If macros were used, include the Macros source file.
            if (generatedData.ContainsMacro)
            {
                AddSourceFromResource(context, "ReswPlus.SourceGenerator.Templates.Macros.Macros.txt", "Macros.cs");
            }

            // If plural forms are detected, add plural-related support sources.
            if (generatedData.ContainsPlural)
            {
                AddSourceFromResource(context, $"{assemblyName}.Templates.Plurals.IPluralProvider.txt", "IPluralProvider.cs");
                AddSourceFromResource(context, $"{assemblyName}.Templates.Plurals.PluralTypeEnum.txt", "PluralTypeEnum.cs");
                AddSourceFromResource(context, $"{assemblyName}.Templates.Utils.IntExt.txt", "IntExt.cs");
                AddSourceFromResource(context, $"{assemblyName}.Templates.Utils.DoubleExt.txt", "DoubleExt.cs");
                AddLanguageSupport(context, allLanguages);
            }
        }
    }

    /// <summary>
    /// Retrieve the default resource file from the given list that matches one of the preferred languages.
    /// </summary>
    /// <param name="reswFiles">Collection of resource file paths.</param>
    /// <param name="defaultLanguage">The default language of the project.</param>
    /// <returns>The path to the selected default resource file, or null if none.</returns>
    private string RetrieveDefaultResourceFile(IEnumerable<string> reswFiles, string defaultLanguage)
    {
        // Build a list of candidate languages.
        var candidateLanguages = new List<string>();
        if (!string.IsNullOrEmpty(defaultLanguage))
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
    private AppType RetrieveAppType(GeneratorExecutionContext context)
    {
        return context.Compilation.ExternalReferences.Any(r =>
            r.Display?.IndexOf("Microsoft.WindowsAppSdk", StringComparison.OrdinalIgnoreCase) >= 0)
            ? AppType.WindowsAppSDK
            : context.Compilation.ExternalReferences.Any(r =>
            r.Display?.IndexOf("Windows.Foundation.UniversalApiContract", StringComparison.OrdinalIgnoreCase) >= 0)
            ? AppType.UWP
            : AppType.Unknown;
    }

    /// <summary>
    /// Adds language support sources for pluralization based on the provided languages.
    /// </summary>
    private static void AddLanguageSupport(GeneratorExecutionContext context, string[] languagesSupported)
    {
        var pluralSelectorCode = "default:\n  return new _ReswPlus_AutoGenerated.Plurals.OtherProvider();\n";
        var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        foreach (var pluralFile in PluralFormsRetriever.RetrievePluralFormsForLanguages(languagesSupported))
        {
            var resourceName = $"{assemblyName}.Templates.Plurals.{pluralFile.Id}Provider.txt";
            AddSourceFromResource(context, resourceName, $"{pluralFile.Id}Provider.cs");

            // Add each language handled by this provider.
            foreach (var lng in pluralFile.Languages)
            {
                pluralSelectorCode += $"case \"{lng}\":\n";
            }
            pluralSelectorCode += $"  return new _ReswPlus_AutoGenerated.Plurals.{pluralFile.Id}Provider();\n";
        }

        // Add the fallback provider.
        AddSourceFromResource(context, $"{assemblyName}.Templates.Plurals.OtherProvider.txt", "OtherProvider.cs");

        // Build and add the ResourceLoaderExtension with the plural selector injected.
        var resourceLoaderResourceName = $"{assemblyName}.Templates.Plurals.ResourceLoaderExtension.txt";
        var resourceLoaderTemplate = ReadAllText(Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceLoaderResourceName));
        var resourceLoaderCode = resourceLoaderTemplate.Replace("{{PluralProviderSelector}}", pluralSelectorCode);
        context.AddSource("ResourceLoaderExtension.cs", SourceText.From(resourceLoaderCode, Encoding.UTF8));
    }

    /// <summary>
    /// Reads a resource stream and adds its content as a source file.
    /// </summary>
    private static void AddSourceFromResource(GeneratorExecutionContext context, string resourcePath, string itemName)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
        if (stream is null)
        {
            // You might want to report a diagnostic or throw if the resource is missing.
            return;
        }
        var sourceText = ReadAllText(stream);
        context.AddSource(itemName, SourceText.From(sourceText, Encoding.UTF8));
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

    /// <summary>
    /// Attempts to retrieve a global analyzer config option.
    /// </summary>
    private static bool TryGetGlobalOption(GeneratorExecutionContext context, string key, out string value)
    {
        return context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(key, out value);
    }
}
