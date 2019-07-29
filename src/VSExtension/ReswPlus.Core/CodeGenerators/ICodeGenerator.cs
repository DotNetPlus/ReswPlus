using EnvDTE;
using ReswPlusCore.ClassGenerator.Models;
using ReswPlusCore.Resw;
using System.Collections.Generic;

namespace ReswPlusCore.CodeGenerators
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
