namespace ReswPlus.SourceGenerator.Models;

/// <summary>
/// Represents a project with a name and a flag indicating if it is a library.
/// </summary>
internal sealed class Project(string name, bool isLibrary) : IProject
{
    /// <summary>
    /// Gets a value indicating whether the project is a library.
    /// </summary>
    public bool IsLibrary { get; } = isLibrary;

    /// <summary>
    /// Gets the name of the project.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the language of the project, which is always C#.
    /// </summary>
    public Language Language => Language.CSharp;
}
