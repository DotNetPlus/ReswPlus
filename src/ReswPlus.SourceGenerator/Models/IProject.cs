namespace ReswPlus.SourceGenerator.Models;

/// <summary>
/// Represents a project with properties and methods to retrieve project-specific information.
/// </summary>
internal interface IProject
{
    /// <summary>
    /// Gets a value indicating whether the project is a library.
    /// </summary>
    bool IsLibrary { get; }

    /// <summary>
    /// Gets the name of the project.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the programming language of the project.
    /// </summary>
    Language Language { get; }

    /// <summary>
    /// Gets the precompiled header for the project.
    /// </summary>
    /// <returns>A string representing the precompiled header.</returns>
    string GetPrecompiledHeader();

    /// <summary>
    /// Gets the indent string used in the project.
    /// </summary>
    /// <returns>A string representing the indent.</returns>
    string GetIndentString();
}
