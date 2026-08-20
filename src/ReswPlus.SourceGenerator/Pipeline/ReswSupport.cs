using System;
using System.Linq;

namespace ReswPlus.SourceGenerator.Pipeline;

/// <summary>
/// The support sources shared by every resource file of a project.
/// </summary>
/// <remarks>
/// Which of them are needed depends on what the resource files of the project use, so this can only be decided
/// once every file has been generated. It is deliberately reduced to a handful of flags and the languages of
/// the project: editing a string changes the code generated for its file, but almost never changes any of
/// this, so the support sources are emitted once and then left alone.
/// </remarks>
internal sealed class ReswSupport : IEquatable<ReswSupport>
{
    private ReswSupport(bool isSupported, AppType appType, bool needsMacros, bool needsPlurals, bool useApplicationLanguages, EquatableArray<NamedPath> languages)
    {
        IsSupported = isSupported;
        AppType = appType;
        NeedsMacros = needsMacros;
        NeedsPlurals = needsPlurals;
        UseApplicationLanguages = useApplicationLanguages;
        Languages = languages;
    }

    public bool IsSupported { get; }

    public AppType AppType { get; }

    public bool NeedsMacros { get; }

    public bool NeedsPlurals { get; }

    public bool UseApplicationLanguages { get; }

    /// <summary>
    /// Gets a resource file of each language of the project, so that a diagnostic about a language has
    /// somewhere to point.
    /// </summary>
    public EquatableArray<NamedPath> Languages { get; }

    /// <summary>
    /// Works out which support sources the project needs.
    /// </summary>
    /// <param name="generatedFiles">The code generated for each resource file of the project.</param>
    /// <param name="project">The project the code is generated for.</param>
    /// <param name="layout">How the resource files of the project are laid out.</param>
    /// <returns>The support the project needs.</returns>
    public static ReswSupport Create(System.Collections.Immutable.ImmutableArray<ReswGeneratedFile> generatedFiles, ReswProject project, ReswLayout layout)
    {
        if (!project.IsSupported)
        {
            return new ReswSupport(false, AppType.Unknown, false, false, false, new EquatableArray<NamedPath>([]));
        }

        return new ReswSupport(
            isSupported: true,
            project.AppType,
            generatedFiles.Any(file => file.ContainsMacro),
            generatedFiles.Any(file => file.ContainsPlural),
            project.UseApplicationLanguages,
            layout.Languages);
    }

    /// <inheritdoc/>
    public bool Equals(ReswSupport? other)
    {
        return other is not null
            && IsSupported == other.IsSupported
            && AppType == other.AppType
            && NeedsMacros == other.NeedsMacros
            && NeedsPlurals == other.NeedsPlurals
            && UseApplicationLanguages == other.UseApplicationLanguages
            && Languages.Equals(other.Languages);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ReswSupport);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = 17;

        hash = (hash * 31) + IsSupported.GetHashCode();
        hash = (hash * 31) + AppType.GetHashCode();
        hash = (hash * 31) + NeedsMacros.GetHashCode();
        hash = (hash * 31) + NeedsPlurals.GetHashCode();
        hash = (hash * 31) + UseApplicationLanguages.GetHashCode();

        return (hash * 31) + Languages.GetHashCode();
    }
}
