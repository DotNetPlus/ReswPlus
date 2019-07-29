using ReswPlus.Core.CodeGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReswPlus.Core.ClassGenerator
{
    public class GenerationResult
    {
        public IEnumerable<GeneratedFile> Files { get; set; }
        public bool MustInstallReswPlusLib { get; set; }
    }
}
