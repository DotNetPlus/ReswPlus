using System;
using System.Collections.Generic;
using System.IO;
using CldrRuleImporter;

namespace ReswPlusUnitTests;

/// <summary>
/// The rules Unicode CLDR publishes, read from the file the importer reads.
/// </summary>
/// <remarks>
/// The generator no longer reads CLDR's file: the importer turns it into C# once, and the generator holds the
/// result. The tests still read it, because checking the generated classes against the rules they were written
/// from is the whole point -- and reading it here, from the same copy the importer reads, is what makes that a
/// check rather than a restatement.
/// </remarks>
internal static class Cldr
{
    private static readonly CldrPublishedRules.Data Published = Read();

    /// <summary>
    /// The CLDR release the rules were published in.
    /// </summary>
    public static string Version => Published.Version;

    /// <summary>
    /// The rules of every language CLDR publishes rules for.
    /// </summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<CldrPublishedRules.Rule>> Cardinal => Published.Cardinal;

    /// <summary>
    /// The codes CLDR renamed, mapped to the name it publishes them under.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string> Renamed = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["bh"] = "bho",
        ["in"] = "id",
        ["iw"] = "he",
        ["ji"] = "yi",
        ["jw"] = "jv",
        ["mo"] = "ro",
        ["sh"] = "sr",
        ["tl"] = "fil",
    };

    /// <summary>
    /// Gets the rules CLDR publishes for a language, under its current name.
    /// </summary>
    /// <param name="language">The language, as ReswPlus maps it.</param>
    /// <returns>The rules, or <see langword="null"/> when CLDR publishes none.</returns>
    public static IReadOnlyList<CldrPublishedRules.Rule>? RulesOf(string language)
    {
        var code = Renamed.TryGetValue(language, out var current) ? current : language;

        return Cardinal.TryGetValue(code, out var rules) ? rules : null;
    }

    private static CldrPublishedRules.Data Read()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }

        var path = Path.Combine(
            directory?.FullName ?? throw new InvalidOperationException("The repository root could not be found."),
            "tools",
            "CldrRuleImporter",
            "plurals.json");

        return CldrPublishedRules.Read(File.ReadAllText(path));
    }
}
