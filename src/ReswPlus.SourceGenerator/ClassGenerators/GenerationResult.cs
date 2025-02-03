using System.Collections.Generic;
using ReswPlus.SourceGenerator.CodeGenerators;

namespace ReswPlus.SourceGenerator.ClassGenerators;

/// <summary>
/// Represents the result of a generation process.
/// </summary>
internal sealed class GenerationResult
{
    /// <summary>
    /// Gets or sets the collection of generated files.
    /// </summary>
    public IEnumerable<GeneratedFile> Files { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the result contains plural forms.
    /// </summary>
    public bool ContainsPlural { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the result contains macros.
    /// </summary>
    public bool ContainsMacro { get; set; }

    public GenerationResult(IEnumerable<GeneratedFile> files)
    {
        Files = files;
    }
}
