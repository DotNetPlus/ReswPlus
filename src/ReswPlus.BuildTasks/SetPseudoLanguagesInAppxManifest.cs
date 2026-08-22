using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace ReswPlus.BuildTasks;

public sealed class SetPseudoLanguagesInAppxManifest : Task
{
    [Required]
    public string ManifestPath { get; set; } = "";

    [Required]
    public string Modes { get; set; } = "";

    public override bool Execute()
    {
        try
        {
            var document = XDocument.Load(ManifestPath, LoadOptions.PreserveWhitespace);
            var package = document.Root;
            var resources = package?.Elements().FirstOrDefault(element => element.Name.LocalName == "Resources");

            if (resources is null)
            {
                Log.LogError("The generated AppX manifest '{0}' has no Resources element.", ManifestPath);
                return false;
            }

            var languages = PseudoLocalizer.ParseModes(Modes)
                .Select(mode => mode.Language)
                .ToArray();
            var existing = resources
                .Elements()
                .Where(element => element.Name.LocalName == "Resource")
                .Select(element => element.Attribute("Language")?.Value ?? "")
                .ToArray();
            var changed = !existing.SequenceEqual(languages, StringComparer.OrdinalIgnoreCase);

            if (changed)
            {
                resources.Elements()
                    .Where(element => element.Name.LocalName == "Resource")
                    .Remove();

                foreach (var language in languages)
                {
                    resources.Add(new XElement(
                        resources.Name.Namespace + "Resource",
                        new XAttribute("Language", language)));
                }

                using var writer = XmlWriter.Create(ManifestPath, new XmlWriterSettings
                {
                    Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                    Indent = true,
                    OmitXmlDeclaration = document.Declaration is null,
                });
                document.Save(writer);
            }

            return true;
        }
        catch (ArgumentException exception)
        {
            Log.LogError(exception.Message);
            return false;
        }
        catch (XmlException exception)
        {
            Log.LogError("Could not set pseudo-languages in '{0}': {1}", ManifestPath, exception.Message);
            return false;
        }
        catch (IOException exception)
        {
            Log.LogError("Could not set pseudo-languages in '{0}': {1}", ManifestPath, exception.Message);
            return false;
        }
        catch (UnauthorizedAccessException exception)
        {
            Log.LogError("Could not set pseudo-languages in '{0}': {1}", ManifestPath, exception.Message);
            return false;
        }
    }
}