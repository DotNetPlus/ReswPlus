using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace ReswPlus.BuildTasks;

public sealed class AddPseudoLanguagesToAppxManifest : Task
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

            var existing = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var resource in resources.Elements().Where(element => element.Name.LocalName == "Resource"))
            {
                var language = resource.Attribute("Language")?.Value;
                if (language is not null)
                {
                    existing.Add(language);
                }
            }

            var changed = false;
            foreach (var mode in PseudoLocalizer.ParseModes(Modes))
            {
                if (existing.Add(mode.Language))
                {
                    resources.Add(new XElement(resources.Name.Namespace + "Resource", new XAttribute("Language", mode.Language)));
                    changed = true;
                }
            }

            if (changed)
            {
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
            Log.LogError("Could not add pseudo-languages to '{0}': {1}", ManifestPath, exception.Message);
            return false;
        }
        catch (IOException exception)
        {
            Log.LogError("Could not add pseudo-languages to '{0}': {1}", ManifestPath, exception.Message);
            return false;
        }
        catch (UnauthorizedAccessException exception)
        {
            Log.LogError("Could not add pseudo-languages to '{0}': {1}", ManifestPath, exception.Message);
            return false;
        }
    }
}