using ReswPlus.Resw;
using System.Collections.Generic;

namespace ReswPlus.ClassGenerator.Models
{
    internal class LocalizationBase
    {
        public string Key { get; set; }
        public List<FormatTagParameter> Parameters { get; set; } = new List<FormatTagParameter>();
        public List<FunctionFormatTagParameter> ExtraParameters { get; } = new List<FunctionFormatTagParameter>();
        public string FormatSummary { get; set; }
        public bool IsDotNetFormatting { get; set; }
    }

    internal class Localization : LocalizationBase
    {
        public string AccessorSummary { get; set; }
    }

    internal class PluralLocalization : LocalizationBase
    {
        public bool SupportNoneState { get; set; }
        public FunctionFormatTagParameter ParameterToUseForPluralization { get; set; }
        public string TemplateAccessorSummary { get; set; }
    }

    internal interface IVariantLocalization
    {
        FunctionFormatTagParameter ParameterToUseForVariant { get; set; }
    }

    internal class PluralVariantLocalization : PluralLocalization, IVariantLocalization
    {
        public FunctionFormatTagParameter ParameterToUseForVariant { get; set; }
    }

    internal class VariantLocalization : LocalizationBase, IVariantLocalization
    {
        public string TemplateAccessorSummary { get; set; }
        public FunctionFormatTagParameter ParameterToUseForVariant { get; set; }
    }
}
