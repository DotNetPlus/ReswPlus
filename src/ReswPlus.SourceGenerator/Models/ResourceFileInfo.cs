namespace ReswPlus.SourceGenerator.Models;

/// <summary>
/// Represents information about a resource file.
/// </summary>
internal sealed class ResourceFileInfo(string path, IProject parentProject)
{
    /// <summary>
    /// Gets the path of the resource file.
    /// </summary>
    public string Path { get; } = path;

    /// <summary>
    /// Gets the project that contains the resource file.
    /// </summary>
    public IProject Project { get; } = parentProject;
}
