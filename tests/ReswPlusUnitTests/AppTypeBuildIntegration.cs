using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace ReswPlusUnitTests;

public class AppTypeBuildIntegration
{
    [Theory]
    [InlineData(null, "Auto")]
    [InlineData("", "Auto")]
    [InlineData("WindowsAppSDK", "WindowsAppSDK")]
    [InlineData("UWP", "UWP")]
    [InlineData("UnoWinUI", "UnoWinUI")]
    [InlineData(" uWp ", " uWp ")]
    [InlineData("Invalid", "Invalid")]
    public async Task ThePackagedTargetsExposeTheDefaultOrUserSelection(string? selection, string expected)
    {
        var properties = new XElement("PropertyGroup");
        if (selection is not null)
        {
            properties.Add(new XElement("ReswPlusAppType", selection));
        }

        await AssertSelectionAsync(properties, expected);
    }

    [Theory]
    [InlineData("", "Auto")]
    [InlineData("net10.0-android", "UnoWinUI")]
    [InlineData("net10.0-ios", "UnoWinUI")]
    [InlineData("net10.0", "UnoWinUI")]
    [InlineData("net10.0-windows10.0.19041.0", "Auto")]
    public async Task TheDocumentedConditionLeavesWindowsHeadsOnAutomaticDetection(string targetFramework, string expected)
    {
        var readme = File.ReadAllText(Path.Combine(RepositoryPath(), "README.md"));
        var example = Regex.Match(
            readme,
            @"### Override application model detection.*?```xml\s*(?<xml>.*?)```",
            RegexOptions.Singleline);
        Assert.True(example.Success, "The README must include the per-target-framework override example.");

        var properties = XElement.Parse(example.Groups["xml"].Value);
        await AssertSelectionAsync(
            new XElement("PropertyGroup", new XElement("TargetFramework", targetFramework)),
            expected,
            properties);
    }

    private static async Task AssertSelectionAsync(XElement properties, string expected, XElement? additionalProperties = null)
    {
        var root = Path.Combine(Path.GetTempPath(), "ReswPlusUnitTests", $"AppType-{Guid.NewGuid():N}");
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "BuildLogs");
        var binlog = Path.Combine(logDirectory, $"app-type-{Guid.NewGuid():N}.binlog");

        try
        {
            Directory.CreateDirectory(root);
            Directory.CreateDirectory(logDirectory);
            var project = new XElement(
                "Project",
                properties,
                additionalProperties,
                new XElement("Import", new XAttribute(
                    "Project",
                    Path.Combine(RepositoryPath(), "nuget", "ReswPlus.targets"))));
            var projectPath = Path.Combine(root, "PropertyProbe.proj");
            project.Save(projectPath);

            var startInfo = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = RepositoryPath(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            foreach (var argument in new[]
            {
                "msbuild",
                projectPath,
                "-nologo",
                "-verbosity:quiet",
                "-getProperty:ReswPlusAppType",
                "-getItem:CompilerVisibleProperty",
                $"-bl:{binlog}",
            })
            {
                startInfo.ArgumentList.Add(argument);
            }

            using var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Could not start dotnet msbuild.");
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(1));
            try
            {
                await process.WaitForExitAsync(timeout.Token);
            }
            catch (OperationCanceledException)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
                throw;
            }
            var output = await outputTask;
            var errors = await errorTask;
            Assert.True(process.ExitCode == 0, $"MSBuild property evaluation failed:{Environment.NewLine}{output}{errors}{Environment.NewLine}Binlog: {binlog}");

            using var result = JsonDocument.Parse(output);
            Assert.Equal(expected, result.RootElement.GetProperty("Properties").GetProperty("ReswPlusAppType").GetString());
            var exportedProperties = result.RootElement.GetProperty("Items").GetProperty("CompilerVisibleProperty")
                .EnumerateArray()
                .Select(item => item.GetProperty("Identity").GetString());
            Assert.Contains("ReswPlusAppType", exportedProperties);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    private static string RepositoryPath([CallerFilePath] string sourceFile = "")
    {
        return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFile)!, "..", ".."));
    }
}
