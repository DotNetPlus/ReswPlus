using System.Collections.Generic;
using ReswPlus.SourceGenerator.ClassGenerators.Models;
using ReswPlus.SourceGenerator.Models;

namespace ReswPlus.SourceGenerator.CodeGenerators;

/// <summary>
/// Represents a generated file with its filename, content, and supported languages.
/// </summary>
internal sealed class GeneratedFile
{
    public GeneratedFile(string filename, string content)
    {
        Filename = filename;
        Content = content;
    }

    /// <summary>
    /// Gets or sets the filename of the generated file.
    /// </summary>
    public string Filename { get; }

    /// <summary>
    /// Gets or sets the content of the generated file.
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// Gets or sets the languages supported by the generated file.
    /// </summary>
    public string[]? Languages { get; set; }
}

/// <summary>
/// Defines a method to generate files based on the provided information.
/// </summary>
internal interface ICodeGenerator
{
    /// <summary>
    /// Generates a collection of files based on the provided base filename, strongly typed class information, and resource file information.
    /// </summary>
    /// <param name="baseFilename">The base filename for the generated files.</param>
    /// <param name="info">The strongly typed class information.</param>
    /// <param name="projectItem">The resource file information.</param>
    /// <returns>A collection of generated files.</returns>
    IEnumerable<GeneratedFile> GetGeneratedFiles(string baseFilename, StronglyTypedClass info, ResourceFileInfo projectItem);
}
