using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReswPlus.ClassGenerator.Models
{
    class StronglyTypedClass
    {
        public bool SupportPluralization{ get; set; }
        public string[] Namespace { get; set; }
        public string ResoureFile { get; set; }
        public string ClassName { get; set; }

        public List<LocalizationBase> Localizations { get; set; } = new List<LocalizationBase>();
    }
}
