using ReswPlus.Resw;
using System.Collections.Generic;

namespace ReswPlus.ClassGenerator.Models
{
    internal class LocalizationBase
    {
        public string Key { get; set; }
        public List<FunctionParameter> Parameters { get; set; } = new List<FunctionParameter>();
        public List<FunctionParameter> ExtraParameters { get; } = new List<FunctionParameter>();
        public string FormatSummary { get; set; }
    }

    internal class Localization : LocalizationBase
    {
        public string AccessorSummary { get; set; }
    }

    internal class PluralLocalization : LocalizationBase
    {
        public bool SupportNoneState { get; set; }
        public FunctionParameter ParameterToUseForPluralization { get; set; }
        public string TemplateAccessorSummary { get; set; }
    }

    internal interface IVariantLocalization
    {
        FunctionParameter ParameterToUseForVariant { get; set; }
    }

    internal class PluralVariantLocalization : PluralLocalization, IVariantLocalization
    {
        public FunctionParameter ParameterToUseForVariant { get; set; }
    }

    internal class VariantLocalization : LocalizationBase, IVariantLocalization
    {
        public string TemplateAccessorSummary { get; set; }
        public FunctionParameter ParameterToUseForVariant { get; set; }
    }
}
