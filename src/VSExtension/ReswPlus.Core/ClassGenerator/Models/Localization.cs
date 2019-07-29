using ReswPlus.Core.Resw;
using System.Collections.Generic;
using System.Linq;

namespace ReswPlus.Core.ClassGenerator.Models
{
    public abstract class Localization
    {
        public string Key { get; set; }
        public List<FormatTagParameter> Parameters { get; set; } = new List<FormatTagParameter>();
        public List<FunctionFormatTagParameter> ExtraParameters { get; } = new List<FunctionFormatTagParameter>();
        public string Summary { get; set; }
        public bool IsDotNetFormatting { get; set; }
        public bool IsProperty => !Parameters.OfType<FunctionFormatTagParameter>().Any() && !ExtraParameters.Any();
    }

    public class RegularLocalization : Localization
    { }

    public class PluralLocalization : Localization
    {
        public bool SupportNoneState { get; set; }
        public FunctionFormatTagParameter ParameterToUseForPluralization { get; set; }
    }

    public interface IVariantLocalization
    {
        FunctionFormatTagParameter ParameterToUseForVariant { get; set; }
    }

    public class PluralVariantLocalization : PluralLocalization, IVariantLocalization
    {
        public FunctionFormatTagParameter ParameterToUseForVariant { get; set; }
    }

    public class VariantLocalization : Localization, IVariantLocalization
    {
        public FunctionFormatTagParameter ParameterToUseForVariant { get; set; }
    }
}
