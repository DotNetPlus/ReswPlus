using ReswPlus.Core.ClassGenerator.Models;
using ReswPlus.Core.ResourceInfo;
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
        IEnumerable<GeneratedFile> GetGeneratedFiles(string baseFilename, StronglyTypedClass info, ResourceFileInfo projectItem);
    }
}
