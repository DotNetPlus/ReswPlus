using ReswPlus.Resw;
using System.Collections.Generic;

namespace ReswPlus.ClassGenerator.Models
{
    internal class LocalizationBase
    {
        public string Key { get; set; }
        public List<FunctionParameter> Parameters { get; set; } = new List<FunctionParameter>();
        public string FormatSummary { get; set; }
    }

    internal class Localization : LocalizationBase
    {
        public string AccessorSummary { get; set; }
    }

    internal class PluralLocalization : LocalizationBase
    {
        public bool SupportNoneState { get; set; }
        public FunctionParameter ExtraParameterForPluralization { get; set; }
        public FunctionParameter ParameterToUseForPluralization { get; set; }
        public string TemplateAccessorSummary { get; set; }
    }
}
