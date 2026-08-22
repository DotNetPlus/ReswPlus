using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ReswPlus.SourceGenerator.Pipeline;

/// <summary>
/// What the generation needs to know about the project, resolved once.
/// </summary>
internal sealed class ReswProject : IEquatable<ReswProject>
{
    private ReswProject(
        bool isSupported,
        AppType appType,
        string assemblyName,
        string projectRootPath,
        string rootNamespace,
        bool isLibrary,
        bool useApplicationLanguages,
        bool generateResourceInterfaces,
        string? defaultLanguage,
        EquatableArray<string> setupProblems)
    {
        DefaultLanguage = defaultLanguage;
        IsSupported = isSupported;
        AppType = appType;
        AssemblyName = assemblyName;
        ProjectRootPath = projectRootPath;
        RootNamespace = rootNamespace;
        IsLibrary = isLibrary;
        UseApplicationLanguages = useApplicationLanguages;
        GenerateResourceInterfaces = generateResourceInterfaces;
        SetupProblems = setupProblems;
    }

    /// <summary>
    /// Gets whether the project is one ReswPlus can generate code for.
    /// </summary>
    public bool IsSupported { get; }

    public AppType AppType { get; }

    public string AssemblyName { get; }

    public string ProjectRootPath { get; }

    public string RootNamespace { get; }

    public bool IsLibrary { get; }

    public bool UseApplicationLanguages { get; }

    public bool GenerateResourceInterfaces { get; }

    /// <summary>
    /// Gets the default language of the project, which picks the resource file the code is generated from.
    /// </summary>
    public string? DefaultLanguage { get; }

    /// <summary>
    /// Gets the identifiers of the diagnostics to report about the setup of the project.
    /// </summary>
    /// <remarks>
    /// The descriptors themselves are resolved when the diagnostics are reported, rather than being carried
    /// here: a <see cref="DiagnosticDescriptor"/> compares by reference, which would keep this value from ever
    /// comparing equal to the one of the previous run.
    /// </remarks>
    public EquatableArray<string> SetupProblems { get; }

    /// <summary>
    /// Resolves what the generation needs from the compilation and the properties of the project.
    /// </summary>
    /// <param name="compilationInfo">What the generation depends on in the compilation.</param>
    /// <param name="options">The properties of the project.</param>
    /// <returns>The resolved project.</returns>
    public static ReswProject Create(CompilationInfo compilationInfo, ReswBuildOptions options)
    {
        var problems = new List<string>();

        if (!compilationInfo.IsCSharp)
        {
            return Unsupported(Diagnostics.UnsupportedLanguage.Id);
        }

        var projectRootPath = options.GetProjectRootPath();

        if (string.IsNullOrEmpty(projectRootPath))
        {
            return Unsupported(Diagnostics.MissingRootPath.Id);
        }

        var isLibrary = false;

        if (options.OutputType is { Length: > 0 })
        {
            isLibrary = options.OutputType.Equals("library", StringComparison.OrdinalIgnoreCase)
                     || options.OutputType.Equals("module", StringComparison.OrdinalIgnoreCase);
        }
        else if (options.ProjectTypeGuids is { Length: > 0 })
        {
            isLibrary = options.ProjectTypeGuids.Equals("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}", StringComparison.OrdinalIgnoreCase)
                     || options.ProjectTypeGuids.Equals("{BC8A1FFA-BEE3-4634-8014-F334798102B3}", StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            problems.Add(Diagnostics.UnknownProjectType.Id);
        }

        // A UWP project built for Native AOT carries no 'Windows.Foundation.UniversalApiContract' reference for
        // the compilation to be recognized by, and says what it is with the 'UseUwp' property instead. The
        // property fills in what the references don't say rather than overriding them, so that a project whose
        // references positively identify it can never be taken for something else by a stray property.
        var appType = compilationInfo.AppType is AppType.Unknown && options.UseUwp
            ? AppType.UWP
            : compilationInfo.AppType;

        if (appType is not (AppType.WindowsAppSDK or AppType.UWP))
        {
            return Unsupported([.. problems, Diagnostics.UnrecognizedAppType.Id]);
        }

        if (string.IsNullOrEmpty(options.RootNamespace))
        {
            return Unsupported([.. problems, Diagnostics.UnknownNamespace.Id]);
        }

        return new ReswProject(
            isSupported: true,
            appType,
            compilationInfo.AssemblyName ?? "",
            projectRootPath!,
            options.RootNamespace!,
            isLibrary,
            options.UseApplicationLanguages,
            options.GenerateResourceInterfaces,
            options.DefaultLanguage,
            new EquatableArray<string>(problems));

        ReswProject Unsupported(params string[] problems) =>
            new(false, AppType.Unknown, "", "", "", false, false, false, options.DefaultLanguage, new EquatableArray<string>(problems));
    }

    /// <summary>
    /// Returns the namespace the class generated for a resource file is declared in.
    /// </summary>
    /// <param name="resourceFilePath">The path of the resource file.</param>
    /// <returns>The namespace, which follows the folder the resource file sits in.</returns>
    /// <remarks>
    /// A resource file that sits outside the project, which is how a project shares its resources with another
    /// one, takes the root namespace of the project as is. The paths are resolved before being compared,
    /// because a linked file is handed over the way it is written -- '..\Shared\Strings\en-US\Resources.resw' --
    /// and comparing that verbatim makes a file outside the project look like it is inside it, with the leading
    /// '..' ending up in the namespace of the generated class.
    /// </remarks>
    public string GetNamespace(string resourceFilePath)
    {
        var directory = GetFullPath(Path.GetDirectoryName(resourceFilePath));

        if (string.IsNullOrEmpty(directory))
        {
            return RootNamespace;
        }

        var root = GetFullPath(ProjectRootPath)!.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (!(directory + Path.DirectorySeparatorChar).StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            return RootNamespace;
        }

        var relative = directory!.Substring(root.Length - 1)
            .Trim(Path.DirectorySeparatorChar)
            .Replace(Path.DirectorySeparatorChar, '.');

        return relative.Length == 0 ? RootNamespace : $"{RootNamespace}.{relative}";
    }

    /// <summary>
    /// Resolves a path, leaving it as it is when it cannot be resolved.
    /// </summary>
    /// <param name="path">The path to resolve.</param>
    /// <returns>The resolved path, or <see langword="null"/> when there is no path to resolve.</returns>
    /// <remarks>
    /// Resolving a path touches no file, but it does reject the ones that are malformed, and a resource file
    /// with an unusable path is not worth failing the whole generation over.
    /// </remarks>
    private static string? GetFullPath(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return path;
        }

        try
        {
            return Path.GetFullPath(path);
        }
        catch (Exception)
        {
            return path;
        }
    }

    /// <inheritdoc/>
    public bool Equals(ReswProject? other)
    {
        return other is not null
            && IsSupported == other.IsSupported
            && AppType == other.AppType
            && AssemblyName == other.AssemblyName
            && ProjectRootPath == other.ProjectRootPath
            && RootNamespace == other.RootNamespace
            && IsLibrary == other.IsLibrary
            && UseApplicationLanguages == other.UseApplicationLanguages
            && GenerateResourceInterfaces == other.GenerateResourceInterfaces
            && DefaultLanguage == other.DefaultLanguage
            && SetupProblems.Equals(other.SetupProblems);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ReswProject);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = 17;

        hash = (hash * 31) + IsSupported.GetHashCode();
        hash = (hash * 31) + AppType.GetHashCode();
        hash = (hash * 31) + AssemblyName.GetHashCode();
        hash = (hash * 31) + ProjectRootPath.GetHashCode();
        hash = (hash * 31) + RootNamespace.GetHashCode();
        hash = (hash * 31) + IsLibrary.GetHashCode();
        hash = (hash * 31) + UseApplicationLanguages.GetHashCode();
        hash = (hash * 31) + GenerateResourceInterfaces.GetHashCode();
        hash = (hash * 31) + (DefaultLanguage?.GetHashCode() ?? 0);

        return (hash * 31) + SetupProblems.GetHashCode();
    }
}
