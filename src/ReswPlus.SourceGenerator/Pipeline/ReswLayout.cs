using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ReswPlus.SourceGenerator.Analysis;
using ReswPlus.SourceGenerator.CodeGenerators;

using ReswPlus.SourceGenerator.ClassGenerators;

namespace ReswPlus.SourceGenerator.Pipeline;

/// <summary>
/// A name paired with the path of the resource file it belongs to.
/// </summary>
internal readonly struct NamedPath : IEquatable<NamedPath>
{
    public NamedPath(string name, string path)
    {
        Name = name;
        Path = path;
    }

    public string Name { get; }

    public string Path { get; }

    /// <inheritdoc/>
    public bool Equals(NamedPath other) => Name == other.Name && Path == other.Path;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is NamedPath other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => (Name.GetHashCode() * 31) + Path.GetHashCode();
}

/// <summary>
/// How the resource files of a project are laid out: which one of each resource carries the default language,
/// and which languages the project is translated in.
/// </summary>
/// <remarks>
/// This is derived from the paths of the resource files alone, never from their content, so that editing a
/// string does not make the whole project regroup. It only changes when a resource file is added, removed or
/// renamed.
/// </remarks>
internal sealed class ReswLayout : IEquatable<ReswLayout>
{
    private Dictionary<string, string>? _hintNamesByPath;

    private ReswLayout(EquatableArray<NamedPath> generatedFiles, EquatableArray<NamedPath> languages)
    {
        GeneratedFiles = generatedFiles;
        Languages = languages;
    }

    /// <summary>
    /// Gets the resource files the code is generated from, each paired with the hint name to emit it under.
    /// </summary>
    public EquatableArray<NamedPath> GeneratedFiles { get; }

    /// <summary>
    /// Gets a resource file of each language of the project, so that a diagnostic about a language has
    /// somewhere to point.
    /// </summary>
    public EquatableArray<NamedPath> Languages { get; }

    /// <summary>
    /// Works out the layout of a project.
    /// </summary>
    /// <param name="paths">The paths of the resource files of the project.</param>
    /// <param name="defaultLanguage">The default language of the project, if it declares one.</param>
    /// <param name="resolveNamespace">Returns the namespace the class of a resource file lands in, which qualifies its hint name so that two resources of the same name in different folders don't collide.</param>
    /// <returns>The layout of the project.</returns>
    public static ReswLayout Create(IEnumerable<string> paths, string? defaultLanguage, Func<string, string> resolveNamespace)
    {
        var allPaths = paths.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        var defaultPaths = ReswFileGrouping.GroupByResource(allPaths)
            .Select(group => ReswFileGrouping.RetrieveDefaultResourceFile(group, defaultLanguage))
            .Where(path => path is not null)
            .Select(path => path!)
            .ToArray();

        // Two resource files can carry the same name in different folders, so the hint name is qualified with
        // the namespace the class lands in. Emitting the same hint name twice throws, and used to make the
        // generator produce nothing at all for the whole project, so what is left is disambiguated rather than
        // trusted.
        var taken = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var generatedFiles = new List<NamedPath>();

        foreach (var path in defaultPaths)
        {
            var hintName = $"{resolveNamespace(path)}.{Path.GetFileName(path)}{GeneratedCode.FileExtension}";

            if (!taken.Add(hintName))
            {
                var suffix = 2;

                while (!taken.Add($"{resolveNamespace(path)}.{Path.GetFileName(path)}.{suffix}{GeneratedCode.FileExtension}"))
                {
                    ++suffix;
                }

                hintName = $"{resolveNamespace(path)}.{Path.GetFileName(path)}.{suffix}{GeneratedCode.FileExtension}";
            }

            generatedFiles.Add(new NamedPath(hintName, path));
        }

        // The folder of a resource is kept whole and normalised exactly the way the generated code normalises
        // the language of the app, so that the two always agree: a region can decline differently from the
        // language it belongs to, and a culture-sensitive ToLower would turn 'IS-IS' into 'ıs' under Turkish
        // and never match again.
        var languages = new List<NamedPath>();
        var seenLanguages = new HashSet<string>(StringComparer.Ordinal);

        foreach (var path in allPaths)
        {
            var language = PluralFormsRetriever.NormalizeTag(Path.GetFileName(Path.GetDirectoryName(path)));

            if (seenLanguages.Add(language))
            {
                languages.Add(new NamedPath(language, path));
            }
        }

        return new ReswLayout(new EquatableArray<NamedPath>(generatedFiles), new EquatableArray<NamedPath>(languages));
    }

    /// <summary>
    /// Returns the hint name to emit the code of a resource file under.
    /// </summary>
    /// <param name="path">The path of the resource file.</param>
    /// <returns>The hint name, or <see langword="null"/> when the file is not the one the code is generated from.</returns>
    public string? GetHintName(string path)
    {
        _hintNamesByPath ??= GeneratedFiles.ToDictionary(file => file.Path, file => file.Name, StringComparer.OrdinalIgnoreCase);

        return _hintNamesByPath.TryGetValue(path, out var hintName) ? hintName : null;
    }

    /// <inheritdoc/>
    public bool Equals(ReswLayout? other)
    {
        return other is not null && GeneratedFiles.Equals(other.GeneratedFiles) && Languages.Equals(other.Languages);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ReswLayout);

    /// <inheritdoc/>
    public override int GetHashCode() => (GeneratedFiles.GetHashCode() * 31) + Languages.GetHashCode();
}
