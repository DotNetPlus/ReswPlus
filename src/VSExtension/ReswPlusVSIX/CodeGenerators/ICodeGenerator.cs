using EnvDTE;
using ReswPlus.ClassGenerator.Models;
using ReswPlus.Resw;
using System.Collections.Generic;

namespace ReswPlus.CodeGenerators
{
    internal class GeneratedFile
    {
        public string Filename { get; set; }
        public string Content { get; set; }
    }

    internal interface ICodeGenerator
    {
        IEnumerable<GeneratedFile> GetGeneratedFiles(string baseFilename, StronglyTypedClass info, ProjectItem projectItem);
    }
}
