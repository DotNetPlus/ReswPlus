using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace ReswPlus.BuildTasks;

public sealed class GeneratePseudoResources : Task
{
    [Required]
    public ITaskItem[] Resources { get; set; } = Array.Empty<ITaskItem>();

    [Required]
    public string DefaultLanguage { get; set; } = "";

    [Required]
    public string ProjectDirectory { get; set; } = "";

    [Required]
    public string IntermediateOutputPath { get; set; } = "";

    [Required]
    public string Modes { get; set; } = "";

    public int ExpansionPercentage { get; set; } = 30;

    [Output]
    public ITaskItem[] GeneratedResources { get; private set; } = Array.Empty<ITaskItem>();

    public override bool Execute()
    {
        if (ExpansionPercentage is < 0 or > 200)
        {
            Log.LogError(
                "ReswPlus pseudo-localization expansion must be between 0 and 200, but was {0}.",
                ExpansionPercentage);
            return false;
        }

        IReadOnlyList<(PseudoLocalizationMode Mode, string Language)> modes;

        try
        {
            modes = PseudoLocalizer.ParseModes(Modes);
        }
        catch (ArgumentException exception)
        {
            Log.LogError(exception.Message);
            return false;
        }

        var sourceResources = Resources
            .Where(resource => Path.GetExtension(resource.ItemSpec).Equals(".resw", StringComparison.OrdinalIgnoreCase))
            .Select(resource => (Item: resource, LogicalPath: GetLogicalPath(resource)))
            .Where(resource => ContainsLanguageFolder(resource.LogicalPath, DefaultLanguage))
            .ToArray();

        if (sourceResources.Length == 0)
        {
            Log.LogError(
                "ReswPlus pseudo-localization could not find a .resw file in the default-language folder '{0}'.",
                DefaultLanguage);
            return false;
        }

        var generated = new List<ITaskItem>();
        var outputPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var source in sourceResources)
        {
            foreach (var mode in modes)
            {
                var logicalPath = ReplaceLanguageFolder(source.LogicalPath, DefaultLanguage, mode.Language);
                var outputPath = Path.GetFullPath(Path.Combine(
                    IntermediateOutputPath,
                    "ReswPlus",
                    "PseudoLocalization",
                    ToSafeRelativePath(logicalPath)));

                if (!outputPaths.Add(outputPath))
                {
                    Log.LogError(
                        "Two resources would generate the same pseudo-localized file '{0}'. Set Link metadata to give linked resources distinct paths.",
                        logicalPath);
                    return false;
                }

                if (!TryGenerate(source.Item.ItemSpec, outputPath, mode.Mode))
                {
                    return false;
                }

                var output = new TaskItem(outputPath);
                output.SetMetadata("Link", logicalPath);
                output.SetMetadata("TargetPath", logicalPath);
                output.SetMetadata("ReswPlusPseudoLocalization", mode.Mode.ToString());
                generated.Add(output);
            }
        }

        GeneratedResources = generated.ToArray();
        return !Log.HasLoggedErrors;
    }

    private bool TryGenerate(string sourcePath, string outputPath, PseudoLocalizationMode mode)
    {
        try
        {
            var document = XDocument.Load(sourcePath, LoadOptions.PreserveWhitespace);

            foreach (var value in document
                .Descendants()
                .Where(element => element.Name.LocalName == "value" &&
                                  element.Parent?.Name.LocalName == "data"))
            {
                value.Value = PseudoLocalizer.Transform(value.Value, mode, ExpansionPercentage);
            }

            var bytes = Serialize(document);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            if (!File.Exists(outputPath) || !File.ReadAllBytes(outputPath).SequenceEqual(bytes))
            {
                File.WriteAllBytes(outputPath, bytes);
            }

            return true;
        }
        catch (XmlException exception)
        {
            Log.LogError("Could not pseudo-localize '{0}': {1}", sourcePath, exception.Message);
            return false;
        }
        catch (IOException exception)
        {
            Log.LogError("Could not pseudo-localize '{0}': {1}", sourcePath, exception.Message);
            return false;
        }
        catch (UnauthorizedAccessException exception)
        {
            Log.LogError("Could not pseudo-localize '{0}': {1}", sourcePath, exception.Message);
            return false;
        }
    }

    private string GetLogicalPath(ITaskItem resource)
    {
        var link = resource.GetMetadata("Link");
        if (!string.IsNullOrWhiteSpace(link))
        {
            return link;
        }

        var sourcePath = Path.GetFullPath(resource.ItemSpec);
        var projectPath = Path.GetFullPath(ProjectDirectory)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        if (sourcePath.StartsWith(projectPath, StringComparison.OrdinalIgnoreCase))
        {
            return sourcePath.Substring(projectPath.Length);
        }

        var languageDirectory = Path.GetDirectoryName(sourcePath);
        var resourceDirectory = Path.GetDirectoryName(languageDirectory);
        return Path.Combine(
            Path.GetFileName(resourceDirectory) ?? "Strings",
            Path.GetFileName(languageDirectory) ?? DefaultLanguage,
            Path.GetFileName(sourcePath));
    }

    private static bool ContainsLanguageFolder(string path, string language)
    {
        return SplitPath(path).Any(segment => segment.Equals(language, StringComparison.OrdinalIgnoreCase));
    }

    private static string ReplaceLanguageFolder(string path, string sourceLanguage, string targetLanguage)
    {
        var segments = SplitPath(path);

        for (var index = segments.Length - 1; index >= 0; index--)
        {
            if (segments[index].Equals(sourceLanguage, StringComparison.OrdinalIgnoreCase))
            {
                segments[index] = targetLanguage;
                return string.Join(Path.DirectorySeparatorChar.ToString(), segments);
            }
        }

        throw new InvalidOperationException($"The path '{path}' does not contain the language '{sourceLanguage}'.");
    }

    private static string[] SplitPath(string path)
    {
        return path.Split(
            new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
            StringSplitOptions.RemoveEmptyEntries);
    }

    private static string ToSafeRelativePath(string path)
    {
        return string.Join(
            Path.DirectorySeparatorChar.ToString(),
            SplitPath(path).Where(segment => segment != "." && segment != ".." && !segment.Contains(':')));
    }

    private static byte[] Serialize(XDocument document)
    {
        using var stream = new MemoryStream();
        using (var writer = XmlWriter.Create(stream, new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            Indent = false,
            OmitXmlDeclaration = document.Declaration is null,
        }))
        {
            document.Save(writer);
        }

        return stream.ToArray();
    }
}
