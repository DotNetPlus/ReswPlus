using EnvDTE;
using ReswPlus.Core.ClassGenerator.Models;
using ReswPlus.Core.Resw;
using System.Collections.Generic;

namespace ReswPlus.Core.CodeGenerators
{
    public class GeneratedFile
    {
        public string Filename { get; set; }
        public string Content { get; set; }
    }

    public interface ICodeGenerator
    {
        IEnumerable<GeneratedFile> GetGeneratedFiles(string baseFilename, StronglyTypedClass info, ProjectItem projectItem);
    }
}
