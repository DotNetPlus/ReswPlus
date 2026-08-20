using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReswPlus.SourceGenerator.Pipeline;

/// <summary>
/// The MSBuild properties the generation depends on.
/// </summary>
/// <remarks>
/// The properties are read into a value of their own rather than being taken off the options provider where
/// they are needed. The provider is a new object on every run, so a stage reading from it directly would never
/// compare equal to the previous run and would be recomputed on every keystroke, along with everything below it.
/// </remarks>
internal sealed class ReswBuildOptions : IEquatable<ReswBuildOptions>
{
    private ReswBuildOptions(
        string? projectDir,
        string? msBuildProjectFullPath,
        string? outputType,
        string? projectTypeGuids,
        string? defaultLanguage,
        string? rootNamespace,
        bool useApplicationLanguages,
        bool useUwp)
    {
        ProjectDir = projectDir;
        MSBuildProjectFullPath = msBuildProjectFullPath;
        OutputType = outputType;
        ProjectTypeGuids = projectTypeGuids;
        DefaultLanguage = defaultLanguage;
        RootNamespace = rootNamespace;
        UseApplicationLanguages = useApplicationLanguages;
        UseUwp = useUwp;
    }

    public string? ProjectDir { get; }

    public string? MSBuildProjectFullPath { get; }

    public string? OutputType { get; }

    public string? ProjectTypeGuids { get; }

    public string? DefaultLanguage { get; }

    public string? RootNamespace { get; }

    /// <summary>
    /// Gets whether the project opted into reading the plural language from the app runtime language list, the
    /// same list the resources themselves are resolved against.
    /// </summary>
    public bool UseApplicationLanguages { get; }

    /// <summary>
    /// Gets whether the project declares itself as a UWP project.
    /// </summary>
    /// <remarks>
    /// A UWP project is otherwise recognized by the <c>Windows.Foundation.UniversalApiContract</c> reference it
    /// carries, which a UWP project built for Native AOT does not have. Such a project says so with this
    /// property instead. It fills in what the references don't say rather than overriding them: a compilation
    /// whose references positively identify it is left alone.
    /// </remarks>
    public bool UseUwp { get; }

    /// <summary>
    /// Reads the properties of a project.
    /// </summary>
    /// <param name="globalOptions">The options of the compilation.</param>
    /// <returns>The properties the generation depends on.</returns>
    public static ReswBuildOptions Read(AnalyzerConfigOptions globalOptions)
    {
        return new ReswBuildOptions(
            Get("build_property.projectdir"),
            Get("build_property.MSBuildProjectFullPath"),
            Get("build_property.OutputType"),
            Get("build_property.projecttypeguids"),
            Get("build_property.DefaultLanguage"),
            Get("build_property.RootNamespace"),
            bool.TryParse(Get("build_property.ReswPlusUseApplicationLanguages"), out var parsed) && parsed,
            bool.TryParse(Get("build_property.UseUwp"), out var parsedUseUwp) && parsedUseUwp);

        string? Get(string key) => globalOptions.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Returns the directory the project is rooted at.
    /// </summary>
    /// <returns>The root path of the project, or <see langword="null"/> when it cannot be determined.</returns>
    public string? GetProjectRootPath()
    {
        if (ProjectDir is { Length: > 0 })
        {
            return ProjectDir;
        }

        return MSBuildProjectFullPath is { Length: > 0 } ? Path.GetDirectoryName(MSBuildProjectFullPath) : null;
    }

    /// <inheritdoc/>
    public bool Equals(ReswBuildOptions? other)
    {
        return other is not null
            && ProjectDir == other.ProjectDir
            && MSBuildProjectFullPath == other.MSBuildProjectFullPath
            && OutputType == other.OutputType
            && ProjectTypeGuids == other.ProjectTypeGuids
            && DefaultLanguage == other.DefaultLanguage
            && RootNamespace == other.RootNamespace
            && UseApplicationLanguages == other.UseApplicationLanguages
            && UseUwp == other.UseUwp;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ReswBuildOptions);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = 17;

        foreach (var value in new[] { ProjectDir, MSBuildProjectFullPath, OutputType, ProjectTypeGuids, DefaultLanguage, RootNamespace })
        {
            hash = (hash * 31) + (value?.GetHashCode() ?? 0);
        }

        return (hash * 31) + UseApplicationLanguages.GetHashCode() + (UseUwp.GetHashCode() * 7);
    }
}
