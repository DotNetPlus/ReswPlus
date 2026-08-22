using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Utilities;
using ReswPlus.BuildTasks;
using Xunit;

namespace ReswPlusUnitTests;

public class PseudoLocalization
{
    [Fact]
    public void AccentedTextPreservesFormatPlaceholdersAndMarkup()
    {
        var transformed = PseudoLocalizer.Transform(
            "Hello {0}, <b>{name}</b> {{literal}}",
            PseudoLocalizationMode.Accented,
            expansionPercentage: 30);

        Assert.StartsWith("\u27e6\u0126\u00eb\u0140\u0140\u00f8 ", transformed);
        Assert.EndsWith("\u27e7", transformed);
        Assert.Contains("{0}", transformed);
        Assert.Contains("<b>{name}</b>", transformed);
        Assert.Contains("{{\u0140\u00ef\u0167\u00eb\u0159\u00e5\u0140}}", transformed);
        Assert.Contains("~", transformed);
    }

    [Fact]
    public void MirroredTextCarriesRightToLeftControlCharacters()
    {
        var transformed = PseudoLocalizer.Transform(
            "Save",
            PseudoLocalizationMode.Mirrored,
            expansionPercentage: 0);

        Assert.Equal("\u202e\u27e6\u0160\u00e5\u1e7d\u00eb\u27e7\u202c", transformed);
    }

    [Fact]
    public void ModesMapToWindowsPseudoLocales()
    {
        var accented = Assert.Single(PseudoLocalizer.ParseModes("Accented"));
        var mirrored = Assert.Single(PseudoLocalizer.ParseModes("Mirrored"));

        Assert.Equal((PseudoLocalizationMode.Accented, "qps-ploc"), accented);
        Assert.Equal((PseudoLocalizationMode.Mirrored, "qps-plocm"), mirrored);
    }

    [Fact]
    public void MultipleModesAreRejected()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            PseudoLocalizer.ParseModes("Accented;Mirrored"));

        Assert.Contains("one pseudo-localization mode", exception.Message);
    }

    [Fact]
    public void UnknownModesAreRejected()
    {
        var exception = Assert.Throws<ArgumentException>(() => PseudoLocalizer.ParseModes("Expanded"));

        Assert.Contains("Accented", exception.Message);
        Assert.Contains("Mirrored", exception.Message);
    }

    [Fact]
    public void TheBuildTaskGeneratesIntermediateResourcesFromTheDefaultLanguage()
    {
        var root = Path.Combine(Path.GetTempPath(), "ReswPlusUnitTests", Guid.NewGuid().ToString("N"));
        var projectDirectory = Path.Combine(root, "Project");
        var sourcePath = Path.Combine(projectDirectory, "Strings", "en-US", "Resources.resw");
        var outputDirectory = Path.Combine(root, "obj");

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(sourcePath)!);
            File.WriteAllText(sourcePath, ReswTestHelpers.CreateResw(
                ("Welcome", "Welcome, {0}!", "#Format[String name]")));

            var source = new TaskItem(sourcePath);
            source.SetMetadata("Link", @"Strings\en-US\Resources.resw");

            var task = new GeneratePseudoResources
            {
                Resources = [source],
                DefaultLanguage = "en-US",
                ProjectDirectory = projectDirectory,
                IntermediateOutputPath = outputDirectory,
                Modes = "Accented",
                ExpansionPercentage = 30,
            };

            Assert.True(task.Execute());
            var accented = Assert.Single(task.GeneratedResources);

            Assert.Equal("qps-ploc", task.PseudoLanguage);
            Assert.Equal(@"Strings\Resources.resw", accented.GetMetadata("Link"));
            Assert.Equal(accented.GetMetadata("Link"), accented.GetMetadata("TargetPath"));
            Assert.Equal("Accented", accented.GetMetadata("ReswPlusPseudoLocalization"));

            var document = XDocument.Load(accented.ItemSpec);
            var value = document.Descendants("data").Single().Element("value")!.Value;
            var comment = document.Descendants("comment").Single().Value;

            Assert.Contains("\u0174\u00eb\u0140\u00e7\u00f8\u1e41\u00eb", value);
            Assert.Contains("{0}", value);
            Assert.Equal("#Format[String name]", comment);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void PseudoLanguagesAreAddedToTheGeneratedAppxManifest()
    {
        var root = Path.Combine(Path.GetTempPath(), "ReswPlusUnitTests", Guid.NewGuid().ToString("N"));
        var manifestPath = Path.Combine(root, "AppxManifest.xml");

        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllText(
                manifestPath,
                """
            <?xml version="1.0" encoding="utf-8"?>
            <Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10">
              <Resources>
                <Resource Language="en-US" />
              </Resources>
            </Package>
            """);

            var task = new SetPseudoLanguagesInAppxManifest
            {
                ManifestPath = manifestPath,
                Modes = "Accented",
            };

            Assert.True(task.Execute());
            Assert.True(task.Execute());

            var document = XDocument.Load(manifestPath);
            var languages = document
                .Descendants()
                .Where(element => element.Name.LocalName == "Resource")
                .Select(element => element.Attribute("Language")!.Value)
                .ToArray();

            Assert.Equal(["qps-ploc"], languages);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}