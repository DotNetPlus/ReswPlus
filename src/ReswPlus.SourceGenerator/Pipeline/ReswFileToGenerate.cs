using System;
using Microsoft.CodeAnalysis;

namespace ReswPlus.SourceGenerator.Pipeline;

/// <summary>
/// A resource file the code is generated from, with everything generating it depends on.
/// </summary>
/// <remarks>
/// Working out which file of a resource carries the default language, and what to call the file generated from
/// it, needs the layout of the whole project. Parsing that file and writing the code does not. Reading the one
/// thing generation needs out of the layout here, and comparing it by value, is what keeps a resource nobody
/// touched from being parsed again because a different resource was added, removed or renamed beside it.
/// </remarks>
internal readonly struct ReswFileToGenerate : IEquatable<ReswFileToGenerate>
{
    public ReswFileToGenerate(AdditionalText file, ReswProject project, string? hintName)
    {
        File = file;
        Project = project;
        HintName = hintName;
    }

    /// <summary>
    /// Gets the resource file.
    /// </summary>
    public AdditionalText File { get; }

    /// <summary>
    /// Gets the project the code is generated for.
    /// </summary>
    public ReswProject Project { get; }

    /// <summary>
    /// Gets the name to emit the generated file under, or <see langword="null"/> when this file holds a
    /// translation rather than the language the code is generated from.
    /// </summary>
    public string? HintName { get; }

    /// <inheritdoc/>
    /// <remarks>
    /// The file is compared by identity because the compiler hands back the same object for a file it has not
    /// seen change, which is exactly the question being asked.
    /// </remarks>
    public bool Equals(ReswFileToGenerate other)
    {
        return ReferenceEquals(File, other.File)
            && Equals(Project, other.Project)
            && string.Equals(HintName, other.HintName, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ReswFileToGenerate other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = File.GetHashCode();

        hash = (hash * 31) + (Project?.GetHashCode() ?? 0);

        return (hash * 31) + (HintName?.GetHashCode() ?? 0);
    }
}
