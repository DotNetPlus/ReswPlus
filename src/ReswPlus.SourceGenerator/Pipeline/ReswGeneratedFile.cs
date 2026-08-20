using System;

namespace ReswPlus.SourceGenerator.Pipeline;

/// <summary>
/// The outcome of generating the code of one resource file.
/// </summary>
/// <remarks>
/// This is what the per file stage of the pipeline produces, and it is compared against what the previous run
/// produced to decide whether the file has to be emitted again. It therefore holds the generated text itself
/// rather than a handle to whatever produced it, so that two runs over an unchanged resource file compare equal.
/// </remarks>
internal sealed class ReswGeneratedFile : IEquatable<ReswGeneratedFile>
{
    private ReswGeneratedFile(string sourcePath, string hintName, string? content, string? error, bool containsMacro, bool containsPlural)
    {
        SourcePath = sourcePath;
        HintName = hintName;
        Content = content;
        Error = error;
        ContainsMacro = containsMacro;
        ContainsPlural = containsPlural;
    }

    /// <summary>
    /// Gets the path of the resource file the code was generated from.
    /// </summary>
    public string SourcePath { get; }

    /// <summary>
    /// Gets the hint name to emit the code under.
    /// </summary>
    public string HintName { get; }

    /// <summary>
    /// Gets the generated code, or <see langword="null"/> when the resource file could not be read.
    /// </summary>
    public string? Content { get; }

    /// <summary>
    /// Gets why the resource file could not be turned into code, if it could not.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Gets whether the generated code uses the macros of ReswPlus, whose support is shared by the project.
    /// </summary>
    public bool ContainsMacro { get; }

    /// <summary>
    /// Gets whether the generated code looks resources up by quantity, whose support is shared by the project.
    /// </summary>
    public bool ContainsPlural { get; }

    public static ReswGeneratedFile Generated(string sourcePath, string hintName, string content, bool containsMacro, bool containsPlural) =>
        new(sourcePath, hintName, content, null, containsMacro, containsPlural);

    public static ReswGeneratedFile Failed(string sourcePath, string hintName, string error) =>
        new(sourcePath, hintName, null, error, false, false);

    /// <inheritdoc/>
    public bool Equals(ReswGeneratedFile? other)
    {
        return other is not null
            && SourcePath == other.SourcePath
            && HintName == other.HintName
            && Content == other.Content
            && Error == other.Error
            && ContainsMacro == other.ContainsMacro
            && ContainsPlural == other.ContainsPlural;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ReswGeneratedFile);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = 17;

        hash = (hash * 31) + SourcePath.GetHashCode();
        hash = (hash * 31) + HintName.GetHashCode();
        hash = (hash * 31) + (Content?.GetHashCode() ?? 0);
        hash = (hash * 31) + (Error?.GetHashCode() ?? 0);
        hash = (hash * 31) + ContainsMacro.GetHashCode();

        return (hash * 31) + ContainsPlural.GetHashCode();
    }
}
