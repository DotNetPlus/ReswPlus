using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReswPlusCore.ClassGenerator.Models
{
    public class StronglyTypedClass
    {
        public bool IsAdvanced{ get; set; }
        public string[] Namespaces { get; set; }
        public string ResoureFile { get; set; }
        public string ClassName { get; set; }

        public List<Localization> Localizations { get; set; } = new List<Localization>();
    }
}
