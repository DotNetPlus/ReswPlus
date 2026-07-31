namespace ReswPlus.SourceGenerator;

/// <summary>
/// The parts of a compilation the generation depends on.
/// </summary>
/// <remarks>
/// A <c>Compilation</c> is a different object after every edit, so keeping one in the incremental pipeline would
/// invalidate every downstream step on every keystroke. Projecting it to this equatable model instead means the
/// pipeline is only invalidated when something the generation actually reads has changed.
/// </remarks>
internal sealed record CompilationInfo
{
    public CompilationInfo(bool isCSharp, AppType appType, string? assemblyName)
    {
        IsCSharp = isCSharp;
        AppType = appType;
        AssemblyName = assemblyName;
    }

    /// <summary>
    /// Gets whether the compilation is a C# compilation, the only language the generator supports.
    /// </summary>
    public bool IsCSharp { get; }

    /// <summary>
    /// Gets the type of application being built, determined from the references of the compilation.
    /// </summary>
    public AppType AppType { get; }

    /// <summary>
    /// Gets the name of the assembly being built, used to build the resource identifier of a library.
    /// </summary>
    public string? AssemblyName { get; }
}
