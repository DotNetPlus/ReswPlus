using System.Collections.Generic;
using System.Linq;
using ReswPlus.Core.ResourceParser;

namespace ReswPlus.SourceGenerator.ClassGenerators.Models;

/// <summary>
/// Represents a base class for localization.
/// </summary>
internal abstract class Localization
{
    protected Localization(string key, string summary)
    {
        Key = key;
        Summary = summary;
    }

    /// <summary>
    /// Gets or sets the key for the localization.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets or sets the list of format tag parameters.
    /// </summary>
    public List<IFormatTagParameter> Parameters { get; set; } = [];

    /// <summary>
    /// Gets the list of extra function format tag parameters.
    /// </summary>
    public List<FunctionFormatTagParameter> ExtraParameters { get; } = [];

    /// <summary>
    /// Gets or sets the summary for the localization.
    /// </summary>
    public string Summary { get; }

    /// <summary>
    /// Gets or sets a value indicating whether .NET formatting is used.
    /// </summary>
    public bool IsDotNetFormatting { get; set; }

    /// <summary>
    /// Gets a value indicating whether the localization is a property.
    /// </summary>
    public bool IsProperty => !Parameters.OfType<FunctionFormatTagParameter>().Any() && !ExtraParameters.Any();
}

/// <summary>
/// Represents a regular localization.
/// </summary>
internal sealed class RegularLocalization : Localization
{
    public RegularLocalization(string key, string summary) : base(key, summary)
    {
    }
}

/// <summary>
/// Represents a plural localization.
/// </summary>
internal class PluralLocalization : Localization
{
    public PluralLocalization(string key, string summary) : base(key, summary)
    {
    }

    /// <summary>
    /// Gets or sets a value indicating whether the none state is supported.
    /// </summary>
    public bool SupportNoneState { get; set; }

    /// <summary>
    /// Gets or sets the parameter to use for pluralization.
    /// </summary>
    public FunctionFormatTagParameter? ParameterToUseForPluralization { get; set; }
}

/// <summary>
/// Represents an interface for variant localization.
/// </summary>
internal interface IVariantLocalization
{
    /// <summary>
    /// Gets or sets the parameter to use for variant.
    /// </summary>
    FunctionFormatTagParameter? ParameterToUseForVariant { get; set; }
}

/// <summary>
/// Represents a plural variant localization.
/// </summary>
internal sealed class PluralVariantLocalization : PluralLocalization, IVariantLocalization
{
    public PluralVariantLocalization(string key, string summary) : base(key, summary)
    {
    }

    /// <summary>
    /// Gets or sets the parameter to use for variant.
    /// </summary>
    public FunctionFormatTagParameter? ParameterToUseForVariant { get; set; }
}

/// <summary>
/// Represents a variant localization.
/// </summary>
internal sealed class VariantLocalization : Localization, IVariantLocalization
{
    public VariantLocalization(string key, string summary) : base(key, summary)
    {
    }

    /// <summary>
    /// Gets or sets the parameter to use for variant.
    /// </summary>
    public FunctionFormatTagParameter? ParameterToUseForVariant { get; set; }
}
